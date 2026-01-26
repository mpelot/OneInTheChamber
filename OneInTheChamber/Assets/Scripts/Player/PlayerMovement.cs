using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Movement
    [Header("Movement")]
    [SerializeField] private float acceleration;
    [SerializeField] private float maxRunSpeedX;
    [SerializeField] private Vector2 trueMaxSpeed;
    [SerializeField] private enum State {AIR, GROUND, WALL};
    [SerializeField] private State currentState = State.AIR;

    //Jumping
    [Header("Jumping")]
    [SerializeField] private float coyoteTimeLength;
    [SerializeField] private float defaultGravity;
    [SerializeField] private float fallingAccel;
    [SerializeField] private float holdingForwardBufferLength;
    [SerializeField] private float inputBufferLength;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldownLength;
    [SerializeField] private float longJumpGravity;
    [SerializeField] private float maxGravity;
    [SerializeField] private float wallGravity;
    [SerializeField] private float wallSpeedDecay;
    [SerializeField] private float wallSpeedLoss;
    [SerializeField] private float wallSplatTime;
    [SerializeField] private float wallStickTime;
    [SerializeField] private float wallThreshhold;
    [SerializeField] private float yBlastTime;

    //Firing
    [Header("Firing")]
    [SerializeField] private float blastForce;
    [SerializeField] private float bulletTimeLength;
    [SerializeField] private float blastCooldownTime;
    [SerializeField] private float bulletTimeSlowdownFactor;
    [SerializeField] public bool canBlast = true;
    [SerializeField] private bool canShoot = true;

    // Ground Detection
    [Header("Ground Detection")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask groundAndPlatLayer;

    // References
    [Header("References")]
    [SerializeField] private LayerMask target;
    [SerializeField] private ParticleSystem blastTrail;
    [SerializeField] private ParticleSystem jumpDust;
    [SerializeField] private ParticleSystem wallJumpDust;
    [SerializeField] private ParticleSystem landDust;
    [SerializeField] public ParticleSystem slideDust;
    [SerializeField] private GameObject blast;
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private Animator ssAnimator;
    [SerializeField] private Animator aimEffectAnimator;
    [SerializeField] private SpriteRenderer tint;
    [SerializeField] private LineRenderer laserGuide;


    private bool facingRight = true;
    private bool coyoteTime = false;
    private bool holdingForward = false;
    private bool sliding = false;
    private bool longJump = false;
    private bool aiming = false;
    private float coyoteTimer;
    private float inputBufferTimer;
    private float holdingForwardBufferTimer;
    private float jumpCooldownTimer;
    private float wallSplatTimer;
    private float wallStickTimer;
    private float yBlastTimer;
    private float bulletTimeTimer;
    private float blastCoolDownTimer;
    private float currentMaxSpeedX;
    private float accelValue;
    private float fastFallModifier;
    private float lastSpeedX;
    [HideInInspector] public Vector2 lastSpeed;
    private Vector2 goalSpeed;
    [HideInInspector] public Vector2 platformVelocity;
    private Rigidbody2D rbody;
    //private LineRenderer laserGuide;
    private Animator animator;
    private BoxCollider2D playerCollider;
    private Camera mainCam;

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        //laserGuide = GetComponent<LineRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
        accelValue = acceleration;
        mainCam = Camera.main;
        fastFallModifier = 1;
        currentState = State.AIR;
    }

    private void Update()
    {
        animator.SetBool("Down Blast", false);
        animator.SetBool("Up Blast", false);
        animator.SetBool("BW Blast", false);
        animator.SetBool("FW Blast", false);
        ssAnimator.SetBool("Squash", false);
        ssAnimator.SetBool("Stretch", false);
        aimEffectAnimator.SetBool("AimEffect", false);
        aimEffectAnimator.SetBool("AimCancel", false);

        // Update the speed parameter in the animator
        float platformOffset = platformVelocity.x != 0 ? .1f : 0f;
        animator.SetFloat("Horizontal Speed", Mathf.Clamp(Mathf.Ceil(Mathf.Abs(rbody.velocity.x - platformVelocity.x) - platformOffset) + 1f, -1, 5));

        // Update the vertical velocity parameter in the animator
        animator.SetFloat("Vertical Velocity", Mathf.Clamp(rbody.velocity.y - platformVelocity.y, -5, 5));

        // If Turning
        if (((goalSpeed.x < 0 && facingRight) || (goalSpeed.x > 0 && !facingRight)) && currentState != State.WALL)
        {
            Turn();
        }

        // Update the moving backwards paramater in the animator
        animator.SetBool("Moving Backwards", transform.localScale.x * rbody.velocity.x < 0);
        
        // Calulate if the player is holding in the direction they are facing with input buffering
        if ((facingRight && goalSpeed.x > 0) || (!facingRight && goalSpeed.x < 0))
        {
            holdingForward = true;
            holdingForwardBufferTimer = holdingForwardBufferLength;
        }
        if (holdingForwardBufferTimer <= 0f)
            holdingForward = false;

        // Player States Input-Based Calculations
        // IN AIR
        if (currentState == State.AIR)
        {
            // If Jumping
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // If Coyote Timer Is Active, Jump
                if (coyoteTimer > 0 && coyoteTime)
                {
                    Jump();
                }
                else
                {
                    inputBufferTimer = inputBufferLength;
                }
            }

            // Enable Fast Falling
            if (Input.GetKey(KeyCode.S))
            {
                fastFallModifier = 1.2f;
                longJump = false;
            }

            // Disable Long Jump
            if (Input.GetKeyUp(KeyCode.Space) || rbody.velocity.y <= 0)
            {
                longJump = false;
            }

        }
        // ON GROUND
        else if (currentState == State.GROUND)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (!sliding)
                {
                    Slide();
                }
            }
            else
            {
                StopSlide();
            }
        }
        // ON WALL
        else if (currentState == State.WALL)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                WallJump();
            }
        }

        // Blasting
        if (Input.GetMouseButtonDown(0) && canBlast && blastCoolDownTimer <= 0)
        {
            Blast();
        }

        // Shooting
        if (Input.GetMouseButtonDown(1) && !aiming && canShoot)
        {
            Aim();
        }

        if (aiming) 
        {
            if (!facingRight)
                Turn();
            if (Input.GetMouseButtonUp(1))
                AimCancel();
            else if (bulletTimeTimer < 0)
                Shoot();
            else
            {
                rbody.velocity = Vector2.MoveTowards(rbody.velocity, Vector2.zero, 20f * Time.fixedDeltaTime);
                Vector2 laserDirection = getVectorFromPlayerToMouse();
                animator.SetFloat("Cosine", Mathf.Cos(Vector2.Angle(Vector2.right, laserDirection) * Mathf.Deg2Rad));
                spriteTransform.rotation = Quaternion.LookRotation(Vector3.forward, laserDirection) * Quaternion.Euler(0, 0, 90);
                //tint.color = new Color(tint.color.r, tint.color.g, tint.color.b, Mathf.Clamp(tint.color.a + Time.unscaledDeltaTime, 0f, .2f));
                //SetLaserDirection(laserDirection);
                //laserGuide.enabled = true;
            }
        } 
       /* else if (tint.color.a > 0)
            tint.color = new Color(tint.color.r, tint.color.g, tint.color.b, Mathf.Clamp(tint.color.a - Time.unscaledDeltaTime, 0f, .2f));*/
    }

    void FixedUpdate()
    {
        lastSpeed = rbody.velocity;

        ssAnimator.SetBool("Land", false);
        ssAnimator.SetBool("WallSplat", false);
        ssAnimator.SetBool("Neutral", false);
        // === PLAYER STATES ===
        // IN AIR
        if (currentState == State.AIR)
        {
            // Restrict Horizontal Movement In Air
            accelValue = .7f * acceleration;

            // If B-Hopping, Change Max Speed settings to keep current momentum
            if (Mathf.Abs(rbody.velocity.x) > maxRunSpeedX && Mathf.Sign(rbody.velocity.x) == Mathf.Sign(Input.GetAxisRaw("Horizontal")))
            {
                currentMaxSpeedX = Mathf.Abs(rbody.velocity.x);
            }
            else
            {
                currentMaxSpeedX = maxRunSpeedX;
            }

            // Variable Jump Height
            if (longJump == false && rbody.gravityScale <= maxGravity)
            {
                // If Short Jump or Ended Long Jump
                rbody.gravityScale = fallingAccel * fastFallModifier;
            }
            else if (longJump == true && rbody.velocity.y >= 0)
            {
                // If Long Jumping
                rbody.gravityScale = longJumpGravity;
            }

            // Check for partial ceilings and correct position
            if (rbody.velocity.y > 1f && !isOnWall())
            {
                bool ll = Physics2D.Raycast(new Vector2(transform.position.x - .20834f, transform.position.y + .4f), Vector2.up, .1f, groundLayer);
                bool l = Physics2D.Raycast(new Vector2(transform.position.x - .0833f, transform.position.y + .4f), Vector2.up, .1f, groundLayer);
                bool r = Physics2D.Raycast(new Vector2(transform.position.x + .0833f, transform.position.y + .4f), Vector2.up, .1f, groundLayer);
                bool rr = Physics2D.Raycast(new Vector2(transform.position.x + .20834f, transform.position.y + .4f), Vector2.up, .1f, groundLayer);
                Vector2 side = Vector2.zero;
                if (ll && !l && !rr)
                    side = Vector2.left;
                if (rr && !r && !ll)
                    side = Vector2.right;
                if (side != Vector2.zero)
                {
                    RaycastHit2D[] wall = new RaycastHit2D[1];
                    ContactFilter2D filter = new ContactFilter2D();
                    filter.layerMask = groundLayer;
                    Physics2D.Raycast(new Vector2(transform.position.x + .0833f * side.x, transform.position.y + .6f), side, filter, wall);
                    transform.position = new Vector3(transform.position.x + (.16f - wall[0].distance) * -side.x, transform.position.y, 0f);
                }
            }

            // Check for partial walls and correct position
            Vector2 dir = facingRight ? Vector2.right : Vector2.left;
            if (rbody.velocity.x * dir.x > 0f && rbody.velocity.y < 2f)
            {
                bool u = Physics2D.Raycast(new Vector2(transform.position.x + .3f * dir.x, transform.position.y - .25f), dir, .1f, groundLayer);
                bool d = Physics2D.Raycast(new Vector2(transform.position.x + .3f * dir.x, transform.position.y - .499f), dir, .1f, groundLayer);
                if (d && !u)
                {
                    RaycastHit2D[] wall = new RaycastHit2D[1];
                    RaycastHit2D[] floor = new RaycastHit2D[1];
                    ContactFilter2D filter = new ContactFilter2D();
                    filter.layerMask = groundLayer;
                    Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - .6f), dir, filter, wall);
                    Physics2D.Raycast(new Vector2(transform.position.x + .5f * dir.x, transform.position.y - .25f), Vector2.down, filter, floor);
                    if (inputBufferTimer <= 0)
                        animator.SetBool("Grounded", true);
                    transform.position = new Vector3(transform.position.x + (wall[0].distance - .2f) * dir.x, transform.position.y + .285f - floor[0].distance, 0f);
                    rbody.velocity = new Vector2(rbody.velocity.x, 0f);
                }
            }

            bool groundCheck = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - .5f), Vector2.down, .1f, groundLayer);
            //GROUND Transtion
            if (isGrounded() && !(isOnWall() && !groundCheck))
            {
                canBlast = true;
                canShoot = true;
                coyoteTime = true;
                currentState = State.GROUND;
                rbody.gravityScale = defaultGravity;
                animator.SetBool("Grounded", true);
                ssAnimator.SetBool("Land", true);
                landDust.Play();
                AudioManager.instance.PlaySFX("Land");
            }
            //WallCling Transition
            else if(isOnWall() && holdingForward && Mathf.Abs(rbody.velocity.x) < 0.1f)
            {
                currentState = State.WALL;
                wallStickTimer = wallStickTime;
                animator.SetBool("Wallclinging", true);
                if (rbody.velocity.y > trueMaxSpeed.y)
                    rbody.velocity = new Vector2(rbody.velocity.x, trueMaxSpeed.y);
                if (Mathf.Abs(lastSpeedX) > maxRunSpeedX)
                {
                    ssAnimator.SetBool("WallSplat", true);
                    wallSplatTimer = wallSplatTime;
                    if (yBlastTimer <= 0f)
                    {
                        rbody.gravityScale = 0f;
                        rbody.velocity = Vector2.zero;
                    }

                    if (Mathf.Abs(lastSpeedX) > maxRunSpeedX * 1.5f)
                    {
                        AudioManager.instance.PlaySFX("Splat");
                    }
                } 
            }
            else if(!(isOnWall() && holdingForward))
            {
                lastSpeedX = rbody.velocity.x;
            }
        }

        // ON GROUND
        else if (currentState == State.GROUND)
        {
            if (inputBufferTimer > 0)
            {
                Jump();
            }
            else if (sliding)
            {
                accelValue = 0.5f * acceleration;
            }
            // If Decelerating
            else if (goalSpeed.magnitude < rbody.velocity.magnitude || Mathf.Sign(goalSpeed.x) != Mathf.Sign(rbody.velocity.x))
            {
                accelValue = 1.75f * acceleration;
            }
            // If Accelerating
            else
            {
                accelValue = acceleration;
            }
            currentMaxSpeedX = maxRunSpeedX;
            canBlast = true;
            canShoot = true;
            lastSpeedX = rbody.velocity.x;
            //Air Transition
            if (isGrounded() == false)
            {
                currentState = State.AIR;
                sliding = false;
                coyoteTimer = coyoteTimeLength;
                blastCoolDownTimer = 0;
                animator.SetBool("Grounded", false);
                slideDust.Stop();
            }
            //Cannot Transition To WallCling
        }

        // WALL CLING
        else if (currentState == State.WALL)
        {
            if (holdingForward)
            {
                wallStickTimer = wallStickTime;
            }
            if (inputBufferTimer > 0)
            {
                WallJump();
            }
            if (wallSplatTimer <= 0 && rbody.velocity.y <= 0f)
            {
                rbody.gravityScale = wallGravity;
                lastSpeedX = 0;
            }
                
            //AIR Transition - Jumped Off Located In WallJump()
            //AIR Transition - Shot Located In Shooting()
            //AIR Transition - Let Go
            if (wallStickTimer <= 0)
            {
                currentState = State.AIR;
                animator.SetBool("Wallclinging", false);
                ssAnimator.SetBool("Neutral", true);
                Turn();
            }
            //AIR Transition - Slid Off
            else if(!isOnWall())
            {
                currentState = State.AIR;
                animator.SetBool("Wallclinging", false);
                ssAnimator.SetBool("Neutral", true);
                wallStickTimer = 0;
                Turn();
            }
            //GROUND Transition
            else if(isGrounded())
            {
                coyoteTime = true;
                wallStickTimer = 0;
                currentState = State.GROUND;
                animator.SetBool("Wallclinging", false);
                animator.SetBool("Grounded", true);
            }
        }

        // Horizontal Speed for air and ground
        goalSpeed = new Vector2(Input.GetAxisRaw("Horizontal") * currentMaxSpeedX, rbody.velocity.y);
        if (currentState != State.WALL)
        {
            rbody.velocity = Vector2.MoveTowards(rbody.velocity - platformVelocity, goalSpeed, accelValue * Time.fixedDeltaTime) + platformVelocity;
        }

        // Update animation parameter
        animator.SetFloat("Horizontal Input", Mathf.Abs(goalSpeed.x));

        // Decrements all of the timers
        if (coyoteTimer > 0)
            coyoteTimer -= Time.fixedDeltaTime;
        if (inputBufferTimer > 0)
            inputBufferTimer -= Time.fixedDeltaTime;
        if (holdingForwardBufferTimer > 0)
            holdingForwardBufferTimer -= Time.fixedDeltaTime;
        if (jumpCooldownTimer > 0)
            jumpCooldownTimer -= Time.fixedDeltaTime;
        if (wallStickTimer > 0)
            wallStickTimer -= Time.fixedDeltaTime;
        if (wallSplatTimer > 0)
            wallSplatTimer -= Time.fixedDeltaTime;
        if (yBlastTimer > 0)
            yBlastTimer -= Time.fixedDeltaTime;
        if (blastCoolDownTimer > 0)
            blastCoolDownTimer -= Time.fixedDeltaTime;
        if (bulletTimeTimer > 0)
            bulletTimeTimer -= Time.fixedUnscaledDeltaTime;  // Time has to be unscaled here or it will be affected by the slowdown
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0f, Vector2.down, .02f, groundAndPlatLayer);
    }
    private bool isOnWall()
    {
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        return Physics2D.BoxCast(playerCollider.bounds.center, new Vector2(playerCollider.bounds.size.x, .7f) , 0f, dir, .04f, groundLayer);
    }

    private void Turn()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void Jump()
    {
        rbody.velocity = new Vector2(rbody.velocity.x, jumpForce);
        coyoteTimer = 0;
        inputBufferTimer = 0;
        jumpCooldownTimer = jumpCooldownLength;
        // Long Jump Must Have Space Held Down (In case using Input Buffering)
        longJump = Input.GetKey(KeyCode.Space);
        fastFallModifier = 1;
        animator.SetBool("Grounded", false);   // Just trust me bro
        ssAnimator.SetBool("Stretch", true);
        jumpDust.Play();
        AudioManager.instance.PlaySFX("Jump");
    }

    private void WallJump()
    {
        // If still splatting on wall, perform a super wall jump, otherwise perform a regular one
        float magnitude = wallSplatTimer > 0 ? Mathf.Abs(lastSpeedX) : maxRunSpeedX;

        if (facingRight)
            magnitude = magnitude * -1;

        rbody.velocity = new Vector2(magnitude, jumpForce);
        coyoteTimer = 0;
        jumpCooldownTimer = jumpCooldownLength;
        wallJumpDust.Play();

        // Long Jump Must Have Space Held Down (In case using Input Buffering)
        longJump = Input.GetKey(KeyCode.Space);
        fastFallModifier = 1;

        //AIR Transition - Jumped Off
        wallStickTimer = 0;
        wallSplatTimer = 0;
        animator.SetBool("Wallclinging", false);
        currentState = State.AIR;
        Turn();
        
        AudioManager.instance.PlaySFX("Jump");
    }

    private void Slide()
    {
        rbody.velocity = new Vector2(maxRunSpeedX * transform.localScale.x, rbody.velocity.y);
        sliding = true;
    }

    private void StopSlide()
    {
        sliding = false;
    }

    public void Bounce(float strength)
    {
        if (currentState != State.WALL)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + .3f, 0f);
            rbody.velocity = new Vector2(rbody.velocity.x, strength);
            canBlast = true;
            coyoteTime = false;
            animator.SetBool("Grounded", false);
            AudioManager.instance.PlaySFX("Bounce");
        }
    }

    private void Blast()
    {
        canBlast = false;
        bool ignore = false;
        rbody.gravityScale = defaultGravity;

        if (currentState == State.GROUND)
        {
            blastCoolDownTimer = blastCooldownTime;
        }

        if (currentState == State.WALL)
        {
            currentState = State.AIR;
            animator.SetBool("Wallclinging", false);
            wallStickTimer = 0;
            wallSplatTimer = 0;
            lastSpeedX = 0;
            Turn();
        }

        Vector2 blastDirection = getVectorFromPlayerToMouse();

        // Confine blast directions to 4
        blastDirection = Mathf.Abs(blastDirection.x) > Mathf.Abs(blastDirection.y) ? Vector2.right * Mathf.Sign(blastDirection.x) : Vector2.up * Mathf.Sign(blastDirection.y);

        Vector2 newVelocity = rbody.velocity + (-blastDirection * blastForce);
        if (blastDirection.x == 0)
        {
            if (blastDirection.y < 0)
            {
                // Minimum y velocity after a downwards blast
                float minY = 6f;
                if (rbody.velocity.y + blastForce < minY)
                    newVelocity = new Vector2(rbody.velocity.x, minY);
                longJump = false;
                yBlastTimer = yBlastTime;
                animator.SetBool("Grounded", false);
                animator.SetBool("Down Blast", true);
                ssAnimator.SetBool("Stretch", true);
            }
            else if (currentState == State.AIR)
            {
                // Maximum y velocity after an upwards blast
                float maxY = -8.3f;
                if (newVelocity.y > maxY)
                    newVelocity = new Vector2(rbody.velocity.x, maxY);
                coyoteTimer = 0;
                jumpCooldownTimer = coyoteTimeLength;
                animator.SetBool("Up Blast", true);
                ssAnimator.SetBool("Stretch", true);
                yBlastTimer = yBlastTime;
            }
            else
            {
                ignore = true;
            }
        }
        else
        {
            if (facingRight && blastDirection.x < 0 || !facingRight && blastDirection.x > 0)
                animator.SetBool("BW Blast", true);
            else
                animator.SetBool("FW Blast", true);
            if (currentState == State.AIR)
                ssAnimator.SetBool("Squash", true);
        }

        if (!ignore)
        {
            float y = rbody.velocity.y > trueMaxSpeed.y && blastDirection != Vector2.up ? rbody.velocity.y : Mathf.Clamp(newVelocity.y, -trueMaxSpeed.y, trueMaxSpeed.y);
            rbody.velocity = new Vector2(Mathf.Clamp(newVelocity.x, -trueMaxSpeed.x, trueMaxSpeed.x), y);

            Vector3 blastPos = new Vector3(transform.position.x + blastDirection.x * .5f, transform.position.y + blastDirection.y * .5f, 0);
            Instantiate(blast, blastPos, Quaternion.identity);

            // Play particle effect
            blastTrail.Play();

            AudioManager.instance.PlaySFX("Blast");
        }
    }

    private void Aim()
    {
        currentState = State.AIR;
        animator.SetBool("Wallclinging", false);
        animator.SetBool("Aiming", true);
        aimEffectAnimator.SetBool("AimEffect", true);

        aiming = true;
        canBlast = false;

        wallStickTimer = 0;
        bulletTimeTimer = bulletTimeLength;
        Time.timeScale = bulletTimeSlowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * .02f; // The default
    }

    private void AimCancel()
    {
        aimEffectAnimator.SetBool("AimCancel", true);
        bulletTimeTimer = -1;
        aiming = false;
        canShoot = false;
        animator.SetBool("Aiming", false);

        spriteTransform.rotation = Quaternion.identity;

        laserGuide.enabled = false;
        bulletTimeTimer = 0;
        Time.timeScale = 1;
        Time.fixedDeltaTime = .02f;  // The default
    }

    private void SetLaserDirection(Vector2 aimDirection)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDirection, 100f, LayerMask.GetMask("Ground"));
        laserGuide.positionCount = 2;
        laserGuide.SetPosition(0, transform.position);
        laserGuide.SetPosition(1, hit.point);
    }

    private void Shoot()
    {
        aiming = false;
        canShoot = false;
        animator.SetBool("Aiming", false);

        Vector2 laserDirection = getVectorFromPlayerToMouse();
        if (laserDirection.x < 0)
            spriteTransform.localScale = new Vector3(1, -1, 1);

        SetLaserDirection(laserDirection);
        laserGuide.enabled = true;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, laserDirection, 1000, target);
        if (hit.collider != null && hit.collider.gameObject.tag == "Target")
        {
            hit.collider.gameObject.GetComponent<Target>().Shatter();
        } 
        else
        {
            FindObjectOfType<LevelManager>().Lose();
            //laserGuide.enabled = false;
            bulletTimeTimer = 0;
            Time.timeScale = 1;
            Time.fixedDeltaTime = .02f;  // The default
            AudioManager.instance.PlaySFX("Laser Blast");
        }
    }

    private Vector2 getVectorFromPlayerToMouse()
    {
        // NOTE: mouse position is in screenspace!
        // We must normalize into worldspace before we can use these coords.
        Vector3 mousePos = Input.mousePosition;
        // z needs to be nonzero for this to work
        mousePos.z = mainCam.nearClipPlane;

        Vector3 norm = mainCam.ScreenToWorldPoint(mousePos);
        // Revert z transform
        norm.z = 0;

        return (norm - transform.position).normalized;
    }
}
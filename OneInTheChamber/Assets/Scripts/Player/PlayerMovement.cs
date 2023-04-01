using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LaserGuide))]
public class PlayerMovement : MonoBehaviour
{
    //Movement
    [Header("Movement")]
    public float acceleration;
    public float maxRunSpeed;
    public float trueMaxSpeed;
    public enum State { inAir, onGround, wallCling };
    public State playerState;

    //Jumping
    [Header("Jumping")]
    public float jumpForce;
    public float coyoteTimeLength;
    public float inputBufferLength;
    public float jumpCooldownLength;
    public float defaultGravity;
    public float longJumpGravity;
    public float fallingAccel;
    public float maxGravity;
    public float wallStickTime;
    public float wallGravity;

    //Firing
    [Header("Firing")]
    public GameObject bulletPrefab;
    public float bulletForce;
    public float bulletTimeLength;
    public float bulletTimeSlowdownFactor;
    public Animator bulletTimeIndicatorAnimator;
    public bool canFire = true;
    LaserGuide laserGuide;
    public ParticleSystem blast;

    // Ground Detection
    [Header("Ground Detection")]
    [SerializeField] BoxCollider2D coll;
    [SerializeField] LayerMask groundLayer;

    private Rigidbody2D rbody;
    private Animator animator;
    private bool facingRight = true;
    private float coyoteTimer = 0;
    private float inputBufferTimer = 0;
    private float jumpCooldownTimer = 0;
    private float wallStickTimer = 0;
    private bool holdingForward = false;
    private float bulletTimeTimer = 0;
    private float currentMaxSpeed;
    private float accelValue;
    private Vector2 goalSpeed;
    private bool longJump = false;
    private float fastFallModifier;
    private bool shooting = false;
    private bool blasting = false;
    private Camera mainCam;

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        laserGuide = GetComponent<LaserGuide>();
        accelValue = acceleration;
        mainCam = Camera.main;
        fastFallModifier = 1;
        playerState = State.inAir;
    }

    private void Update()
    {
        // Update the speed parameter in the animator
        animator.SetFloat("Horizontal Speed", Mathf.Clamp(Mathf.Ceil(Mathf.Abs(rbody.velocity.x)) + 1, -1, 5));

        // Update the vertical velocity parameter in the animator
        animator.SetFloat("Vertical Velocity", Mathf.Clamp(rbody.velocity.y, -5, 5));

        // If Turning
        if (((goalSpeed.x < 0 && facingRight) || (goalSpeed.x > 0 && !facingRight)) && playerState != State.wallCling)
        {
            Flip();
        }

        // Update the moving backwards paramater in the animator
        if (transform.localScale.x * rbody.velocity.x < 0)
        {
            animator.SetBool("Moving Backwards", true);
        }
        else
        {
            animator.SetBool("Moving Backwards", false);
        }
        
        // Calulate if the player is holding in the direction they are facing
        holdingForward = facingRight ? goalSpeed.x > 0 : goalSpeed.x < 0;

        // Determine Current State
        if (isGrounded() && playerState != State.onGround)  // If landing:
        {
            canFire = true;
            animator.SetBool("Wallclinging", false);

            // Reset WallStickTimer
            wallStickTimer = 0;

            // Set Gravity Back To Normal
            rbody.gravityScale = defaultGravity;

            // If wallclinging, flip player to match animation
            if (playerState == State.wallCling)
                Flip();

            // Input Buffer Jumping
            if (inputBufferTimer > 0)
            {
                Jump();
            }
            else
            {
                playerState = State.onGround;
                animator.SetBool("Grounded", true);
            }
        }
        else if (!isGrounded() && isOnWall() && holdingForward) // If clinging to wall
        {
            // Start WallStickTimer
            wallStickTimer = wallStickTime;
            if (playerState != State.wallCling)  // If first contact with wall:
            {
                rbody.gravityScale = defaultGravity;
                animator.SetBool("Grounded", false);

                // Input Buffer Jumping
                if (inputBufferTimer > 0)
                {
                    WallJump();
                }
                else
                {
                    playerState = State.wallCling;
                    animator.SetBool("Wallclinging", true);
                }
            }
        }
        else if (!isGrounded() && !(isOnWall() && wallStickTimer > 0) && playerState != State.inAir)  // If exiting from a surface:
        {
            // Update the grounded parameter in the animator
            animator.SetBool("Grounded", false);
            animator.SetBool("Wallclinging", false);
            wallStickTimer = 0;

            // If wallclinging, flip player to match animation
            if (playerState == State.wallCling)
                Flip();

            // Start Coyote Time Timer
            if (jumpCooldownTimer <= 0 && playerState == State.onGround)
            {
                coyoteTimer = coyoteTimeLength;
            }
            playerState = State.inAir;
        }


        // Player States
        // IN AIR
        if (playerState == State.inAir)
        {
            // Restrict Horizontal Movement In Air
            accelValue = .7f * acceleration;

            // If Jumping
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // If Coyote Timer Is Active, Jump
                if (coyoteTimer > 0)
                {
                    Jump();
                }
                else
                {
                    // Sets Input Buffer Timer
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

        // ON GROUND (And Jumping)
        else if (playerState == State.onGround && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // ON WALL
        else if (playerState == State.wallCling)
        {
            if (rbody.velocity.y < -2f)
            {
                rbody.velocity = new Vector2(rbody.velocity.x, -2f);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                WallJump();
            }
        }

        // Blasting
        if (Input.GetMouseButtonDown(0) && canFire)
        {
            blasting = true;
            canFire = false;
        }

        // Shooting
        if (Input.GetMouseButtonDown(1) && canFire)
        {
            shooting = true;
            canFire = false;

            // Start bullet time timer
            bulletTimeTimer = bulletTimeLength;

            // Set time scale to the slowdown factor
            Time.timeScale = bulletTimeSlowdownFactor;
            Time.fixedDeltaTime = Time.timeScale * .02f;
            bulletTimeIndicatorAnimator.SetBool("BulletTime", true);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            // Reload Scene
            SceneManager.LoadScene("Testing");
        }

        if (blasting)
        {
            blasting = false;

            Vector2 blastDirection = getVectorFromPlayerToMouse();

            // Confine blast directions to 4
            blastDirection = Mathf.Abs(blastDirection.x) > Mathf.Abs(blastDirection.y) ? Vector2.right * Mathf.Sign(blastDirection.x) : Vector2.up * Mathf.Sign(blastDirection.y);

            Vector2 newVelocity = rbody.velocity + (-blastDirection * bulletForce);
            if (blastDirection.x == 0)
            {
                if (blastDirection.y < 0)
                {
                    // Minimun y velocity after a downwards blast
                    float minY = 6f;
                    if (rbody.velocity.y + bulletForce < minY)
                        newVelocity = new Vector2(rbody.velocity.x, minY);
                    animator.SetTrigger("Down Blast");
                }
                else
                {
                    coyoteTimer = 0;
                    jumpCooldownTimer = coyoteTimeLength;
                }
            }
            else if (playerState == State.inAir && facingRight && blastDirection.x < 0 || !facingRight && blastDirection.x > 0)
            {
                animator.SetTrigger("BW Blast");
            }

            rbody.velocity = new Vector2(Mathf.Clamp(newVelocity.x, -trueMaxSpeed, trueMaxSpeed), Mathf.Clamp(newVelocity.y, -trueMaxSpeed, trueMaxSpeed));

            longJump = false;

            // Play particle effect
            blast.transform.position = transform.position;
            blast.transform.rotation = Quaternion.AngleAxis(Vector2.Angle(Vector2.right, blastDirection) + 10, Vector3.back);
            blast.Play();
        }

        if (shooting)
        {
            Vector2 laserDirection = getVectorFromPlayerToMouse();
            laserGuide.setLaserDirection(laserDirection);
            laserGuide.showLaser();
        }

        // If left click is released or the timer expires:
        if (Input.GetMouseButtonUp(0) && shooting || bulletTimeTimer < 0)
        {
            shooting = false;
            if (bulletTimeTimer < 0)
            {
                Vector2 laserDirection = getVectorFromPlayerToMouse();

                GameObject newBullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
                newBullet.GetComponent<BulletPhysics>().movAngle = laserDirection;

                laserGuide.hideLaser();
                bulletTimeIndicatorAnimator.SetBool("BulletTime", false);
            }

            // Reset bullet time parameters
            bulletTimeTimer = 0;
            // Reset the time scale
            Time.timeScale = 1;
            Time.fixedDeltaTime = .02f;
        }
    }

    void FixedUpdate()
    {
        // Player States
        // IN AIR
        if (playerState == State.inAir)
        {
            // If B-Hopping, Change Max Speed settings to keep current momentum
            if (Mathf.Abs(rbody.velocity.x) > maxRunSpeed && Mathf.Sign(rbody.velocity.x) == Mathf.Sign(Input.GetAxisRaw("Horizontal")))
            {
                currentMaxSpeed = Mathf.Abs(rbody.velocity.x);
            }
            else
            {
                currentMaxSpeed = maxRunSpeed;
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
        }

        // ON GROUND
        else if (playerState == State.onGround)
        {
            // If Decelerating
            if (goalSpeed.magnitude < rbody.velocity.magnitude || Mathf.Sign(goalSpeed.x) != Mathf.Sign(rbody.velocity.x))
            {
                accelValue = 1.75f * acceleration;
            }
            // If Accelerating
            else
            {
                accelValue = acceleration;
            }
            currentMaxSpeed = maxRunSpeed;
        }

        // Horizontal Speed
        goalSpeed = new Vector2(Input.GetAxisRaw("Horizontal") * currentMaxSpeed, rbody.velocity.y);
        if (playerState != State.wallCling)
        {
            rbody.velocity = Vector2.MoveTowards(rbody.velocity, goalSpeed, accelValue * Time.fixedDeltaTime);
        }

        // Update animation parameter
        animator.SetFloat("Horizontal Input", Mathf.Abs(goalSpeed.x));

        // Decrements all of the timers
        if (coyoteTimer > 0)
            coyoteTimer -= Time.fixedDeltaTime;
        if (inputBufferTimer > 0)
            inputBufferTimer -= Time.fixedDeltaTime;
        if (jumpCooldownTimer > 0)
            jumpCooldownTimer -= Time.fixedDeltaTime;
        if (wallStickTimer > 0)
            wallStickTimer -= Time.fixedDeltaTime;
        if (bulletTimeTimer > 0)
            bulletTimeTimer -= Time.fixedUnscaledDeltaTime;  // Time has to be unscaled here or it will be affected by the slowdown
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .02f, groundLayer);
    }
    private bool isOnWall()
    {
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, dir, .02f, groundLayer);
    }

    private void Flip()
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
        jumpCooldownTimer = jumpCooldownLength;
        // Long Jump Must Have Space Held Down (In case using Input Buffering)
        longJump = Input.GetKey(KeyCode.Space);
        fastFallModifier = 1;
    }

    private void WallJump()
    {
        float magnitude = Mathf.Sqrt((jumpForce * jumpForce) / 2);
        rbody.velocity = facingRight ? magnitude * new Vector2(-1, 1) : magnitude * new Vector2(1, 1);
        canFire = true;
        coyoteTimer = 0;
        jumpCooldownTimer = jumpCooldownLength;

        // Long Jump Must Have Space Held Down (In case using Input Buffering)
        longJump = Input.GetKey(KeyCode.Space);
        fastFallModifier = 1;
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

        return (Vector2)(norm - transform.position).normalized;
    }
}
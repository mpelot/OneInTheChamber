using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LaserGuide))]
public class PlayerMovement : MonoBehaviour
{
    //Movement
    public float acceleration;
    public float maxRunSpeed;
    public float trueMaxSpeed;
    public enum State {inAir, onGround, wallCling};
    public State playerState;

    //Jumping
    public float jumpForce;
    public float coyoteTimeLength;
    public float inputBufferLength;
    public float jumpCooldownLength;
    public float defaultGravity;
    public float longJumpGravity;
    public float fallingAccel;
    public float maxGravity;
    public float wallHangTime;
    public float wallGravity;

    //Shooting
	public GameObject bulletPrefab;
    public float bulletForce;
    public float bulletTimeLength;
    public float bulletTimeSlowdownFactor;
    public Animator bulletTimeIndicatorAnimator;
    public bool canFire = true;
    LaserGuide laserGuide;
    public ParticleSystem blast;
    public float redirectYScale;

    private Rigidbody2D rbody;
    private Animator animator;
    private bool facingRight = true;
    private float coyoteTimer = 0;
    private float inputBufferTimer = 0;
    private float jumpCooldownTimer = 0;
    private float wallTimer = 0;
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
        //Update the speed parameter in the animator
        animator.SetFloat("Horizontal Speed", Mathf.Clamp(Mathf.Ceil(Mathf.Abs(rbody.velocity.x)) + 1, -1, 5));

        //Update the vertical velocity parameter in the animator
        animator.SetFloat("Vertical Velocity", Mathf.Clamp(rbody.velocity.y, -5, 5));

        //If Turning
        if ((goalSpeed.x < 0 && facingRight) || (goalSpeed.x > 0 && !facingRight)) 
        {
            // Flip the player
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        //Update the moving backwards paramater in the animator
        if (transform.localScale.x * rbody.velocity.x < 0) 
        {
            animator.SetBool("Moving Backwards", true);
        }
        else 
        {
            animator.SetBool("Moving Backwards", false);
        }

        //Player States
        //IN AIR
        if (playerState == State.inAir)
        {
            //Restrict Horizontal Movement In Air
            accelValue = .7f * acceleration;

            //If Jumping
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //If Coyote Timer Active, Jump
                if (coyoteTimer > 0)
                {
                    Jump();
                }
                else
                {
                    //Sets Input Buffer Timer
                    inputBufferTimer = inputBufferLength;
                }
            }

            //Enable Fast Falling
            if (Input.GetKey(KeyCode.S))
            {
                fastFallModifier = 1.2f;
                longJump = false;
            }

            //Disable Long Jump
            if (Input.GetKeyUp(KeyCode.Space) || rbody.velocity.y <= 0)
            {
                longJump = false;
            }
        }

        //ON GROUND
        else if (playerState == State.onGround)
        {
            //Jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
        }
		
		//Blasting
		if (Input.GetMouseButtonDown(0) && canFire) 
        {
            blasting = true;
            canFire = false;
        }

        //Shooting
        if (Input.GetMouseButtonDown(1) && canFire) {
            shooting = true;
            canFire = false;
            //Start bullet time timer
            bulletTimeTimer = bulletTimeLength;
            //Set time scale to the slowdown factor
            Time.timeScale = bulletTimeSlowdownFactor;
            Time.fixedDeltaTime = Time.timeScale * .02f;
            bulletTimeIndicatorAnimator.SetBool("BulletTime", true);
        }

        //Reload Scene
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Testing");
        }

        if (blasting) {
            //Fire
            // NOTE: mouse position is in screenspace!
            // We must normalize into worldspace before we can use these coords.
            Vector3 mousePos = Input.mousePosition;
            // z needs to be nonzero for this to work
            mousePos.z = mainCam.nearClipPlane;

            Vector3 norm = mainCam.ScreenToWorldPoint(mousePos);
            // Revert z transform
            norm.z = 0;

            Vector2 bulletDirection = (Vector2)(norm - transform.position).normalized;

            // Snap bullet direction to 45 degree increments
            float angle = Mathf.Atan2(bulletDirection.y, bulletDirection.x) * Mathf.Rad2Deg;
            angle = Mathf.Round(angle / 45) * 45;
            bulletDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            //Add Recoil
            Vector2 newVelocity = rbody.velocity + (-bulletDirection * bulletForce);

            if ((bulletDirection.x > 0 && rbody.velocity.x > 0) || (bulletDirection.x < 0 && rbody.velocity.x < 0))
            {
                float newVelocityX = -rbody.velocity.x + (-bulletDirection.x * bulletForce);
                float newVelocityY = rbody.velocity.y + (-bulletDirection.y * bulletForce);
                newVelocity = new Vector2(newVelocityX, newVelocityY);
            }
            if ((bulletDirection.y > 0 && rbody.velocity.y > 0) || (bulletDirection.y < 0 && rbody.velocity.y < 0))
            {
                
                float newVelocityX = rbody.velocity.x + (-bulletDirection.x * bulletForce);
                float newVelocityY = (-rbody.velocity.y + (-bulletDirection.y * bulletForce)) * redirectYScale;
                newVelocity = new Vector2(newVelocityX, newVelocityY);
            }

            rbody.velocity = new Vector2(Mathf.Clamp(newVelocity.x, -trueMaxSpeed, trueMaxSpeed), Mathf.Clamp(newVelocity.y, -trueMaxSpeed, trueMaxSpeed));

            // Alternate way to clamp speed
            //rbody.velocity = Vector2.ClampMagnitude(newVelocity, trueMaxSpeed);

            longJump = false;
            if (bulletDirection.y > 0) 
            {
                coyoteTimer = 0;
                jumpCooldownTimer = coyoteTimeLength;
            }

            if (bulletDirection == Vector2.down) 
            {
                animator.SetTrigger("Down Blast");
            }

            blast.transform.position = transform.position;
            blast.transform.rotation = Quaternion.AngleAxis(Vector2.Angle(Vector2.right, bulletDirection) + 10, Vector3.back);
            blast.Play();

            //Reset Parameter
            blasting = false;
        }

        if (shooting)
        {
            // NOTE: mouse position is in screenspace!
            // We must normalize into worldspace before we can use these coords.
            Vector3 mousePos = Input.mousePosition;
            // z needs to be nonzero for this to work
            mousePos.z = mainCam.nearClipPlane;

            Vector3 norm = mainCam.ScreenToWorldPoint(mousePos);
            // Revert z transform
            norm.z = 0;

            Vector2 bulletDirection = (Vector2)(norm - transform.position).normalized;

            laserGuide.setLaserDirection(bulletDirection);
            laserGuide.showLaser();
        }

        //If left click is released or the timer expires:
        if (Input.GetMouseButtonUp(0) && shooting || bulletTimeTimer < 0) 
        {
            if (bulletTimeTimer < 0) 
            {
                //Fire
                // NOTE: mouse position is in screenspace!
                // We must normalize into worldspace before we can use these coords.
                Vector3 mousePos = Input.mousePosition;
                // z needs to be nonzero for this to work
                mousePos.z = mainCam.nearClipPlane;

                Vector3 norm = mainCam.ScreenToWorldPoint(mousePos);
                // Revert z transform
                norm.z = 0;

                Vector2 bulletDirection = (Vector2)(norm - transform.position).normalized;

                GameObject newBullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
                newBullet.GetComponent<BulletPhysics>().movAngle = bulletDirection;

                laserGuide.hideLaser();
                bulletTimeIndicatorAnimator.SetBool("BulletTime", false);
            }
            
            //Reset bullet time parameters
            shooting = false;
            bulletTimeTimer = 0;
            //Reset the time scale
            Time.timeScale = 1;
            Time.fixedDeltaTime = .02f;
        }
    }

    void FixedUpdate()
    {
        //Player States
        //IN AIR
        if (playerState == State.inAir)
        {
            //If B-Hopping, Change Max Speed settings to keep current momentum
            if (Mathf.Abs(rbody.velocity.x) > maxRunSpeed && Mathf.Sign(rbody.velocity.x) == Mathf.Sign(Input.GetAxisRaw("Horizontal")))
            {
                currentMaxSpeed = Mathf.Abs(rbody.velocity.x);
            }
            else
            {
                currentMaxSpeed = maxRunSpeed;
            }
            
            //Variable Jump Height
            if (longJump == false && rbody.gravityScale <= maxGravity)
            {
                //If Short Jump or Ended Long Jump
                rbody.gravityScale = fallingAccel * fastFallModifier;
            }
            else if (longJump == true && rbody.velocity.y >= 0)
            {
                //If Long Jumping
                rbody.gravityScale = longJumpGravity;
            }
        }

        //ON GROUND
        else if (playerState == State.onGround)
        {
            //If Decelerating
            if (goalSpeed.magnitude < rbody.velocity.magnitude || Mathf.Sign(goalSpeed.x) != Mathf.Sign(rbody.velocity.x))
            {
                accelValue = 1.75f * acceleration;
            }
            //If Accelerating
            else
            {
                accelValue = acceleration;
            }
            currentMaxSpeed = maxRunSpeed;
        }

        //Horizontal Speed
        goalSpeed = new Vector2(Input.GetAxisRaw("Horizontal") * currentMaxSpeed, rbody.velocity.y);
        rbody.velocity = Vector2.MoveTowards(rbody.velocity, goalSpeed, accelValue * Time.fixedDeltaTime);

        //Update animation parameter
        animator.SetFloat("Horizontal Input", Mathf.Abs(goalSpeed.x));

        //Decrements Coyote Time Timer, Input Buffer Timer, Jump Cooldown Timer, and Bullet Time Timer
        if(coyoteTimer > 0)
        {
            coyoteTimer -= Time.fixedDeltaTime;
        }
        if(inputBufferTimer > 0)
        {
            inputBufferTimer -= Time.fixedDeltaTime;
        }
        if(jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= Time.fixedDeltaTime;
        }
        if(wallTimer > 0)
        {
            wallTimer -= Time.fixedDeltaTime;
        }
        if (bulletTimeTimer > 0) 
        {
            //Time has to be unscaled here or it will be affected by the slowdown
            bulletTimeTimer -= Time.fixedUnscaledDeltaTime;
        }
    }

    public void StayGround()
    {
        //Input Buffer Jumping
        if(inputBufferTimer > 0)
        {
            Jump();
        }
        
        //Sets Gravity Back To Normal
        rbody.gravityScale = defaultGravity;

        //Sets In-Air to false
        playerState = State.onGround;
    }

    public void EnterGround()
    {
            playerState = State.onGround;
            canFire = true;

            //Update the grounded parameter in the animator
            animator.SetBool("Grounded", true);
    }

    public void ExitGround() {
        playerState = State.inAir;

        //Update the grounded parameter in the animator
        animator.SetBool("Grounded", false);

        //Starts Coyote Time Timer
        if (jumpCooldownTimer <= 0) {
            coyoteTimer = coyoteTimeLength;
        }
    }

    private void Jump()
    {
        rbody.velocity = new Vector2(rbody.velocity.x, jumpForce);
        coyoteTimer = 0;
        jumpCooldownTimer = jumpCooldownLength;
        //Long Jump Must Have Space Held Down (In case using Input Buffering)
        if(Input.GetKey(KeyCode.Space))
        {
            longJump = true;
        }
        else
        {
            longJump = false;
        }
        fastFallModifier = 1;
    }
}

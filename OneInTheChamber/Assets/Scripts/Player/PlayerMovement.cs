using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Movement
    public float acceleration;
    public float maxRunSpeed;
    public float trueMaxSpeed;

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

    private Rigidbody2D rbody;
    private float coyoteTimer = 0;
    private float inputBufferTimer = 0;
    private float jumpCooldownTimer = 0;
    private float wallTimer = 0;
    private float currentMaxSpeed;
    private float accelValue;
    private bool inAir = true;
    private Vector2 goalSpeed;
    private bool longJump = false;
    private float fastFallModifier;
    private Camera mainCam;
    private LayerMask wall;
    private float lastSpeed;

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        accelValue = acceleration;
		mainCam = Camera.main;
        wall = LayerMask.GetMask("Walls");
        fastFallModifier = 1;
    }

    private void Update()
    {
        //If Jump Pressed
        if(Input.GetKey(KeyCode.Space))
        {

            //Coyote Time
            if(coyoteTimer > 0)
            {
                Jump();
            }
            else
            {
                //Sets Input Buffer Timer
                inputBufferTimer = inputBufferLength;
            }
        }

        //Right Wall Cling
        /*if (Input.GetAxisRaw("Horizontal") > 0)
        {
            RaycastHit2D right = Physics2D.Raycast((Vector2)transform.position, Vector2.right*.42f, .42f, wall);
            if(right.collider != null && right.collider.tag == "Floor" && inAir)
            {
                if(wallTimer <= 0)
                {
                    rbody.velocity = new Vector2(rbody.velocity.x, rbody.velocity.y * .5f);
                }
                wallTimer = wallHangTime;
            }
        }
        if(wallTimer > 0)
        {
            rbody.gravityScale = wallGravity;
        }*/

        //Air and Ground Acceleration
        if(inAir)
        {
            //Decreases Air Horizontal Movement Control
            accelValue = .7f *acceleration;
        }
        else if (!Input.GetKey(KeyCode.Space) && !inAir && (goalSpeed.magnitude < rbody.velocity.magnitude || Mathf.Sign(goalSpeed.x) != Mathf.Sign(rbody.velocity.x)))
        {
            //If decelerating on the ground, make accel value faster
            accelValue = 1.75f * acceleration;
        }
        else
        {
            //If accelerating, make accel value determined acceleration rate
            accelValue = acceleration;
        }
		
        //Early Long Jump End
        if(Input.GetKeyUp(KeyCode.Space))
        {
            //End Long Jump
            longJump = false;
        }
		
        //Fast Falling
        if(inAir && Input.GetKey(KeyCode.S))
        {
            longJump = false;
            fastFallModifier = 1.2f;
        }
		
		if (Input.GetMouseButtonDown(0)) {
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
            
            //Add Recoil
            rbody.velocity += -bulletDirection * bulletForce;
            longJump = false;
            if(bulletDirection.y > 0)
            {
                coyoteTimer = 0;
                jumpCooldownTimer = coyoteTimeLength;
            }
		}

        //Keep Current Speed For Next Frame
        lastSpeed = rbody.velocity.x;
    }

    void FixedUpdate()
    {
        //If B-Hopping, Change Max Speed settings to keep current momentum
        if(inAir && Mathf.Abs(rbody.velocity.x) > maxRunSpeed && Mathf.Sign(rbody.velocity.x) == Mathf.Sign(Input.GetAxisRaw("Horizontal")))
        {
            currentMaxSpeed = Mathf.Abs(rbody.velocity.x);
        }
        else
        {
            currentMaxSpeed = maxRunSpeed;
        }

        //Horizontal Speed
        goalSpeed = new Vector2(Input.GetAxisRaw("Horizontal") * currentMaxSpeed, rbody.velocity.y);
        rbody.velocity = Vector2.MoveTowards(rbody.velocity, goalSpeed, accelValue * Time.fixedDeltaTime);
        
        //Decrements Coyote Time Timer, Input Buffer Timer, and Jump Cooldown Timer
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

        //Increases Gravity Scale During Jump
        if((longJump == false || rbody.velocity.y < 0 ) && rbody.gravityScale <= maxGravity)
        {
            //If Short Jump or Ended Long Jump
            rbody.gravityScale = fallingAccel * fastFallModifier;
        }
        else if(longJump == true && rbody.velocity.y >= 0)
        {
            //If Long Jumping
            rbody.gravityScale = longJumpGravity;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //Input Buffer Jumping
        if(inputBufferTimer > 0)
        {
            Jump();
        }
        //Starts Coyote Time Timer
        if(jumpCooldownTimer <= 0)
        {
            coyoteTimer = coyoteTimeLength;
        }
        
        //Sets Gravity Back To Normal
        rbody.gravityScale = defaultGravity;

        //Sets In-Air to false
        inAir = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        inAir = false;
        wallTimer = 0;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        inAir = true;
    }

    private void Jump()
    {
        Debug.Log(transform.position.y);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rbody;
    public SpriteRenderer sprender;
    public float acceleration;
    public float maxSpeed = 4;
    public float jumpForce = 20;
    public float coyoteTimeLength = .25f;
    public float inputBufferLength = .25f;
    public float jumpCooldownLength = .4f;
	public GameObject bulletPrefab;

    private float coyoteTimer = 0;
    private float inputBufferTimer = 0;
    private float jumpCooldownTimer = 0;
    private float accelValue;
    private bool inAir = true;
    private Vector2 goalSpeed;
    private bool longJump = false;
    private float fastFallModifier;
    private Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        accelValue = acceleration;
		mainCam = Camera.main;
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
        if(inAir)
        {
            //Decreases Air Horizontal Movement
            accelValue = .7f *acceleration;
        }
        else if (!inAir && (goalSpeed.magnitude < rbody.velocity.magnitude || Mathf.Sign(goalSpeed.x) != Mathf.Sign(rbody.velocity.x)))
        {
            accelValue = 1.75f * acceleration;
        }
        else
        {
            accelValue = acceleration;
        }
		
        if(Input.GetKeyUp(KeyCode.Space))
        {
            //End Long Jump
            longJump = false;
        }
		
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
			
			// Find angle between myself and the spawned bullet
			float angle = Mathf.Atan2(norm.y - transform.position.y, norm.x - transform.position.x);
			Vector3 npos = new Vector3(transform.position.x + Mathf.Cos(angle) * 0.4f, transform.position.y + Mathf.Sin(angle) * 0.4f, 0);
			
			// TODO: add recoil to the player
			
			GameObject newBullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
			newBullet.GetComponent<BulletPhysics>().movAngle = angle;
			
		}
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Horizontal Speed
        goalSpeed = new Vector2(Input.GetAxisRaw("Horizontal") * maxSpeed, rbody.velocity.y);
        rbody.velocity = Vector2.MoveTowards(rbody.velocity, goalSpeed, accelValue * Time.fixedDeltaTime);
        
        //Decreases Coyote Time Timer, Input Buffer Timer, and Jump Cooldown Timer
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
        //Increases Gravity Scale During Jump
        if((longJump == false || rbody.velocity.y < 0 ) && rbody.gravityScale <= 4)
        {
            rbody.gravityScale = rbody.gravityScale + 16.5f * fastFallModifier * Time.fixedDeltaTime;
        }
        else if(longJump == true && rbody.velocity.y >= 0)
        {
            rbody.gravityScale = 1.75f;
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
        rbody.gravityScale = 2f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        inAir = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        inAir = true;
    }

    private void Jump()
    {
        rbody.velocity = new Vector2(rbody.velocity.x, jumpForce);
        coyoteTimer = 0;
        jumpCooldownTimer = jumpCooldownLength;
        longJump = true;
        fastFallModifier = 1;
    }
}

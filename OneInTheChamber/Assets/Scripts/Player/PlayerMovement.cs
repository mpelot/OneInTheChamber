using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rbody;
    public SpriteRenderer sprender;
    public float acceleration;
    public float maxSpeed = 4;
    public float jumpForce = 10;
    public float coyoteTimeLength = .25f;
    public float inputBufferLength = .25f;
    public float jumpCooldownLength = .4f;

    private float coyoteTimer = 0;
    private float inputBufferTimer = 0;
    private float jumpCooldownTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(coyoteTimer > 0)
            {
                Jump();
            }
            else
            {
                inputBufferTimer = inputBufferLength;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 goalSpeed = new Vector2(Input.GetAxisRaw("Horizontal") * maxSpeed, rbody.velocity.y);
        rbody.velocity = Vector2.MoveTowards(rbody.velocity, goalSpeed, acceleration * Time.fixedDeltaTime);
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
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(inputBufferTimer > 0)
        {
            Jump();
        }
        if(jumpCooldownTimer <= 0)
        {
            coyoteTimer = coyoteTimeLength;
        }
    }
    private void Jump()
    {
        rbody.velocity = new Vector2(rbody.velocity.x, jumpForce);
        coyoteTimer = 0;
        jumpCooldownTimer = jumpCooldownLength;
    }
}

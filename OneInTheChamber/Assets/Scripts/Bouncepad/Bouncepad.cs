using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncepad : MonoBehaviour
{
	[Header("Bouncepad Setup")]
	public float amount = 20;

    private Animator animator;
    private bool bounce;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();   
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("Bounce", false);
        if (bounce)
        {
            animator.SetBool("Bounce", true);
            bounce = false;
        }
    }
	
	private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            GameObject player = collision.gameObject;
            if (player.GetComponent<PlayerMovement>().playerState != PlayerMovement.State.wallCling)
            {
                player.transform.position = new Vector3(player.transform.position.x, transform.position.y + .5f, 0f);
                Rigidbody2D rbody = player.GetComponent<Rigidbody2D>();
                rbody.velocity = new Vector2(rbody.velocity.x, amount);
                player.GetComponent<PlayerMovement>().canBlast = true;
                bounce = true;
            }
        }
    }
}

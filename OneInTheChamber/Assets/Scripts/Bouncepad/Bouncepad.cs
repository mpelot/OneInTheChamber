using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncepad : MonoBehaviour
{
	[Header("Bouncepad Setup")]
	public float strength = 20f;

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
            collision.gameObject.GetComponent<PlayerMovement>().Bounce(strength);
            bounce = true;
        }
    }
}

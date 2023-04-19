using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncepad : MonoBehaviour
{
	[Header("Bouncepad Setup")]
	public float amount = 20;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
			Rigidbody2D rbody = collision.gameObject.GetComponent<Rigidbody2D>();
			rbody.velocity = new Vector2(rbody.velocity.x, amount);
        }
    }
}

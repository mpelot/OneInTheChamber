using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPhysics : MonoBehaviour
{
    public PhysicsMaterial2D stoppedMatirial;
	[HideInInspector]
    public Vector2 movAngle;
    public float bulletSpeed;
    private Rigidbody2D rbody;
    public int bounceLimit;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        rbody.velocity = movAngle * bulletSpeed;
    }

    // Update is called once per frame
    void Update()
    {
		// Move
		//transform.position = transform.position + new Vector3(Mathf.Cos(movAngle) * 0.05f, Mathf.Sin(movAngle) * 0.05f, 0);
		
		// TODO: move this over to the rigidbody system
    }
    // When the bullet is retrieved
    public void Retrieve() {
        // Destroy bullet
        Destroy(gameObject);
    }

    // When the bullet bounces
    private void OnCollisionEnter2D(Collision2D collision) 
    {
        // Enable the trigger collider for retrieval
        GetComponentInChildren<CircleCollider2D>().enabled = true;
        bounceLimit--;
        if (bounceLimit == 0) {
            rbody.velocity = Vector3.zero;
            rbody.gravityScale = 1;
            GetComponent<BoxCollider2D>().sharedMaterial = stoppedMatirial;
        }
    }
}

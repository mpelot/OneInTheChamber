using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPhysics : MonoBehaviour
{
	[HideInInspector]
    public Vector2 movAngle;
    public float bulletSpeed;
    private Rigidbody2D rbody;
	
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

}

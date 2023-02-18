using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPhysics : MonoBehaviour
{
	public float movAngle;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		// Move
		transform.position = transform.position + new Vector3(Mathf.Cos(movAngle) * 0.05f, Mathf.Sin(movAngle) * 0.05f, 0);
		
		// TODO: move this over to the rigidbody system
    }
}

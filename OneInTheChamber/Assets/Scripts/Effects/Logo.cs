using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logo : MonoBehaviour
{
    private float timer = 0;
    private Vector3 start;
    private Rigidbody2D rbody;
    // Start is called before the first frame update
    void Start()
    {
        start = transform.position;
        rbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timer += .75f * Time.fixedDeltaTime;
        if (timer >= Mathf.PI * 2)
        {
            timer = 0;
            transform.position = start;
        }
        rbody.velocity = new Vector2(0, .15f*Mathf.Cos(timer));
    }
}

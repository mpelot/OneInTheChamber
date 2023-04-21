using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
	[Header("Platform Setup")]
	public Vector2 start;
	public Vector2 end;
	public float time;
	
    private float timer = 0;
    private Rigidbody2D rbody;
    private float startX;
    private float startY;
    private float endX;
    private float endY;
    private float xDiff;
    private float yDiff;
    private Vector2 lastPos;
    public bool attached = false;
    // Start is called before the first frame update
    void Start()
    {
        lastPos = start;
        startX = start.x;
        startY = start.y;
        endX = end.x;
        endY = end.y;
        xDiff = endX - startX;
        yDiff = endY - startY;
        rbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (attached && (rbody.velocity.y < 0 || rbody.velocity.x != 0))
        {
            GameObject.Find("Player").transform.Translate(transform.position - (Vector3)lastPos);
        }
        lastPos = transform.position;
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime * (2*Mathf.PI) / time;
        if(timer >= Mathf.PI * 2)
        {
            timer = 0;
            transform.position = start;
        }
        rbody.velocity = new Vector2(Mathf.Sin(timer) * (xDiff / 2) * (2 * Mathf.PI) / time, Mathf.Sin(timer) * (yDiff / 2) * (2 * Mathf.PI) / time);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            attached = true;
            collision.gameObject.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.None;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            attached = false;
            collision.gameObject.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.Interpolate;
            collision.gameObject.GetComponent<Rigidbody2D>().velocity += rbody.velocity;
        }
    }
}

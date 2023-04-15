using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPhysics : MonoBehaviour
{
    public PhysicsMaterial2D stoppedMatirial;
    public float bulletSpeed;
    public Vector2 movAngle;
    private Rigidbody2D rbody;
    public int bounceLimit;
    private bool dying = false;
    public LayerMask ground;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        rbody.velocity = Vector2.zero;
        StartCoroutine(StartMove());
        GetComponent<TrailRenderer>().endColor = new Color(1, 1, 1, 0);
    }

    void Update()
    {
        if(Physics2D.BoxCast(transform.position, new Vector2(.1f, .1f), 0f, Vector2.zero, .001f, ground) && !dying)
        {
            dying = true;
            rbody.velocity = Vector3.zero;
            StartCoroutine(DieCoroutine());
        }
    }

    public IEnumerator StartMove()
    {
        yield return new WaitForSeconds(.1f);
        rbody.velocity = movAngle * bulletSpeed;
    }

    // When the bullet is retrieved
    public void Retrieve() {
        // Destroy bullet
        Destroy(gameObject);
    }

    // When the bullet bounces
    /*private void OnCollisionEnter2D(Collision2D collision) 
    {
        // Enable the trigger collider for retrieval
        //GetComponentInChildren<CircleCollider2D>().enabled = true;
        //bounceLimit--;
        //if (bounceLimit == 0) {
            rbody.velocity = Vector3.zero;
            //GetComponent<BoxCollider2D>().sharedMaterial = stoppedMatirial;
            StartCoroutine(DieCoroutine());
        //}
    }*/


    IEnumerator DieCoroutine()
    {
        for(int i = 0; i < 150; i++)
        {
            yield return new WaitForSeconds(.006666f);
            GetComponent<TrailRenderer>().startColor = new Color(1, 1, 1, 1-.006666f*i);
        }
        Destroy(gameObject);
    }
}

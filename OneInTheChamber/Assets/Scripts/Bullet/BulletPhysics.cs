using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPhysics : MonoBehaviour
{
    public PhysicsMaterial2D stoppedMatirial;
    public float bulletSpeed;
    public Vector2 movAngle;
    public int bounceLimit;
    public LayerMask ground;
    public LayerMask target;
    public GameObject blastParticles;
    private RaycastHit2D hitG;

    // Start is called before the first frame update
    void Start()
    {
        hitG = Physics2D.Raycast(transform.position, movAngle, 1000, ground);
        RaycastHit2D hitT = Physics2D.Raycast(transform.position, movAngle, 1000, target);
        if(hitT.collider != null)
        {
            if (hitT.collider.gameObject.tag == "Target")
            {
                hitT.collider.gameObject.GetComponent<Target>().Shatter();
            }
            else
            {
                FindObjectOfType<LevelManager>().Lose();
            }
            
        }
        StartCoroutine(DieCoroutine());
        GetComponent<TrailRenderer>().endColor = new Color(1, 1, 1, 0);
    }

    void Update()
    {
        
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
        yield return new WaitForSeconds(.01f);
        if (hitG.collider != null)
        {
            transform.Translate(hitG.distance * movAngle.normalized);
        }
        else
        {
            transform.Translate(100 * movAngle.normalized);
        }
        Instantiate(blastParticles, transform.position, transform.rotation);
        for (int i = 0; i < 150; i++)
        {
            yield return new WaitForSeconds(.006666f);
            GetComponent<TrailRenderer>().startColor = new Color(1, 1, 1, 1-.006666f*i);
        }
        Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    public enum Direction { Left, Right, Up, Down };
    public Direction currentDirection = Direction.Left;
    private Vector2 dirVector;
    private LineRenderer lrender;
    private BoxCollider2D bCollide;
    private LayerMask lMask;
    // Start is called before the first frame update
    void Start()
    {

        lrender = GetComponent<LineRenderer>();
        lrender.useWorldSpace = false;
        bCollide = GetComponent<BoxCollider2D>();
        currentDirection = (Direction)transform.parent.gameObject.GetComponent<Laser>().currentDirection;
        switch(currentDirection)
        {
            case Direction.Left:
                dirVector = Vector2.left;
                break;
            case Direction.Right:
                dirVector = Vector2.right;
                break;
            case Direction.Up:
                dirVector = Vector2.up;
                break;
            case Direction.Down:
                dirVector = Vector2.down;
                break;
        }
        lMask = LayerMask.GetMask("Ground");
        if(transform.parent.gameObject.GetComponent<Laser>().cooldownTime != 0)
        { 
            StartCoroutine(KillTime());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3)(dirVector), dirVector, 100, lMask);
        if(hit.collider != null)
        {
            //If Horizontal
            if(currentDirection == Direction.Left || currentDirection == Direction.Right)
            {
                float distance = hit.point.x - transform.position.x;
                lrender.SetPosition(1, new Vector3(distance, 0, 0));
                bCollide.size = new Vector2(Mathf.Abs(distance), bCollide.size.y);
                bCollide.offset = new Vector2(distance / 2, 0);
            }
            //If Vertical
            else
            {
                float distance = hit.point.y - transform.position.y;
                lrender.SetPosition(1, new Vector3(0, distance, 0));
                bCollide.size = new Vector2(bCollide.size.x, Mathf.Abs(distance));
                bCollide.offset = new Vector2(0, distance / 2);
            }
        }
        lrender.sortingOrder = (int)transform.position.y;
    }

    private IEnumerator KillTime()
    {
        yield return new WaitForSeconds(transform.parent.gameObject.GetComponent<Laser>().shootTime);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            GameObject.Find("LevelManager").GetComponent<LevelManager>().Lose();
        }
    }
}

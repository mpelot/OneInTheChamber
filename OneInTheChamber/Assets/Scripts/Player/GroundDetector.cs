using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GroundDetector : MonoBehaviour
{
    PlayerMovement playerMovement;
    private void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            playerMovement.EnterGround();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            playerMovement.ExitGround();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            playerMovement.StayGround();
        }
    }


}

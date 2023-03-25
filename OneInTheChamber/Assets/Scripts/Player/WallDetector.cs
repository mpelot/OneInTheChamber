using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetector : MonoBehaviour
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
            playerMovement.canFire = true;
        }
    }
}

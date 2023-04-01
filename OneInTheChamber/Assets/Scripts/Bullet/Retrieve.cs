using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Retrieve : MonoBehaviour
{
    // When the bullet hits the player
    private void OnTriggerEnter2D(Collider2D collision) {
        GetComponentInParent<BulletPhysics>().Retrieve();
    }
}

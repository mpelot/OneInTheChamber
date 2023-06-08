using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dropper : MonoBehaviour
{
    public float timeUntilDrop;
    public float resetTime;

    private BoxCollider2D boxCollider;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            StartCoroutine(DropPlatform());
        }
    }

    private IEnumerator DropPlatform()
    {
        yield return new WaitForSeconds(timeUntilDrop);
        boxCollider.isTrigger = true;
        yield return new WaitForSeconds(resetTime);
        boxCollider.isTrigger = false;
    }
}

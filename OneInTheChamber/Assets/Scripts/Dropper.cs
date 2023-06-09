using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dropper : MonoBehaviour
{
    private enum Orientation
    {
        Up,
        Down,
        Left,
        Right
    }

    public float timeUntilDrop;
    public float resetTime;
    public float forceVelocity;
    public BoxCollider2D hitBox;

    private SpriteRenderer spriteRenderer;
    private Orientation orientation;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        float mod = transform.rotation.eulerAngles.z % 360;
        switch (transform.rotation.eulerAngles.z % 360)
        {
            case 0:
                orientation = Orientation.Up;
                break;
            case 90:
                orientation = Orientation.Left;
                break;
            case 180:
                orientation = Orientation.Down;
                break;
            case 270:
                orientation = Orientation.Right;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            Vector3 lastPlayerSpeed = collision.gameObject.GetComponent<PlayerMovement>().lastSpeed;
            if (orientation == Orientation.Up && lastPlayerSpeed.y < -forceVelocity
                || orientation == Orientation.Down && lastPlayerSpeed.y > forceVelocity
                || orientation == Orientation.Left && lastPlayerSpeed.x > forceVelocity
                || orientation == Orientation.Right && lastPlayerSpeed.x < -forceVelocity)
            {
                collision.gameObject.GetComponent<Rigidbody2D>().velocity = lastPlayerSpeed;
                OpenPlatform();
                Invoke("ClosePlatform", resetTime);
            }
            else
            {
                Invoke("OpenPlatform", timeUntilDrop);
                Invoke("ClosePlatform", timeUntilDrop + resetTime);
            }
        }
    }
    
    private void OpenPlatform()
    {
        this.gameObject.layer = LayerMask.NameToLayer("Default");
        hitBox.enabled = false;
        Color color = spriteRenderer.color;
        color.a = 0.5f;
        spriteRenderer.color = color;
    }

    private void ClosePlatform()
    {
        this.gameObject.layer = LayerMask.NameToLayer("Ground");
        hitBox.enabled = true;
        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;
    }
}

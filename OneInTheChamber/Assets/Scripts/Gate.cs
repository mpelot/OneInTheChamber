using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    private enum Orientation
    {
        Up,
        Down,
        Left,
        Right
    }

    public float timeUntilOpen;
    public float resetTime;
    public float forceVelocity;
    public float hitPauseDuration;
    public BoxCollider2D hitBox;

    private Animator animator;
    private Orientation orientation;
    private bool queueCloseGate;

    private void Start()
    {
        animator = GetComponent<Animator>();
        queueCloseGate = false;
        
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
            if (queueCloseGate)
            {
                queueCloseGate = false;
                CancelInvoke("CloseGate");
                Invoke("CloseGate", resetTime);
            }
            else
            {
                Vector3 lastPlayerSpeed = collision.gameObject.GetComponent<PlayerMovement>().lastSpeed;
                if (orientation == Orientation.Up && lastPlayerSpeed.y < -forceVelocity
                    || orientation == Orientation.Down && lastPlayerSpeed.y > forceVelocity
                    || orientation == Orientation.Left && lastPlayerSpeed.x > forceVelocity
                    || orientation == Orientation.Right && lastPlayerSpeed.x < -forceVelocity)
                {
                    if (orientation == Orientation.Up)
                    {
                        collision.gameObject.GetComponent<PlayerMovement>().canBlast = true;
                    }
                    collision.gameObject.GetComponent<Rigidbody2D>().velocity = lastPlayerSpeed;
                    animator.SetBool("Slam", true);
                    StartCoroutine(HitPause());
                    OpenGate();
                    Invoke("CloseGate", resetTime);
                }
                else
                {
                    Invoke("OpenGate", timeUntilOpen);
                    Invoke("CloseGate", timeUntilOpen + resetTime);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            if (queueCloseGate)
            {
                CloseGate();
            }
            else
            {
                queueCloseGate = true;
            }
        }
    }

    private void OpenGate()
    {
        this.gameObject.layer = LayerMask.NameToLayer("Default");
        hitBox.enabled = false;
        animator.SetBool("Opened", true);
    }

    private void CloseGate()
    {
        if (queueCloseGate)
        {
            this.gameObject.layer = LayerMask.NameToLayer("Ground");
            hitBox.enabled = true;
            
            queueCloseGate = false;
            animator.SetBool("Opened", false);
            animator.SetBool("Slam", false);
        }
        else
        {
            queueCloseGate = true;
        }
    }

    private IEnumerator HitPause()
    {
        yield return new WaitForSecondsRealtime(0.05f);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitPauseDuration);
        Time.timeScale = 1f;
    }
}

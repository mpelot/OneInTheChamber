using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    private Camera cam;
    public Transform playerTransform;
    private Animator animator;
    private bool aiming = false;

    void Start()
    {
        cam = Camera.main;
        Cursor.visible = false;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        transform.position = cam.ScreenToWorldPoint(mousePos);

        if (Input.GetMouseButtonDown(1) && !aiming)
        {
            aiming = true;
            animator.Play("Crosshair");
        }
        if (Input.GetMouseButtonUp(1) && aiming)
        {
            aiming = false;
        }
        if (!aiming)
        {
            Vector2 dir = playerTransform.position - transform.position;
            dir = Mathf.Abs(dir.x) > Mathf.Abs(dir.y) ? Vector2.right * Mathf.Sign(dir.x) : Vector2.up * Mathf.Sign(dir.y);
            if (dir.x == 1)
                animator.Play("Right");
            else if (dir.x == -1)
                animator.Play("Left");
            else if (dir.y == 1)
                animator.Play("Up");
            else
                animator.Play("Down");
        }
    }
}

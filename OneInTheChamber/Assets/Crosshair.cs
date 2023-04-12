using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        // z needs to be nonzero for this to work
        mousePos.z = cam.nearClipPlane;
        transform.position = cam.ScreenToWorldPoint(mousePos);
    }
}

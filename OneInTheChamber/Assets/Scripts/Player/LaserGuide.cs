using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserGuide : MonoBehaviour
{
    public float laserDistance;
    public float rayCastWidth;
    public int maxBounces;
    LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        hideLaser();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void setLaserDirection(Vector2 aimDirection)
    {
        float remainingDistance = laserDistance;
        Vector2 previousBouncePosition = transform.position;
        Vector2 bounceDirection = aimDirection;
        List<Vector3> linePositions = new List<Vector3>();
        linePositions.Add(transform.position);
        int bounceCount = 1;
        while(remainingDistance > 0 && bounceCount <= maxBounces)
        {
            Vector2 forwardWidthOffset = bounceDirection.normalized * rayCastWidth * 0.5f;
            //Rotate width offset 90 degrees clockwise
            Vector2 sideWidthOffset = new Vector2(forwardWidthOffset.y, -forwardWidthOffset.x);

            RaycastHit2D hit1 = Physics2D.Raycast(previousBouncePosition + sideWidthOffset + forwardWidthOffset, bounceDirection, remainingDistance, LayerMask.GetMask("Ground"));
            RaycastHit2D hit2 = Physics2D.Raycast(previousBouncePosition - sideWidthOffset + forwardWidthOffset, bounceDirection, remainingDistance, LayerMask.GetMask("Ground"));
            RaycastHit2D hit;

            // Determine which raycast hit first
            if (hit1.collider != null && hit2.collider != null)
            {
                if (hit1.distance < hit2.distance)
                {
                    hit = hit1;
                    hit.point -= sideWidthOffset;
                }
                else
                {
                    hit = hit2;
                    hit.point += sideWidthOffset;
                }
            }
            else if (hit1.collider != null)
            {
                hit = hit1;
                hit.point -= sideWidthOffset;
            }
            else if (hit2.collider != null)
            {
                hit = hit2;
                hit.point += sideWidthOffset;
            }
            else
            {
                hit = hit1;
                hit.point -= sideWidthOffset;
            }
            // Push the hit point to the edge of the radius
            hit.point += forwardWidthOffset;

            if (hit.collider != null)
            {
                linePositions.Add(hit.point);
                remainingDistance -= Vector2.Distance(previousBouncePosition, hit.point);
                bounceDirection = Vector2.Reflect(bounceDirection, hit.normal);
                previousBouncePosition = hit.point + bounceDirection * 0.01f;
            }
            else
            {
                linePositions.Add(previousBouncePosition + bounceDirection * remainingDistance);
                remainingDistance = 0;
            }
            bounceCount++;
        }
        lineRenderer.positionCount = bounceCount;
        lineRenderer.SetPositions(linePositions.ToArray());
    }

    public void showLaser()
    {
        lineRenderer.enabled = true;
    }

    public void hideLaser()
    {
        lineRenderer.enabled = false;
    }
}

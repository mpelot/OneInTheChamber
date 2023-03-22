using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserGuide : MonoBehaviour
{
    public float laserDistance;
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
            RaycastHit2D hit = Physics2D.Raycast(previousBouncePosition, bounceDirection, remainingDistance, LayerMask.GetMask("Ground"));
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

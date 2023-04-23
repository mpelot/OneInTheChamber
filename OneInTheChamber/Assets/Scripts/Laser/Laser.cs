using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public enum Direction {Left, Right, Up, Down};
    public Direction currentDirection = Direction.Left;
    public float cooldownTime = 1f;
    public float shootTime = 1f;
    public GameObject laserBeam;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnLasers());
    }

    private IEnumerator SpawnLasers()
    {
        while(true)
        {
            yield return new WaitForSeconds(cooldownTime);
            GameObject currentBeam = Instantiate(laserBeam, transform);
            if(cooldownTime == 0)
            {
                break;
            }
            yield return new WaitForSeconds(shootTime);
        }
    }

}

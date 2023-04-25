using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public enum Direction {Left, Right, Up, Down};
    public Direction currentDirection = Direction.Left;
    public float initialDelay = 0f;
    public float cooldownTime = 1f;
    public float shootTime = 1f;
    public GameObject laserBeam;
    public GameObject sprite;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnLasers());
        // There has to be a better way to do this
        if (currentDirection == Direction.Left)
            sprite.transform.rotation = Quaternion.Euler(0, 0, 180);
        else if (currentDirection == Direction.Up)
            sprite.transform.rotation = Quaternion.Euler(0, 0, 90);
        else if (currentDirection == Direction.Down)
            sprite.transform.rotation = Quaternion.Euler(0, 0, -90);
    }

    private IEnumerator SpawnLasers()
    {
        yield return new WaitForSeconds(initialDelay);
        while(true)
        {
            GameObject currentBeam = Instantiate(laserBeam, transform);
            if(cooldownTime == 0)
            {
                break;
            }
            yield return new WaitForSeconds(shootTime);
            yield return new WaitForSeconds(cooldownTime);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
	[Header("Platform Setup")]
	public Vector3 start;
	public Vector3 end;
	public int ticksToReturn = 100;
	
	private int ticks;
	private bool dir;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		// Update lerp amt
        if (dir) {
			ticks--;
		} else {
			ticks++;
		}
		
		// Mutate direction for next tick
		if (ticks >= this.ticksToReturn) {
			dir = true;
		} else if (ticks <= 0) {
			dir = false;
		}
		
		// Change position
		// TODO: change position of player on top of horizontally moving platform
		transform.position = Vector3.Lerp(start, end, ticks / (float)this.ticksToReturn);
    }
}

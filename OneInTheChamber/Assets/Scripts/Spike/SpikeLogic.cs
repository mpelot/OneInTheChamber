using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpikeLogic : MonoBehaviour
{
	void Update()
	{
	}
	
	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Player") {
			// TODO: cause damage
			//collision.gameObject.transform.position += Vector3.up * 1.5f;
			SceneManager.LoadScene("Testing");
		}
	}
}

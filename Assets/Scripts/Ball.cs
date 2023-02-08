using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
	void OnCollisionEnter(Collision other)
	{
		string tag = other.gameObject.tag;
		//Debug.Log("Collision with: " + tag);

		// If the ball hits the net, reduce the speed.
		if (tag.Equals("Net"))
		{
			GetComponent<Rigidbody>().velocity /= 5f;
		}
	}
}

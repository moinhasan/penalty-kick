using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
	// Goal Scored when ball enters the goal post
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Ball")
		{
			GameManager.Instance.GoalScored();
		}
	}
}

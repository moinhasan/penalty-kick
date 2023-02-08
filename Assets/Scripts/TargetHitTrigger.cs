using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TargetHitTrigger : MonoBehaviour
{
	private int _points = 0;
	
	// Update score when ball enters the goal post
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Ball")
		{
			Debug.Log("Target Hit!");
			GameManager.Instance.TargetHit(_points);
			Destroy(gameObject);
		}
	}

	public void SetPoints(int points)
    {
		_points = points;
		gameObject.GetComponentInChildren<TextMeshPro>().text = string.Format("+{0}", points);
	}
}

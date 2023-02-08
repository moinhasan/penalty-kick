using UnityEngine;
using System.Collections;

///<summary>
///Control camera movement
///</summary>
public class CameraController : MonoBehaviour {
	private Vector3 target = Vector3.zero;   //the target to lock on to
	private float speed = 1.0f; //to control the rotation 
	private bool smooth = true;

	private ShotController shotController;
	private Transform _myTransform;
	
	void Awake() {
		_myTransform = transform;
	}

    private void Start()
    {
		shotController = ShotController.instance;
	}

	void LateUpdate() {
		if (shotController.currentShot == null)
        {
			return;
		} 
		else if (shotController.currentShot.CurrentState == Shot.State.proceed) {
			smooth = true;
			target = shotController.currentShot.Target;
		}
        else
        {
			smooth = true;
			target = Vector3.zero;
		}

		if(smooth) {
				
			//Look at and limit the rotation
			Quaternion rotation = Quaternion.LookRotation(target - _myTransform.position);
			_myTransform.rotation = Quaternion.Slerp(_myTransform.rotation, rotation, Time.deltaTime * speed);
		}
		else { //Just look at
			_myTransform.rotation = Quaternion.LookRotation(target - _myTransform.position);
			//_myTransform.rotation = Quaternion.FromToRotation(-Vector3.forward, (new Vector3(target.x, target.y, target.z) - _myTransform.position).normalized);				
		}	
	}
}
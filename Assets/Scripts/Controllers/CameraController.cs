using UnityEngine;
using System.Collections;
using DG.Tweening;

///<summary>
/// Automatic control camera movement to show the close up shot
///</summary>
public class CameraController : MonoBehaviour {
	private Transform _camTransform;
	private Vector3 _camPosition; // Default camera position
	private Vector3 _movePosition; // The position to move to when shot in progress
	private Vector3 _target = Vector3.zero; // The target to lock on to
	private float _duration = 1.0f; // Movement duration
	private bool _smooth = true; // Smooth transition

	void Awake() {
		GameManager.OnShotUpdate += AdjustCameraAction;
		GameManager.OnGameRestart += ResetCamera;

		_camTransform = transform;
		_camPosition = transform.position;		
	}

	/// <summary>
	/// Adjust camera movement and target based on current shot state 
	/// </summary>
	/// <param name="shot"></param>
	void AdjustCameraAction(Shot shot)
	{
		// When shot starts, get closeup of the shot
		if (shot.CurrentState == Shot.State.Start)
		{
			_smooth = true;
			_target = shot.Target;
			_movePosition = new Vector3(_camPosition.x, _camPosition.y, -10.0f);
		}
		// Set default position and target when shot ends 
		else if (shot.CurrentState == Shot.State.End)
		{
			_smooth = false;
			_target = Vector3.zero;
			_movePosition = _camPosition;
		}

		if (_smooth) SmoothLookAt();
		else LookAt();
	}

	void ResetCamera() {
		_target = Vector3.zero;
		_movePosition = _camPosition;
		LookAt();
	}

	/// <summary>
	/// Smoothly look at the target and move to position
	/// </summary>
	private void SmoothLookAt()
    {
		_camTransform.DOLookAt(_target, _duration).SetUpdate(true);
		_camTransform.DOMove(_movePosition, _duration).SetUpdate(true);
	}

	/// <summary>
	/// Just look at the target
	/// </summary>
	private void LookAt()
	{
		_camTransform.position = _movePosition;
		_camTransform.LookAt(_target);
	}
}
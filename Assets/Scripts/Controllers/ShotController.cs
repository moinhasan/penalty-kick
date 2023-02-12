using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Controles and executes the shot based on players touch input.
/// </summary>
public class ShotController : MonoBehaviour
{
	[SerializeField] private GameObject _ballPrefab;
	[SerializeField] private AnimationCurve _animCurve;

	public bool IsTouchEnable { get; private set; } = false; // Is able to use touch gesture to shoot the ball

	private GameObject _ball;
	private Rigidbody _ballRigidbody;
	private Vector3 _ballInitPostition;

	// Keep tracking of touch positions
	private Vector3 _touchStartPosition, _touchCurrentPosition, _touchPreviousPosition, _touchEndPosition;

	// Keep tracking of touch time
	private float _touchStartTime, _touchEndTime, _swipeDuration;

	private Rect _circlingBox; // Touch input area
	private float _swipeInputTimeLimit = 2.0f; // Input time limit
	private float _minSwipeDistance = 40.0f; // Minimum swipe length required to shoot the ball

	private float _goalDistanceFromCamera = 16f; // Distance between camera and goal post
	private Vector3 _shotTarget; // The target player aimed for
	private float _shootDistance; // Distance between ball and the target
	private int _shotCount; // Number of shots taken
	private float _shotExecutionTime = 5.0f; // Time to execute the shot after input

	private Shot _shot = null; // Shot instance

	// Line (curve) variables  
	private TouchDirection _previousTouchDirection;
	private enum TouchDirection // line direction
	{
		None,
		Straight,
		Left,
		Right
	}
	private List<Vector3> _curvePoints = new List<Vector3>();
	private Vector3 _curvePeakPoint = Vector3.zero; // last touch point before changing direction
	private float _curveTriangleLength = 0.0f;
	private float _curveAngle = 0.0f;

	private static ShotController instance;
	public static ShotController Instance
	{
		get
		{
			return instance;
		}
	}

	void Awake()
	{
		GameManager.OnGameStart += OnGameStart;
		GameManager.OnGameRestart += OnGameRestart;
		GameManager.OnGamePause += OnGamePause;
		GameManager.OnGameResume += OnGameResume;

		GameManager.OnGoalScored += OnGoalScored;
		GameManager.OnTargetHit += OnTargetHit;

		// Create Instance
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void OnGameStart()
	{
		StartPlay();
	}

	void OnGamePause()
    {
		DisableTouch();
	}

	void OnGameResume()
	{
		_isTouchPhase = false;
		EnableTouch();
	}

	void OnGameRestart()
	{
		_isTouchPhase = false;
		RestartPlay();
	}

	// Update shot and result on the hud
	void OnGoalScored()
	{
		if (!_shot.IsSuccess)
		{
			_shot.ProcessGoalScore(hasScored: true);
			_shot.ChangeState(Shot.State.Result);
			GameManager.Instance.ShotUpdate(_shot);
		}
	}

	// Update shot on target hit
	void OnTargetHit(int points)
	{
		_shot.ProcessTargetHit(points);
	}

	void Start()
    {
		_ballInitPostition = new Vector3(0.0f, 0.15f, -11f); // Place the ball above the ground 11m in front of the centre of the goal
		_minSwipeDistance = ScreenFactors.ConvertBaseToActual(_minSwipeDistance);
	}

	private void StartPlay()
	{
		_shotCount = 0;
		_shot = null;
		CreateBall();
		EnableTouch();
	}

	private void RestartPlay()
    {
		StopAllCoroutines();
		DestroyBall();
        StartPlay();
    }

	/// <summary>
	/// Wait to complete executing the current shot then end the play session
	/// </summary>
	/// <returns></returns>
	private void EndPlay()
	{
		GameManager.Instance.EndGame();
	}

	void CreateBall()
	{
		_ball = Instantiate(_ballPrefab, _ballInitPostition, Quaternion.identity);
		_ballRigidbody = _ball.GetComponent<Rigidbody>();
		_ballRigidbody.maxAngularVelocity = 90.0f;
	}

	void DestroyBall()
	{
		Destroy(_ball);
	}

	void Update()
	{
		if (_ball == null) return;
		GetTouchInput();
	}

	private bool _isTouchPhase = false;
	private void GetTouchInput() // touch input to execute the shot
	{
		//if (currentShot == null || currentShot.CurrentState != Shot.State.Ready) return;
		if (IsTouchEnable)
		{
			if (Input.GetMouseButtonDown(0)) // touch phase began
			{
				_isTouchPhase = true;				
				TouchBegin(Input.mousePosition);
				StartCoroutine(ShootOnTimeUp(_swipeInputTimeLimit));
            }
			else if (Input.GetMouseButton(0) && _isTouchPhase) // touch continues
			{
				TouchMove(Input.mousePosition);
			}
			else if (Input.GetMouseButtonUp(0) && _isTouchPhase) // touch ends
			{
				_isTouchPhase = false;
				TouchEnd(Input.mousePosition);				
			}
		}
	}

	/// <summary>
    /// Make the shot when the input time is over.
    /// </summary>
    /// <param name="time"> Time to complete the shot input</param>
    /// <returns></returns>
	private IEnumerator ShootOnTimeUp(float time)
	{
		yield return new WaitForSeconds(time);

		if (IsTouchEnable && _isTouchPhase)
		{
			Debug.Log("Shoot TimeUp!");
			_isTouchPhase = false;
			TouchEnd(Input.mousePosition);
		}
	}

	public void TouchBegin(Vector3 position)
	{
		// Reset touch variables 
		_touchStartTime = Time.time;
		_touchStartPosition = _touchPreviousPosition = _touchCurrentPosition = position;

		// Reset curve (line) variables 
		_curvePoints = new List<Vector3>();
		_curvePeakPoint = _touchStartPosition;
		_curveTriangleLength = 0.0f;
		_curveAngle = 0.0f;

		// Create a rect area to keep record of the touch input area
		_circlingBox = new Rect(_touchStartPosition.x, _touchStartPosition.y, 0f, 0f);
	}

	public void TouchMove(Vector3 position)
	{
        // Check if touch phase moved min distance
        if (Vector3.Distance(_touchPreviousPosition, position) < _minSwipeDistance)
        {
            return;
        }

        _touchPreviousPosition = _touchCurrentPosition;
		_touchCurrentPosition = position;

		// Expand the circling box depending on touch input area
		if (_touchCurrentPosition.x < _circlingBox.xMin)
			_circlingBox.xMin = _touchCurrentPosition.x;
		if (_touchCurrentPosition.x > _circlingBox.xMax)
			_circlingBox.xMax = _touchCurrentPosition.x;
		if (_touchCurrentPosition.y < _circlingBox.yMin)
			_circlingBox.yMin = _touchCurrentPosition.y;
		if (_touchCurrentPosition.y > _circlingBox.yMax)
			_circlingBox.yMax = _touchCurrentPosition.y;

		CalculateCurve();
	}

	/// <summary>
	/// Calculates if the touch movement is clockwise or counter clockwise,
	/// to decide start of a new curve in different direction,
	/// compare and select the large curve for shot input 
	/// ref: https://algs4.cs.princeton.edu/91primitives/
	/// </summary>
	private void CalculateCurve()
	{
		Vector2 b = _touchPreviousPosition;
		Vector2 c = _touchCurrentPosition;
		Vector2 a = _circlingBox.center; // Center of the touch input area
		int moveDirectionValue = (int)((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x));

		TouchDirection mouseDirection = TouchDirection.Straight;

		if (moveDirectionValue > 0) mouseDirection = TouchDirection.Left; // counterclock wise
		else if (moveDirectionValue < 0) mouseDirection = TouchDirection.Right; // clock wise
		else mouseDirection = TouchDirection.Straight; // straight or not moving

		if (_previousTouchDirection == TouchDirection.None)
		{
			_previousTouchDirection = mouseDirection;
			_curvePoints.Add(_touchCurrentPosition);
			return;
		}

		// Change of direction, next curve starts
		if ((mouseDirection != TouchDirection.Straight) && (mouseDirection != _previousTouchDirection))
		{
			UpdateCurveDetails();

			_previousTouchDirection = mouseDirection;

			// NOTE: This can be the intersection point of previous curve and line formed by brevious curve start point and current curve end position
			_curvePoints.Add(_curvePeakPoint); // New curve start position

			_curvePoints.Add(_touchPreviousPosition);
			//CurvePoints.Add(mouseCurrentPosition); 
		}
		// Start of first curve
		else if (_previousTouchDirection == TouchDirection.None)
		{
			_previousTouchDirection = mouseDirection;
		}

		_curvePoints.Add(_touchCurrentPosition);
	}

	/// <summary>
	/// Compare selected curve size with the current curve size/length and keep the bigger curve
	/// </summary>
	private void UpdateCurveDetails()
	{
		Vector3 currentCurvePeakPoint = Vector3.zero;
		float currentCurveTriangleLength = 0.0f;
		float currentCurveAngle = 0.0f;
		Mathematics.GetCurveDetails(_curvePoints, out currentCurvePeakPoint, out currentCurveTriangleLength, out currentCurveAngle);

		// TODO: Comparison can be based on curve size/length or curve angle. For now considering size.
		if (currentCurveTriangleLength >= _curveTriangleLength)
		{
			_curvePeakPoint = currentCurvePeakPoint;
			_curveTriangleLength = currentCurveTriangleLength;
			_curveAngle = currentCurveAngle;
		}

		// Remove existing curve
		_curvePoints.Clear();
	}

	public void TouchEnd(Vector3 position)
	{
		_touchEndTime = Time.time;
		_swipeDuration = (_touchEndTime - _touchStartTime); // Duration of swipe gesture

		DisableTouch();
		UpdateCurveDetails();

		_touchEndPosition = Input.mousePosition;

		if (Vector3.Distance(_touchEndPosition, _touchStartPosition) > _minSwipeDistance)
		{
			// Kick the ball

			// Target position in the scene
			Vector3 targetPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _goalDistanceFromCamera));

			// Ground shot, ball can't go below ground. So we need to find the shot direction on the ground
			if (targetPosition.y < 0.0f) 
			{
				Ray ray = Camera.main.ScreenPointToRay(_touchEndPosition);
				if(Physics.Raycast(ray, out RaycastHit hitInfo))
                {
					targetPosition = hitInfo.point;
				}
				// to keep the target on the surface 
                targetPosition.y = 0.15f;
			}

			Vector3 shootDirection = targetPosition - _ball.transform.position;

			// Find shoot target on goal line 
			_shotTarget = Mathematics.FindIntersection(_ball.transform.position, shootDirection, Vector3.zero, Vector3.right);

			//if (shootTarget.y < 0.15f) shootTarget.y = 0.15f; // Ground rolling shot, ball can't go below ground.
			_shotTarget.x = Math.Clamp(_shotTarget.x, -4.5f, 4.5f); // keep target near goal post.
            _shotTarget = new Vector3(_shotTarget.x, _shotTarget.y, 0f);
			_shootDistance = Vector3.Distance(_ball.transform.position, _shotTarget);

			StartCoroutine(ExecuteShot()); // Execute the Shot
		}
        else
        {
			Debug.Log("Flick distance < minFlickDistance : " + Vector3.Distance(_touchEndPosition, _touchStartPosition));
			EnableTouch();
		}
	}

	/// <summary>
	/// Setup a new shot to execute. Wait to complete executing the shot, then enable input for next shot.
	/// </summary>
	/// <returns></returns>
	private IEnumerator ExecuteShot()
	{
		// 1. Create a new shot
		_shotCount = _shotCount + 1;	
		_shot = new Shot(	_ball,
							_shotCount,
							GameDataManager.Level.goalPoint,
							_shotTarget,
							GameDataManager.Player.skill.accuracy,
							GetBallSpeed(),
							GetUpwardForce(),
							GetSwingForce()
						);

		// 2. Start the shot
		_shot.ChangeState(Shot.State.Start);
		GameManager.Instance.ShotUpdate(_shot);

		// 3. Wait to complete executing the shot
		yield return new WaitForSeconds(_shotExecutionTime);

		// 4. End the shot
		_shot.ChangeState(Shot.State.End);
		GameManager.Instance.ShotUpdate(_shot);

		// If shots remaining for the session, initialize the ball and enable input for the next shot
		// else end the session
		if (_shotCount < GameDataManager.Level.shots)
		{
			InitializeBall();
			EnableTouch();
		}
		else
		{
			EndPlay(); // End the play session
		}
	}

	/// <summary>
	/// Place the ball at the initial position and reset all parameters
	/// </summary>
	private void InitializeBall()
	{
		_ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
		_ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		_ball.transform.rotation = Quaternion.identity;
		_ball.transform.position = _ballInitPostition;
		_ball.GetComponentInChildren<TrailRenderer>().Clear();
	}

	void FixedUpdate()
	{
		if (_shot == null || _shot.CurrentState != Shot.State.Start) return;

		// Calculate the direction and distance to the target
		Vector3 direction = _shot.Target - _ball.transform.position;
		float distance = direction.magnitude;

        // Continue heading towards the goal line till it's close enough.
        if (_ball.transform.position.z < -0.5f)
		{
            // Launch the ball by setting its velocity
            _ballRigidbody.velocity = direction.normalized * _shot.Speed;

			// Upward speed depends on ball speed
			// Add a upward force to the ball, perpendicular to the direction of the launch, taking into account direction of the launch
			float upwardForce = _shot.UpwardForce * (_animCurve.Evaluate(distance / _shootDistance));
			_ballRigidbody.AddForce(Quaternion.Euler(-90, 0, 0) * direction.normalized * upwardForce, ForceMode.VelocityChange);

			// Swing speed depends on curve angle
            // Add a swing force to the ball, perpendicular to the direction of the launch, taking into account direction of the launch
            float swingForce = _shot.SwingForce * (_animCurve.Evaluate(distance / _shootDistance));
			_ballRigidbody.AddForce(Quaternion.Euler(0, 90, 0) * direction.normalized * swingForce, ForceMode.VelocityChange);

			// Set ball angular velocity to make it spin along it's own axis
			_ballRigidbody.angularVelocity = GetAngularVelocity(_shot.SwingForce);
        }
        else
        {
			_shot.ChangeState(Shot.State.Aftermath);
			GameManager.Instance.ShotUpdate(_shot);
		}
	}

	/// <summary>
	/// Calculate the ball speed based on swipe duration. Swipe time upto 0.2s is considered as fastest swipe and
	/// slowest swipe time is when it reaches Swipe input time limit. Minimum speed required for the ball to reach the
	/// goal is 10.0f m/s and maximum speed depends on player skill. 
	/// </summary>
	/// <returns>speed</returns>
	private float GetBallSpeed()
	{
		float shortestSwipeTime = 0.2f;
		float longestSwipeTime = _swipeInputTimeLimit;

		//Can't exceed 50.0f or go below 10.0f;
		float minSpeed = 10.0f;
		float maxSpeed = 50.0f;

		// Player kick speed value check, between 0 -> 10
		float kickSpeed = Mathf.Clamp(GameDataManager.Player.skill.speed, 0, 10);

		Mathf.Clamp(GameDataManager.Player.skill.speed, minSpeed, 50.0f); 

		// Adjust max speed from player kick speed value
		maxSpeed = Mathf.Lerp(minSpeed, maxSpeed, kickSpeed / 10);

		// Keep the swipe duration within the limit.
		_swipeDuration = Mathf.Clamp(_swipeDuration, shortestSwipeTime, longestSwipeTime);

		// Get the final speed within the min, max speed range based on slow, fast swipe time range.
        // Long time -> slow speed, short time -> fast speed.
		float speed = minSpeed + ((maxSpeed - minSpeed) * (longestSwipeTime - _swipeDuration) / (longestSwipeTime - shortestSwipeTime));

		return speed;
	}

	/// <summary>
	/// Calculate the ball upward speed based on swipe duration. Slow high shots needs more air time.  
	/// </summary>
	/// <returns>force</returns>
	private float GetUpwardForce()
	{
		float longestSwipeTime = _swipeInputTimeLimit;
		float maxUpwardSpeed = 5.0f;
		 
		// keep the swipe duration within limit
		_swipeDuration = Mathf.Clamp(_swipeDuration, 0.0f, longestSwipeTime);

		// shot time -> less up speed, long time -> more up speed.
		float force = Mathf.Lerp(0, maxUpwardSpeed, _swipeDuration / longestSwipeTime);
		//float speed = (maxUpwardSpeed * swipeDuration / slowestSwipeTime);

		return force;
	}

	/// <summary>
	/// Calculate the swing force based on input curve angle and player curve skill.
	/// Large curve angle will produce high swing force
	/// </summary>
	/// <returns>force</returns>
	private float GetSwingForce()
	{
		float maxCurveAngle = 60.0f;
		float maxCurveValue = 10.0f;

		// Player curve skill value check, between 0 -> 10
		float curveValue = Mathf.Clamp(GameDataManager.Player.skill.curve, 0, 10);

		// Find curve value within the Curve Value range based on user curve skill value
		curveValue = Mathf.Lerp(0, maxCurveValue, curveValue / 10);

		// Input curve angle check, between -90 -> 90
		_curveAngle = Mathf.Clamp(_curveAngle, -maxCurveAngle, maxCurveAngle);

		// Evaluate curve/swing force (with +/- direction) based on curveAngle 
		float force = (curveValue * _curveAngle / maxCurveAngle);

        return force;
	}

	/// <summary>
	/// Angular velocity should depend on ball speed and swing direction.
    /// [NOTE] Need to improve this part.
	/// </summary>
	/// <returns>force</returns>
	private Vector3 GetAngularVelocity(float swingForce)
	{
		float maxAngularVelocityY = 30.0f;
		float maxCurveValue = 10.0f;

		// Evaluate angular velocity (with +/- direction) based on swing force 
		float angularVelocityY = (maxAngularVelocityY * -swingForce / maxCurveValue);

		// For now assigning a fixed value to X 
		Vector3 angularVelocity = new Vector3(5.0f, angularVelocityY, 0f);

		return angularVelocity;
	}

	private void EnableTouch(float delay = 0.0f)
	{
		if (delay <= 0.0f) IsTouchEnable = true;
		else
		{
			StartCoroutine(_EnableTouch(delay));
		}
	}

	private IEnumerator _EnableTouch(float delay)
	{
		yield return new WaitForSeconds(delay);
		IsTouchEnable = true;
	}

	private void DisableTouch()
	{
		IsTouchEnable = false;
	}

	void OnDestroy()
	{
		GameManager.OnGameStart -= OnGameStart;
		GameManager.OnGameRestart -= OnGameRestart;
		GameManager.OnGamePause -= OnGamePause;
		GameManager.OnGameResume -= OnGameResume;
		GameManager.OnGoalScored -= OnGoalScored;
		GameManager.OnTargetHit -= OnTargetHit;
	}

}

/// <summary>
/// Contains initial values and states and implementations of a single Shot 
/// </summary>
public class Shot
{
	public enum State
	{
		Ready, // Shot Iniitialized, ready for input
		Start, // Shot started
		Result, // Goal success or fail
		Aftermath, // Continuing movement after reaching the target
		End // Shot ended
	}
	public State CurrentState { get; private set; } // Current state of the shot
	public int Number { get; private set; } // Shot number (of a single game session) 
	public int Point { get; private set; } // Point for a successful shot on goal
	public bool IsSuccess { get; private set; } // Is it a successful shot on goal
	public int Score { get; private set; } // Total number of points scored from the shot
	public Vector3 Postition { get; private set; } // Shot postition (Ball start position)
	public Vector3 Target { get; private set; } // Shot target
	public GameObject Ball { get; private set; } // Ball object to shoot

	public float Speed { get; private set; }
	public float Accuracy { get; private set; }
	public float UpwardForce { get; private set; }
	public float SwingForce { get; private set; }

	/// <summary>
	/// Provide access to all information of a single shot and provides some basic functions to update shot information
	/// </summary>
	/// <param name="ball">Ball object to shoot</param>
	/// <param name="number">Shot number</param>
	/// <param name="point">Point for a successful shot on goal</param>
	/// <param name="target"></param>
	/// <param name="accuracy"></param>
	/// <param name="speed"></param>
	/// <param name="upwardForce"></param>
	/// <param name="swingForce"></param>
	public Shot(GameObject ball, int number, int point, Vector3 target, float accuracy, float speed,
				float upwardForce, float swingForce)
	{
		Number = number;
		Point = point;
		IsSuccess = false;
		Score = 0;

		Postition = new Vector3(0f, 0.15f, -11f);
		Target = AdjustTarget(target, accuracy);
		Speed = speed;
		UpwardForce = upwardForce;
		SwingForce = swingForce;

		Ball = ball;
		CurrentState = State.Ready;
	}

	/// <summary>
	/// Adjust the final target based on accuracy. more accuracy more precise target. 
	/// </summary>
	/// <param name="actualTarget"></param>
	/// <param name="accuracy"></param>
	/// <returns></returns>
	private Vector3 AdjustTarget(Vector3 actualTarget, float accuracy)
	{
		// Accuracy check, between 0 -> 10
		accuracy = Mathf.Clamp(accuracy, 0.0f, 10.0f);

		// Get adjustment amount from adjustment range (0 - 1 unit) based on accuracy skill
		float adjustmentAmount = Mathf.Lerp(1.0f, 0.0f, accuracy / 10);

		// Get x, y position adjustment randomly from adjustment amount
		float xAdjustment = UnityEngine.Random.Range(-adjustmentAmount, adjustmentAmount);
		float yAdjustment = UnityEngine.Random.Range(-adjustmentAmount, adjustmentAmount);

		// Return final target after adding adjustment to the actual target
		return new Vector3(actualTarget.x + xAdjustment, actualTarget.y + yAdjustment, actualTarget.z);
	}

	public void ChangeState(State state)
	{
		CurrentState = state;
	}

	public void ProcessGoalScore(bool hasScored)
	{
		IsSuccess = hasScored;
		if (hasScored)
		{
			Score = Score + Point;
		}

		Debug.Log("ProcessGoalScore: " + Score);
	}

	public void ProcessTargetHit(int amount)
	{
		Score = Score + amount;
		Debug.Log("ProcessTargetHit: " + Score);
	}

	public void SetTarget(Vector3 target)
	{
		Target = target;
	}
}
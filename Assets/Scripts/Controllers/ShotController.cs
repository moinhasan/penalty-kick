using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShotController : MonoBehaviour
{
	public static ShotController instance;

	[SerializeField] private GameObject ballPrefab;
	[SerializeField] private GameObject curvePrefab;
	[SerializeField] private float ballSpeed;
	[SerializeField] private float ballCurve;
	[SerializeField] private AnimationCurve _curve;

	public bool IsTouchEnable { get; private set; } = false; // is able to use touch gesture to shoot the ball

	private GameObject ballInstance;
	private Rigidbody ballRigidbody;

	private Vector3 touchStartPosition, touchCurrentPosition, touchPreviousPosition, touchEndPosition;
	float touchStartTime, touchEndTime, swipeDuration;

	private Rect circlingBox;
	private Vector3 _ballInitPostition;
	private float swipeInputTimeLimit = 2.0f;
	private float minSwipeDistance = 40.0f; // minimum swipe length required to shoot the ball 
	private float goalDistanceFromCamera = 16f; // Distance between camera and goal post
	private Vector3 shootTarget;
	private float shootDistance;

	public Shot currentShot { get; private set; } = null;

	public TextMeshProUGUI scoreText;

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
		RestartPlay();
	}

	void OnGoalScored()
	{
		if (!currentShot.IsSuccess) //replace it with disabling trigger
		{
			currentShot.ProcessGoalScore(hasScored: true);
			ScoreManager.Instance.ProcessResult(currentShot);
		}
	}

	void OnTargetHit(int points)
	{
		currentShot.ProcessTargetHit(points);
	}

	void Start()
    {
		_ballInitPostition = new Vector3(0.0f, 0.15f, -11f); // Place the ball above the ground 11m in front of the centre of the goal
		minSwipeDistance = ScreenFactors.ConvertBaseToActual(minSwipeDistance);
	}

    public void StartPlay()
	{
		currentShot = null;
		StartCoroutine(SetUpShot(1.0f));
	}

    public void RestartPlay()
    {
		StopAllCoroutines();
		DestroyBall();
        StartPlay();
    }

    void CreateBall()
	{
		ballInstance = Instantiate(ballPrefab, _ballInitPostition, Quaternion.identity);
		ballRigidbody = ballInstance.GetComponent<Rigidbody>();
		ballRigidbody.maxAngularVelocity = 90.0f;
	}

	void DestroyBall()
	{
		Destroy(ballInstance);
	}

	void Update()
	{
		if (ballInstance == null) return;
		GetTouchInput();
	}

	public bool _isTouchPhase = false;
	private void GetTouchInput() // touch input to execute the shot
	{
		if (currentShot == null || currentShot.CurrentState != Shot.State.Ready) return;
		if (IsTouchEnable)
		{
			if (Input.GetMouseButtonDown(0)) // touch phase began
			{
				_isTouchPhase = true;				
				TouchBegin(Input.mousePosition);
				StartCoroutine(ShootOnTimeUp(swipeInputTimeLimit));
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
		touchStartTime = Time.time;
		touchStartPosition = touchPreviousPosition = touchCurrentPosition = position;

		curvePoints = new List<Vector3>();
		curvePeakPoint = touchStartPosition;
		curveTriangleLength = 0.0f;
		curveAngle = 0.0f;

		circlingBox = new Rect(touchStartPosition.x, touchStartPosition.y, 0f, 0f);
	}

	public void TouchMove(Vector3 position)
	{
        // check if touch phase moved min distance
        if (Vector3.Distance(touchPreviousPosition, position) < minSwipeDistance) //mouseCurrentPosition == position 
        {
            return;
        }

        touchPreviousPosition = touchCurrentPosition;
		touchCurrentPosition = position;

		if (touchCurrentPosition.x < circlingBox.xMin)
			circlingBox.xMin = touchCurrentPosition.x;
		if (touchCurrentPosition.x > circlingBox.xMax)
			circlingBox.xMax = touchCurrentPosition.x;
		if (touchCurrentPosition.y < circlingBox.yMin)
			circlingBox.yMin = touchCurrentPosition.y;
		if (touchCurrentPosition.y > circlingBox.yMax)
			circlingBox.yMax = touchCurrentPosition.y;

		//CalcCurve(mouseCurrentPosition);
		CalculateCurve();
	}

	public void TouchEnd(Vector3 position)
	{
		touchEndTime = Time.time;
		swipeDuration = (touchEndTime - touchStartTime); // Duration of swipe gesture

		DisableTouch();
		UpdateCurveDetails();

		touchEndPosition = Input.mousePosition;

		if (Vector3.Distance(touchEndPosition, touchStartPosition) > minSwipeDistance)
		{
			// Kick the ball

			// Target position in the scene
			Vector3 targetPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, goalDistanceFromCamera));

			// Ground shot, ball can't go below ground. So we need to find the shot direction on the ground
			if (targetPosition.y < 0.0f) 
			{
				Ray ray = Camera.main.ScreenPointToRay(touchEndPosition);
				if(Physics.Raycast(ray, out RaycastHit hitInfo))
                {
					targetPosition = hitInfo.point;
				}
				// to keep the target on the surface 
                targetPosition.y = 0.15f;
			}

			Vector3 shootDirection = targetPosition - ballInstance.transform.position;

			// Find shoot target on goal line 
			shootTarget = Mathematics.FindIntersection(ballInstance.transform.position, shootDirection, Vector3.zero, Vector3.right);

			//if (shootTarget.y < 0.15f) shootTarget.y = 0.15f; // Ground rolling shot, ball can't go below ground.
			shootTarget.x = Math.Clamp(shootTarget.x, -4.5f, 4.5f); // keep target near goal post.
            shootTarget = new Vector3(shootTarget.x, shootTarget.y, 0f);

            shootDistance = Vector3.Distance(ballInstance.transform.position, shootTarget); //null ref

			currentShot.SetTarget(shootTarget);
			currentShot.ChangeState(Shot.State.proceed);

			if (currentShot.Number < 5)
            {
				StartCoroutine(SetUpShot(5.0f));
			}
            else
            {
				StartCoroutine(EndGame(5.0f));
            }
        }
        else
        {
			Debug.Log("Flick distance < minFlickDistance : " + Vector3.Distance(touchEndPosition, touchStartPosition));
			EnableTouch();
		}
	}

	private IEnumerator SetUpShot(float delay)
    {
		yield return new WaitForSeconds(delay);

		if (currentShot == null) // First shot
        {
			CreateBall();
			currentShot = new Shot(ballInstance, 1, 1);
        }
        else
        {
			//ResetBall();
			currentShot = new Shot(ballInstance, currentShot.Number + 1, 1);
		}

		touchStartPosition = touchCurrentPosition = touchPreviousPosition = touchEndPosition = Vector3.zero;
		EnableTouch();
	}

	private IEnumerator EndGame(float delay)
    {
		yield return new WaitForSeconds(delay);
		MainMenuController.instance.GameOver(0);
	}

	void FixedUpdate()
	{
		if (currentShot == null || currentShot.CurrentState != Shot.State.proceed) return;
		
		// calculate the direction and distance to the target
		Vector3 direction = shootTarget - ballInstance.transform.position;
		float distance = direction.magnitude;

		//Debug.Log(distance);
		// if the object is close enough to the target, apply a force to hit it
		if (distance > 0.5f)
		{
            // launch the ball by setting its velocity
            ballRigidbody.velocity = direction.normalized * GetBallSpeed();

			// Upward speed depends on ball speed
			// Add a upward force to the ball, perpendicular to the direction of the launch, taking into account direction of the launch
			float upwardForce = GetUpwardForce() * (_curve.Evaluate(distance / shootDistance));
			//Debug.Log("upwardSpeed: " + upwardSpeed);
			ballRigidbody.AddForce(Quaternion.Euler(-90, 0, 0) * direction.normalized * upwardForce, ForceMode.VelocityChange);

			// Swing speed depends on curve angle
            // Add a swing force to the ball, perpendicular to the direction of the launch, taking into account direction of the launch
            float swingForce = GetSwingForce() * (_curve.Evaluate(distance / shootDistance));
			//Debug.Log("swingSpeed: " + swingSpeed);
			ballRigidbody.AddForce(Quaternion.Euler(0, 90, 0) * direction.normalized * swingForce, ForceMode.VelocityChange);

			// Set ball angular velocity to make it spin along it's own axis
			ballRigidbody.angularVelocity = GetAngularVelocity(GetSwingForce());
		}
        else
        {
			currentShot.ChangeState(Shot.State.end);
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
		float longestSwipeTime = swipeInputTimeLimit;

		//Can't exceed 50.0f or go below 10.0f;
		float minSpeed = 10.0f;
		float maxSpeed = 50.0f;

		// Player kick speed value check, between 0 -> 10
		float kickSpeed = Mathf.Clamp(GameDataManager.Player.skill.speed, 0, 10);

		Mathf.Clamp(GameDataManager.Player.skill.speed, minSpeed, 50.0f); 

		// Adjust max speed from player kick speed value
		maxSpeed = Mathf.Lerp(minSpeed, maxSpeed, kickSpeed / 10);

		// Keep the swipe duration within the limit.
		swipeDuration = Mathf.Clamp(swipeDuration, shortestSwipeTime, longestSwipeTime);

		// Get the final speed within the min, max speed range based on slow, fast swipe time range.
        // Long time -> slow speed, short time -> fast speed.
		float speed = minSpeed + ((maxSpeed - minSpeed) * (longestSwipeTime - swipeDuration) / (longestSwipeTime - shortestSwipeTime));

		return speed;
	}

	/// <summary>
	/// Calculate the ball upward speed based on swipe duration. Slow high shots needs more air time.  
	/// </summary>
	/// <returns>force</returns>
	private float GetUpwardForce()
	{
		float longestSwipeTime = swipeInputTimeLimit;
		float maxUpwardSpeed = 5.0f;
		 
		// keep the swipe duration within limit
		swipeDuration = Mathf.Clamp(swipeDuration, 0.0f, longestSwipeTime);

		// shot time -> less up speed, long time -> more up speed.
		float force = Mathf.Lerp(0, maxUpwardSpeed, swipeDuration / longestSwipeTime);
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
		curveAngle = Mathf.Clamp(curveAngle, -maxCurveAngle, maxCurveAngle);

		// Evaluate curve/swing force (with +/- direction) based on curveAngle 
		float force = (curveValue * curveAngle / maxCurveAngle);

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
		Vector3 angularVelocity = new Vector3(0, angularVelocityY, 0f);

		return angularVelocity;
	}

	public MouseDirection previousMouseDirection;
	public enum MouseDirection // line/curve direction
	{
		None,
		Straight,
		Left,
		Right
	}

	public List<Vector3> curvePoints = new List<Vector3>();
	public Vector3 curvePeakPoint = Vector3.zero; // last touch point before changing direction
	public float curveTriangleLength = 0.0f;
	public float curveAngle = 0.0f;

	/// <summary>
	/// Calculates if the touch movement is clockwise or counter clockwise,
    /// to decide start of a new curve in different direction,
    /// compare and select the large curve for shot input 
	/// ref: https://algs4.cs.princeton.edu/91primitives/
	/// </summary>
	private void CalculateCurve()
	{
		Vector2 b = touchPreviousPosition;
		Vector2 c = touchCurrentPosition;
		Vector2 a = circlingBox.center;
		int moveDirectionValue = (int)((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x));

		MouseDirection mouseDirection = MouseDirection.Straight;

		if (moveDirectionValue > 0) mouseDirection = MouseDirection.Left; // counterclock wise
		else if (moveDirectionValue < 0) mouseDirection = MouseDirection.Right; // clock wise
		else mouseDirection = MouseDirection.Straight; // straight or not moving

		if (previousMouseDirection == MouseDirection.None)
        {
            previousMouseDirection = mouseDirection;
			curvePoints.Add(touchCurrentPosition);
			return;
        }

        // Change of direction, next curve starts
        if ((mouseDirection != MouseDirection.Straight) && (mouseDirection != previousMouseDirection))
		{
			UpdateCurveDetails();

			previousMouseDirection = mouseDirection;

            // NOTE: This can be the intersection point of previous curve and line formed by brevious curve start point and current curve end position
			curvePoints.Add(curvePeakPoint); // New curve start position

			curvePoints.Add(touchPreviousPosition);
			//CurvePoints.Add(mouseCurrentPosition); 
		}
		// Start of first curve
        else if (previousMouseDirection == MouseDirection.None)
		{
			previousMouseDirection = mouseDirection;
		}

		curvePoints.Add(touchCurrentPosition);
	}

	/// <summary>
	/// Compare selected curve size with the current curve size/length and keep the bigger curve
	/// </summary>
	private void UpdateCurveDetails()
	{
		Vector3 currentCurvePeakPoint = Vector3.zero;
		float currentCurveTriangleLength = 0.0f;
		float currentCurveAngle = 0.0f;
		Mathematics.GetCurveDetails(curvePoints, out currentCurvePeakPoint, out currentCurveTriangleLength, out currentCurveAngle);

		// TODO: Comparison can be based on curve size/length or curve angle. For now considering size.
		if (currentCurveTriangleLength >= curveTriangleLength)
        {
			curvePeakPoint = currentCurvePeakPoint;
			curveTriangleLength = currentCurveTriangleLength;
			curveAngle = currentCurveAngle;
		}

		// Remove existing curve
		curvePoints.Clear();
	}

	private void EnableTouch()
	{		
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
		Ready,
		proceed,
		Ongoing,
		end
	}
	public State CurrentState { get; private set; } // Current state of the shot
	public int Number { get; private set; } // Shot number (of a single game session) 
	public int Point { get; private set; } // Point for a successful shot on goal
	public bool IsSuccess { get; private set; } // Is it a successful shot on goal
	public int Score { get; private set; } // Total number of points scored from the shot
	public Vector3 Postition { get; private set; } // Shot postition (Ball start position)
	public Vector3 Target { get; private set; } // Shot target
	public GameObject Ball { get; private set; } // Ball object to shoot

	public float Power { get; private set; } // Speed
	public float Accuracy { get; private set; }
	public float Curve { get; private set; }

	/// <summary>
	/// Shot constructor
	/// </summary>
	/// <param name="ball">Ball object to shoot</param>
	/// <param name="number">Shot number</param>
	/// <param name="point">Point for a successful shot on goal</param>
	public Shot(GameObject ball, int number, int point)
	{
		Number = number;
		Point = point;
		CurrentState = State.Ready;
		IsSuccess = false;
		Score = 0;
		Postition = new Vector3(0f, 0.15f, -11f);
		Target = Vector3.zero;
		Ball = ball;
		InitializeBall();
	}

	private void InitializeBall()
	{
		Ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
		Ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		Ball.transform.rotation = Quaternion.identity;
		Ball.transform.position = Postition;
		Ball.GetComponentInChildren<TrailRenderer>().Clear();
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
using UnityEngine;

public class HitTarget : MonoBehaviour
{
    public Transform target; // the target to hit
    public float force; // the force with which the object will hit the target
    public float currentSwing;
    public float swing;
    public float gravity;
    public AnimationCurve _curve;
    public float initDistance;
    public float curveValue;
    public float distance;
    private Rigidbody rb; // the Rigidbody component of the object

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initDistance = Vector3.Distance(transform.position, target.position);
    }

    void FixedUpdate()
    {
        if (!Input.GetMouseButton(0))
        {
            return;
        }
        // calculate the direction and distance to the target
        Vector3 direction = target.position - transform.position;
        distance = direction.magnitude;
        //Debug.Log(distance);
        // if the object is close enough to the target, apply a force to hit it
        if (distance > 0.1f)
        {
            //// calculate the initial velocity needed to hit the target, taking into account gravity
            //Vector3 velocity = CalculateLaunchVelocity(transform.position, target.position, gravity);

            rb.AddForce(direction.normalized * force, ForceMode.Impulse);


            //// add a swing force to the ball, perpendicular to the direction of the launch
            //rb.AddForce(Quaternion.Euler(0, 90, 0) * velocity.normalized * swing, ForceMode.Impulse);

            //// launch the ball by setting its velocity
            //rb.velocity = direction.normalized * force;

            // calculate the initial velocity needed to hit the target, taking into account gravity
            //Vector3 velocity = CalculateLaunchVelocity(transform.position, target.position, gravity);
            rb.velocity = direction.normalized * force;
            curveValue = _curve.Evaluate(distance / initDistance);
            currentSwing = swing * curveValue;
            // add a swing force to the ball, perpendicular to the direction of the launch
            rb.AddForce(Quaternion.Euler(0, 90, 0) * direction.normalized * swing, ForceMode.VelocityChange);

            // launch the ball by setting its velocity
            //rb.velocity = direction.normalized * force;
            //rb.AddForce(direction.normalized * force, ForceMode.Force);

        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Calculate the direction from the ball to the target
            Vector3 direction = (target.position - transform.position).normalized;

            // Calculate the upwards force based on the desired height and gravity
            float upwardsForce = Mathf.Sqrt(2f * Physics.gravity.magnitude * 4f);

            // Add the force to the ball in the direction of the target, with an upwards force
            rb.AddForce(direction * force + Vector3.up * upwardsForce, ForceMode.VelocityChange);
        }
    }

    Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 end, float gravity)
    {
        // calculate the distance and height difference between the start and end points
        float distance = Vector3.Distance(start, end);
        Vector3 direction = end - start;
        float height = end.y - start.y;

        // calculate the initial velocity required to hit the target, using the equation v = sqrt(gravity * distance / 2 * sin(2 * angle))
        float velocity = Mathf.Sqrt(gravity * distance / Mathf.Sin(2 * Mathf.Deg2Rad * Mathf.Atan(height / distance)));

        // return the initial velocity as a vector, with the direction pointing towards the target
        return velocity * direction.normalized;
    }
}

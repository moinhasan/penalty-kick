using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Mathematics
{
    /// <summary>
    /// Function to
    /// 1. Get the peak point on the curve from the start and end points.
    /// 2. Length of the Triangle formed by start point, peak point and end point
    /// 3. Angle between [peak - start] and [end - start]
    /// </summary>
    /// <param name="curvePoints"></param>
    /// <param name="curvePeakPoint"></param>
    /// <param name="curveTriangleLength"></param>
    /// <param name="curveAngle"></param>
    public static void GetCurveDetails(List<Vector3> curvePoints, out Vector3 curvePeakPoint, out float curveTriangleLength, out float curveAngle)
    {

        // Distance between the start and end points and the peak point
        float peakDistance = 0.0f;

        // Initialize curve details
        curvePeakPoint = Vector3.zero;
        curveTriangleLength = 0.0f;
        curveAngle = 0.0f;

        // Need at least 3 points for the curve 
        if (curvePoints.Count < 1) return;

        // Get start and end points
        Vector3 startPoint = curvePoints[0];
        curvePeakPoint = curvePoints[0];
        Vector3 endPoint = curvePoints[curvePoints.Count - 1];

        // Calculate the distance between the start and end points
        float curveDistance = Vector3.Distance(startPoint, endPoint);

        // Iterate over each point on the curve
        foreach (Vector3 currentPoint in curvePoints)
        {
            // Calculate the distance between the start and end points and the current point
            float currentDistance = Vector3.Distance(startPoint, currentPoint) + Vector3.Distance(endPoint, currentPoint);

            // If the current distance is greater than the maximum distance
            if (currentDistance > peakDistance)
            {
                // Set the new maximum distance
                peakDistance = currentDistance;

                // Set the new furthest point
                curvePeakPoint = currentPoint;
            }
        }

        // Get the triangle length
        curveTriangleLength = curveDistance + peakDistance;

        // Get the Curve Angle
        curveAngle = Vector3.SignedAngle(curvePeakPoint - startPoint, endPoint - startPoint, Vector3.forward);
    }

    //Calculate the intersection point of two lines.
    public static Vector3 FindIntersection(Vector3 p1, Vector3 v1, Vector3 p2, Vector3 v2)
    {
        Vector3 v3 = p2 - p1;
        float t1 = Vector3.Cross(v2, v3).magnitude / Vector3.Cross(v1, v2).magnitude;
        return p1 + t1 * v1;
    }

}

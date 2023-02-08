using UnityEngine;
using System.Collections.Generic;

public class CursorTrail : MonoBehaviour
{
    [SerializeField] LineRenderer trailPrefab = null;
    [SerializeField] Camera Cam; 
    [SerializeField] float distanceFromCamera = 1;
    
    private LineRenderer currentTrail;
    private List<Vector3> points = new List<Vector3>();
 
    private void Update()
    {
        if (ShotController.instance.IsTouchEnable)
        {
            if (Input.GetMouseButtonDown(0))
            {
                CreateCurrentTrail();
                AddPoint();
            }

            if (Input.GetMouseButton(0))
            {
                AddPoint();
            }

            if (Input.GetMouseButtonUp(0))
            {
                DestroyCurrentTrail();
            }
        }
        else
        {
            DestroyCurrentTrail();
        }

        UpdateTrailPoints();    
    }
 
    private void DestroyCurrentTrail()
    {
        if (currentTrail != null)
        {
            Destroy(currentTrail.gameObject);
            currentTrail = null;
            points.Clear();
        }
    }

    private void CreateCurrentTrail()
    {
        currentTrail = Instantiate(trailPrefab);
        currentTrail.transform.SetParent(transform, true);
    }

    private void AddPoint()
    {
        if (currentTrail != null)
        {
            Vector3 mousePosition = Input.mousePosition;
            points.Add(Cam.ViewportToWorldPoint(new Vector3(mousePosition.x / Screen.width, mousePosition.y / Screen.height, distanceFromCamera)));
        }
    }
 
    private void UpdateTrailPoints()
    {
        if (currentTrail != null && points.Count > 1)
        {
            currentTrail.positionCount = points.Count;
            currentTrail.SetPositions(points.ToArray());
        }
        else
        {
            DestroyCurrentTrail();
        }
    }

    void OnDisable()
    {
        DestroyCurrentTrail();
    }
 
}

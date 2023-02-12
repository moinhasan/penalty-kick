using UnityEngine;
using System.Collections.Generic;

public class CursorTrail : MonoBehaviour
{
    [SerializeField] private LineRenderer _trailPrefab = null;
    [SerializeField] private Camera _trailCamera; 
    [SerializeField] private float _distanceFromCamera = 35;
    
    private LineRenderer _currentTrail;
    private List<Vector3> _points = new List<Vector3>();
 
    private void Update()
    {
        if (ShotController.Instance.IsTouchEnable)
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
        if (_currentTrail != null)
        {
            Destroy(_currentTrail.gameObject);
            _currentTrail = null;
            _points.Clear();
        }
    }

    private void CreateCurrentTrail()
    {
        _currentTrail = Instantiate(_trailPrefab);
        _currentTrail.transform.SetParent(transform, true);
    }

    private void AddPoint()
    {
        if (_currentTrail != null)
        {
            Vector3 mousePosition = Input.mousePosition;
            _points.Add(_trailCamera.ViewportToWorldPoint(new Vector3(mousePosition.x / Screen.width, mousePosition.y / Screen.height, _distanceFromCamera)));
        }
    }
 
    private void UpdateTrailPoints()
    {
        if (_currentTrail != null && _points.Count > 1)
        {
            _currentTrail.positionCount = _points.Count;
            _currentTrail.SetPositions(_points.ToArray());
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

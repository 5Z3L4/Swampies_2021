using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZoomAreaDollyTrack : MonoBehaviour
{
    private CinemachineSmoothPath _dollyTrack;
    private List<WaypointsToZoom> _zoomWaypoints = new();
    
    private CinemachineTrackedDolly _cameraBody;
    private CinemachineVirtualCamera _vcam;
    private float _defaultZoom;
    private float _zoomSpeed;
    private int _previousWaypoint, _nextWaypoint;
    private bool _shouldZoom = true;
    private float _targetSize;
    private bool _canZoom = true;

    private void Awake()
    {
        _vcam = GetComponent<CinemachineVirtualCamera>();
        _cameraBody = _vcam.GetCinemachineComponent<CinemachineTrackedDolly>();
        _dollyTrack = FindObjectOfType<CinemachineSmoothPath>();
        _zoomWaypoints = CameraSettings.Instance.zoomWaypoints;
        _defaultZoom = CameraSettings.Instance.CameraSize;
        _zoomSpeed = CameraSettings.Instance.ZoomSpeed;
    }
    private void Start()
    {
        if (_zoomWaypoints.Count == 0) //disables script if there's no waypoints to zooming
        {
            enabled = false;
        }
        if (_zoomWaypoints.Count > _dollyTrack.m_Waypoints.Length) //if there's more waypoints in list than waypoints for real(dolly track) it removes excess
        {
            _zoomWaypoints.RemoveRange((int)_dollyTrack.m_Waypoints.Length, (int)(_zoomWaypoints.Count - _dollyTrack.m_Waypoints.Length));
        }
        foreach (WaypointsToZoom waypoint in _zoomWaypoints)
        {
            if (waypoint.zoomValue < 0) //if entered the negative value it changes it to positive
            {
                waypoint.zoomValue *= -1;
            }
        }
        _zoomWaypoints = _zoomWaypoints.OrderBy(x => x.waypointNumber).ToList(); //sorts list ascending (0 -> 1 -> 2...)
        _previousWaypoint = _zoomWaypoints[0].waypointNumber;
        _nextWaypoint = _zoomWaypoints[1].waypointNumber;
    }
    private void Update()
    {
        var currentPosition = _cameraBody.m_PathPosition;
        CheckPosition(currentPosition);
    }
    private void Zoom(int waypointNumber)
    {
        if (!_canZoom) return;   
        if (_zoomWaypoints[waypointNumber].zoomType == WaypointsToZoom.ZoomType.ZoomIn)
        {
            _targetSize = _defaultZoom + _zoomWaypoints[waypointNumber].zoomValue;
        }
        else
        {
            _targetSize = _defaultZoom - _zoomWaypoints[waypointNumber].zoomValue;
        }
        if (_vcam.m_Lens.OrthographicSize == _targetSize)
        {
            _canZoom = false;
            return;
        }
        _vcam.m_Lens.OrthographicSize = Mathf.MoveTowards(_vcam.m_Lens.OrthographicSize, _targetSize, _zoomSpeed * Time.deltaTime);
    }
    private void ZoomReset()
    {
        if (_vcam.m_Lens.OrthographicSize != _defaultZoom)
        {
            _vcam.m_Lens.OrthographicSize = Mathf.MoveTowards(_vcam.m_Lens.OrthographicSize, _defaultZoom, _zoomSpeed * Time.deltaTime);
        }
        else
        {
            _canZoom = true;
        }
    }
    private void CheckPosition(float currentPos)
    {
        if ((currentPos < _zoomWaypoints[0].waypointNumber || currentPos > _zoomWaypoints[^1].waypointNumber)) //checks if position of camera is before/after the first/last waypoint
        {
            if (_vcam.m_Lens.OrthographicSize == _defaultZoom) //checks if zoom is set to default
            {
                return;
            }
            else
            {
                ZoomReset();
            }
        }
        else
        {
            if (currentPos >= _previousWaypoint)
            {
                ChangeWaypoints(true);
            }
            else
            {
                ChangeWaypoints(false);
            }
            Zoom(_previousWaypoint);
        }
        
    }
    private void ChangeWaypoints(bool doIncrement)
    {
        if (doIncrement)
        {
            _previousWaypoint += 1;
            _nextWaypoint += 1;
        }
        else
        {
            _previousWaypoint -= 1;
            _nextWaypoint -= 1;
        }
        _canZoom = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private float _timeToMove;
    [SerializeField] private Transform firstPosition;
    [SerializeField] private Transform secondPosition;

    private Camera _camera;
    private Transform _nextPosition;

    private Transform CameraTransform => _camera.transform;

    private void Awake()
    {
        _camera = Camera.main;
        CameraTransform.position = firstPosition.position;
        CameraTransform.rotation = firstPosition.rotation;
        _nextPosition = secondPosition;
    }

    public IEnumerator OnCameraMove()
    {
        while(Vector3.Distance(CameraTransform.position,_nextPosition.position) >= 0.01f)
        {
            CameraTransform.position = Vector3.Lerp(CameraTransform.position, _nextPosition.position, _timeToMove * Time.deltaTime);
            CameraTransform.rotation = Quaternion.Lerp(CameraTransform.rotation,_nextPosition.rotation, _timeToMove * Time.deltaTime);

            yield return null;
        }

        if(_nextPosition == firstPosition)
        {
            _nextPosition = secondPosition;
        }
        else
        {
            _nextPosition = firstPosition;
        }
    }
}

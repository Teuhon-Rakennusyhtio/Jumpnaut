using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    [SerializeField] float _speed = 0.5f;
    [SerializeField] float _yOffset = -20f;
    [SerializeField] float _topClamp = float.PositiveInfinity;
    [SerializeField] float _bottomClamp = float.NegativeInfinity;
    Transform _transform, _camera;

    void Start()
    {
        if (_topClamp < _bottomClamp)
        {
            _topClamp = float.PositiveInfinity;
            _bottomClamp = float.NegativeInfinity;
            Debug.LogError($"The parallax component of {gameObject.name} has a greater bottom clamp value than its top clamp value! Disabled its clamps.");
        }
        _transform = GetComponent<Transform>();
        _camera = Camera.main.GetComponent<Transform>();
    }
    void LateUpdate()
    {
        float cameraYPos = _camera.position.y * _speed + _yOffset;
        cameraYPos = Mathf.Max(cameraYPos, _bottomClamp);
        cameraYPos = Mathf.Min(cameraYPos, _topClamp);
        Vector3 position = new Vector3(_transform.position.x, cameraYPos, _transform.position.z);
        _transform.position = position;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    [SerializeField] float _speed = 0.5f;
    [SerializeField] float _yOffset = -20f;
    Transform _transform, _camera;

    void Start()
    {
        _transform = GetComponent<Transform>();
        _camera = Camera.main.GetComponent<Transform>();
    }
    void LateUpdate()
    {
        _transform.position = new Vector3(_transform.position.x, _camera.position.y * _speed + _yOffset, _transform.position.z);
    }
}

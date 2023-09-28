using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    public Transform _spawnPoint;
    bool _fellBehind;

    void OnTriggerEnter2D(Collider2D collision)
    {
        _fellBehind = true;
        if (collision.CompareTag("MainCamera") && _fellBehind == true)
        {
            transform.position = _spawnPoint.TransformPoint(new Vector3(0,0,0));
            _fellBehind = false;
        }
    }
}

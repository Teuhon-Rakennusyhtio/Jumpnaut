using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    // THIS SCRIPT HAS BEEN APPROPRIATED BY PLAYERMOVER
    [SerializeField] private float _spawnSpeed;
    [SerializeField] private GameObject _spawnPoint;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MainCamera"))
        {
            float distance = Vector3.Distance(transform.position, _spawnPoint.transform.position);
            transform.position = Vector3.MoveTowards(transform.position, _spawnPoint.transform.position, _spawnSpeed * Time.deltaTime);
        }
    }
}

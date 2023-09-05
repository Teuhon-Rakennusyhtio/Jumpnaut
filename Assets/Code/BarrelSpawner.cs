using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float minTime = 2f;
    [SerializeField] private float maxTime = 4f;

    private void Start()
    {
        Spawn();
    }

    private void Spawn()
    {
        Instantiate(prefab, transform.position, Quaternion.identity);
        Invoke(nameof(Spawn), Random.Range(minTime, maxTime));
    }
}

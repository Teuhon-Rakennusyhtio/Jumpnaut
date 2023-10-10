using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatterySpawner : MonoBehaviour
{
    [SerializeField] GameObject _battery;
    [SerializeField] bool _existsInSinglePlayer;
    void Start()
    {
        if (!_existsInSinglePlayer && GameManager.PlayerDevices.Count == 1) return;
        SpawnBattery();
    }

    public void SpawnBattery()
    {
        GameObject newBattery = Instantiate(_battery, transform.position, Quaternion.identity);
        newBattery.GetComponent<Battery>().BatterySpawner = this;
    }
}

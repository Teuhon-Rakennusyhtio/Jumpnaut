using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] float _speed = 10f;
    ChildDeviceManager _device;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(_device.GetMoveInput * _speed * Time.deltaTime);
    }

    public void SetPlayerDevice(ChildDeviceManager device)
    {
        _device = device;
    }
}

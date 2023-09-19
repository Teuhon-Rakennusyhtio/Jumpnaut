using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : GenericMover
{
    public ChildDeviceManager Device;
    public int Id;
    Vector2 _cameraPosition;
    bool _pauseInput;

    void Start()
    {
        _cameraPosition = Vector2.zero;
        Camera.main.GetComponent<CameraMovement>().AddPlayer(this);
    }

    protected override void FixedUpdateLogic()
    {
        OpenPauseMenu();
        RaycastHit2D cameraPosition = Physics2D.Raycast(transform.position, Vector2.down, 3f, _groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * 3f, Color.green);
        if (cameraPosition) _cameraPosition = cameraPosition.point;
        else _cameraPosition = transform.position;
    }

    protected override void GetInputs()
    {
        if (Device.GetMoveAnalogInput.magnitude > 0.2f) _moveInput = Device.GetMoveAnalogInput;
        else _moveInput = Device.GetMoveInput;
        _jumpInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.jump];
        _useInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.use];
        _catchInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pickup];
        _pauseInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pause];
    }

    void OpenPauseMenu()
    {
        if (_pauseInput) PauseMenu.Open(this);
    }

    public Vector2 GetPlayerCameraPosition() => _cameraPosition;
}

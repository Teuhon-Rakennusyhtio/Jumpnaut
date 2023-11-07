using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMover : GenericMover
{
    protected override void GetInputs()
    {
        //_jumpInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.jump];
        //_useInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.use];
        //_catchInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pickup];
        //_pauseInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pause];

        _catchInput = true;
        _moveInput = Vector2.right * Random.Range(-1f, 1f);
    }
}

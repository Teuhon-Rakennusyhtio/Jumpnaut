using System.Collections;
using System.Collections.Generic;
using static System.Enum;
using UnityEngine;
using UnityEngine.InputSystem;


public class ChildDeviceManager : MonoBehaviour
{
    int _id = -1;

    Vector2 _moveInput, _moveAnalogInput;
    bool _newInput = false;
    public enum InputTypes { confirm, cancel, pause, jump, use, pickup };
    bool[] _inputs = new bool[GetNames(typeof(InputTypes)).Length], _oldInputs = new bool[GetNames(typeof(InputTypes)).Length], _passedInputs = new bool[GetNames(typeof(InputTypes)).Length];
    
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        MainDeviceManager.AddNewDevice(this);
    }

    public void OnMove(InputAction.CallbackContext context) => _moveInput = context.ReadValue<Vector2>();
    public void OnMoveAnalog(InputAction.CallbackContext context) => _moveAnalogInput = context.ReadValue<Vector2>();
    public void OnConfirm(InputAction.CallbackContext context) => _inputs[(int) InputTypes.confirm] = context.action.triggered;
    public void OnCancel(InputAction.CallbackContext context) => _inputs[(int) InputTypes.cancel] = context.action.triggered;
    public void OnOpenPauseMenu(InputAction.CallbackContext context) => _inputs[(int) InputTypes.pause] = context.action.triggered;
    public void OnJump(InputAction.CallbackContext context) => _inputs[(int) InputTypes.jump] = context.action.triggered;
    public void OnUseItem(InputAction.CallbackContext context) => _inputs[(int) InputTypes.use] = context.action.triggered;
    public void OnCatchOrPickup(InputAction.CallbackContext context) => _inputs[(int) InputTypes.pickup] = context.action.triggered;

    void LateUpdate()
    {
        _newInput = false;
        for(int i = 0; i < _inputs.Length; i++)
        {
            if (_inputs[i] != _oldInputs[i]) _newInput = true;
            _passedInputs[i] = _inputs[i] != _oldInputs[i] && _inputs[i];
            _oldInputs[i] = _inputs[i];
        }
        if (_moveInput.magnitude > 0.3f || _newInput)
        {
            MainDeviceManager.PassInputs(_id, _passedInputs, _moveInput);
        }
        else if (_moveAnalogInput.magnitude > 0.3f)
        {
            MainDeviceManager.PassInputs(_id, _passedInputs, _moveAnalogInput);
        }
    }

    public bool[] GetInputs => _inputs;
    public Vector2 GetMoveInput => _moveInput;
    public Vector2 GetMoveAnalogInput => _moveAnalogInput;

    public int Id
    {
        get
        {
            return _id;
        }
        set
        {
            if (_id == -1)
            {
                _id = value;
                gameObject.name = $"ChildDeviceManager {_id}";
            }
            else
            {
                Debug.LogError($"Overwriting the _id of ChildDeviceManager with _id {_id} is not allowed!");
            }
        }
    }
}

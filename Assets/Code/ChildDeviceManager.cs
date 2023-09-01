using System.Collections;
using System.Collections.Generic;
using static System.Enum;
using UnityEngine;
using UnityEngine.InputSystem;


public class ChildDeviceManager : MonoBehaviour
{
    int id = -1;

    Vector2 moveInput;
    bool confirmInput, cancelInput, pauseInput, jumpInput, useInput, catchInput;
    bool newInput = false;
    public enum InputTypes { confirm, cancel, pause, jump, use, pickup };
    bool[] inputs = new bool[GetNames(typeof(InputTypes)).Length];
    
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        MainDeviceManager.AddNewDevice(this);
    }

    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
    public void OnConfirm(InputAction.CallbackContext context) => inputs[(int) InputTypes.confirm] = context.action.triggered;
    public void OnCancel(InputAction.CallbackContext context) => inputs[(int) InputTypes.cancel] = context.action.triggered;
    public void OnOpenPauseMenu(InputAction.CallbackContext context) => inputs[(int) InputTypes.pause] = context.action.triggered;
    public void OnJump(InputAction.CallbackContext context) => inputs[(int) InputTypes.jump] = context.action.triggered;
    public void OnUseItem(InputAction.CallbackContext context) => inputs[(int) InputTypes.use] = context.action.triggered;
    public void OnCatchOrPickup(InputAction.CallbackContext context) => inputs[(int) InputTypes.pickup] = context.action.triggered;

    void LateUpdate()
    {
        newInput = false;
        foreach(bool input in inputs) if (input) newInput = true;
        if (moveInput != Vector2.zero || newInput)
        {
            MainDeviceManager.PassInputs(id, inputs, moveInput);
        }
    }

    public bool[] GetInputs => inputs;
    public Vector2 GetMoveInput => moveInput;

    public int Id
    {
        get
        {
            return id;
        }
        set
        {
            if (id == -1)
            {
                id = value;
                gameObject.name = $"ChildDeviceManager {id}";
            }
            else
            {
                Debug.LogError($"Overwriting the id of ChildDeviceManager with id {id} is not allowed!");
            }
        }
    }
}

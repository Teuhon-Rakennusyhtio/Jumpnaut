using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainDeviceManager : MonoBehaviour
{
    public static MainDeviceManager Instance;

    List<ChildDeviceManager> _inputDevices;
    int _latestInputId;
    bool[] _lastInputs;
    Vector2 _lastMoveInput;
    bool _alreadyInputOnThisFrame = false;
    float _uiDelay = 0f;
    [SerializeField] EventSystem _eventSystem;

    void Start()
    {
        Instance = this;
        _inputDevices = new List<ChildDeviceManager>();
    }

    public static int GetLatestInputId() => Instance._latestInputId;
    public static bool[] GetLatestInputs() => Instance._lastInputs;
    public static ChildDeviceManager GetChildDeviceManager(int id) => Instance._inputDevices[id];

    public static void AddNewDevice(ChildDeviceManager newDevice)
    {
        newDevice.Id = Instance._inputDevices.Count;
        Instance._inputDevices.Add(newDevice);
        Debug.Log($"A new ChildDeviceManager with id {newDevice.Id} has been added to the device list of the MainDeviceManager.");
    }

    public static void PassInputs(int id, bool[] inputs, Vector2 moveInput)
    {
        if (Instance._alreadyInputOnThisFrame) return;
        Instance._alreadyInputOnThisFrame = true;
        Instance._latestInputId = id;
        Instance._lastInputs = inputs;
        Instance._lastMoveInput = moveInput;

        // UI Stuff
        if (!GameManager.CurrentlyInUI || Instance._uiDelay > 0f || (GameManager.UIOwnerId != -1 && GameManager.UIOwnerId != id)) return;
        
        Instance._uiDelay = 0.2f;
        
        GameObject selectedGameObject = Instance._eventSystem.currentSelectedGameObject;
        Selectable nextSelect = null;
        if (selectedGameObject.GetComponent<Button>() != null)
        {
            Button selectedButton = selectedGameObject.GetComponent<Button>();

            
            if (moveInput.y < -0.2f) nextSelect = selectedButton.FindSelectableOnDown();
            else if (moveInput.x < -0.2f) nextSelect = selectedButton.FindSelectableOnLeft();
            else if (moveInput.x > 0.2f) nextSelect = selectedButton.FindSelectableOnRight();
            else if (moveInput.y > 0.2f) nextSelect = selectedButton.FindSelectableOnUp();
            else if (inputs[(int) ChildDeviceManager.InputTypes.confirm])
            {
                ExecuteEvents.Execute(Instance._eventSystem.currentSelectedGameObject, new BaseEventData(Instance._eventSystem), ExecuteEvents.submitHandler);
            }
            if (nextSelect != null) nextSelect.Select();
        }
    }

    void Update()
    {
        if (!_alreadyInputOnThisFrame)
        {
            _uiDelay = 0f;
        }
        else if (_uiDelay > 0f)
        {
            _uiDelay -= Time.deltaTime;
        }
        _alreadyInputOnThisFrame = false;
    }
}

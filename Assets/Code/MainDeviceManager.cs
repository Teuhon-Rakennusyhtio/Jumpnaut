using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainDeviceManager : MonoBehaviour
{
    public static MainDeviceManager Instance;
    List<ChildDeviceManager> inputDevices;
    int latestInputId;
    bool[] lastInputs;
    Vector2 lastMoveInput;
    bool alreadyInputOnThisFrame = false;

    void Start()
    {
        Instance = this;
        inputDevices = new List<ChildDeviceManager>();
    }

    public static int GetLatestInputId() => Instance.latestInputId;
    public static bool[] GetLatestInputs() => Instance.lastInputs;
    public static ChildDeviceManager GetChildDeviceManager(int id) => Instance.inputDevices[id];

    public static void AddNewDevice(ChildDeviceManager newDevice)
    {
        newDevice.Id = Instance.inputDevices.Count;
        Instance.inputDevices.Add(newDevice);
        Debug.Log($"A new ChildDeviceManager with id {newDevice.Id} has been added to the device list of the MainDeviceManager.");
    }

    public static void PassInputs(int id, bool[] inputs, Vector2 moveInput)
    {
        if (Instance.alreadyInputOnThisFrame) return;
        Instance.alreadyInputOnThisFrame = true;
        Instance.latestInputId = id;
        Instance.lastInputs = inputs;
        Instance.lastMoveInput = moveInput;
        //Debug.Log(Instance.lastInputs[(int) ChildDeviceManager.InputTypes.confirm]);
    }

    void Update()
    {
        alreadyInputOnThisFrame = false;
    }
}

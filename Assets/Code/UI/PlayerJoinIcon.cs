using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJoinIcon : MonoBehaviour
{
    ChildDeviceManager _device;
    JoinGameSubMenu _joinGame;
    bool[] _inputs;
    bool _confirmReleased = false, _cancelReleased = false, _ready = false;

    [SerializeField] GameObject _image, _confirmText, _cancelText;
    
    void Awake()
    {
        _joinGame = gameObject.GetComponentInParent<JoinGameSubMenu>();
        
        //_device = MainDeviceManager.GetChildDeviceManager(MainDeviceManager.GetLatestInputId());
    }

    void Update()
    {
        if (_device == null) return;
        _inputs = _device.GetInputs;
        if (_inputs[(int) ChildDeviceManager.InputTypes.confirm])
        {
            if (_confirmReleased && !_ready)
            {
                Readiness(true);
                _confirmReleased = false;
            }
        }
        else
        {
            _confirmReleased = true;
        }
        if (_inputs[(int) ChildDeviceManager.InputTypes.cancel])
        {
            if (_cancelReleased)
            {
                if (_ready)
                {
                    Readiness(false);
                }
                else
                {
                    Leave();
                }
                _cancelReleased = false;
            }
        }
        else
        {
            _cancelReleased = true;
        }
    }

    void Readiness(bool toggle)
    {
        _confirmText.SetActive(!toggle);
        _cancelText.SetActive(toggle);
        _ready = toggle;
        _joinGame.PlayerReady(toggle);
    }

    public void AssignPlayer(ChildDeviceManager device)
    {
        _device = device;
        _image.SetActive(true);
        _confirmText.SetActive(true);
    }

    void Leave()
    {
        _joinGame.PlayerLeft(_device.Id, this);
        Destroy(gameObject);
    }
}

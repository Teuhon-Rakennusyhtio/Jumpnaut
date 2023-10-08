using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : GenericMover
{
    [SerializeField] Animator _animator;
    public ChildDeviceManager Device;
    public int Id;
    Vector2 _cameraPosition;
    bool _pauseInput;
    public float _respawnSpeed = 7f;
    GameObject _spawnPoint;

    void Start()
    {
        _spawnPoint = GameObject.Find("Spawnpoint");
        _cameraPosition = Vector2.zero;
        Camera.main.GetComponent<CameraMovement>().AddPlayer(this);
    }

    protected override void FixedUpdateLogic()
    {
        OpenPauseMenu();

        RaycastHit2D cameraPosition = Physics2D.Raycast(transform.position, Vector2.down, 5f, _groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * 5f, Color.green);
        if (cameraPosition) _cameraPosition = cameraPosition.point;
        else if (transform.position.y < _cameraPosition.y || _climbingLadder) _cameraPosition = transform.position;
        if (_animator != null) Animate(_animator);
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

    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MainCamera"))
        {
            SetControl(false);
        }

        if (collision.gameObject == _spawnPoint && IsInControl == false)
        {
            SetControl(true);
        }  
    }

    void Respawning()
    {

    if (IsInControl == false)
        {
            float distance = Vector3.Distance(transform.position, _spawnPoint.transform.position);
            transform.position = Vector3.MoveTowards(transform.position, _spawnPoint.transform.position, _respawnSpeed * Time.deltaTime);
        }
    }

    void Update()
    {
        Respawning();
    }
}

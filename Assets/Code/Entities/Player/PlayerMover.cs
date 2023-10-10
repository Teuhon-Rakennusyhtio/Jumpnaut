using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : GenericMover
{
    [SerializeField] Animator _animator;
    [SerializeField] Transform _mainRig, _leftArm, _rightArm;
    public ChildDeviceManager Device;
    public int Id;
    Vector2 _cameraPosition;
    bool _pauseInput;//, _throwAnimationStarted;
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

    protected override void CheckFacingLogic()
    {
        bool oldFacing = _facingLeft;
        // Figure out which direction the player is facing
        _facingLeft = _mainRig.localScale.x < 0f;

        if (oldFacing != _facingLeft) FlipLogic();
    }

    protected override void FlipLogic()
    {
        if (!_heldItemIsFlipalbe)
        {
            _handTransform.localScale = new Vector3((_facingLeft ? -1f : 1f), 1f, 1f);
        }
        //_handTransform.localScale = new Vector3((_facingLeft && _heldItemIsFlipalbe ? -1f : 1f), 1f, 1f);
        /*Transform heldItem = _handTransform.GetChild(0);
        if (heldItem != null)
        {
            heldItem.localScale = new Vector3(1f, 1f, 1f);
        }*/
        if (!_holdingSomething && !_catchInput)
        {
            _animator.Play("Empty Hand");
        }
        else if (!_heldItemIsFlipalbe)
        {
            Transform heldItem = _handTransform.GetChild(0).transform;
            heldItem.localPosition = new Vector2(-heldItem.localPosition.x, heldItem.localPosition.y);
        }
        /*if (_holdingHeavyObject)
        {
            _animator.Play("Hold Two Handed");
        }*/
        if (_holdingOut)
        {
            if (_facingLeft)
            {
                _animator.Play("Hold Out Left", -1, 1f);
            }
            else
            {
                _animator.Play("Hold Out Right", -1, 1f);
            }
        }
        
        if (_holdingHeavyObject || !_facingLeft)
        {
            _handTransform.parent = _rightArm;
            
        }
        else
        {
            _handTransform.parent = _leftArm;
        }
        _handTransform.localPosition = Vector3.right * 0.45f;
        _handTransform.localRotation = Quaternion.identity;
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

    protected override void CatchLogic(Holdable holdable)
    {
        base.CatchLogic(holdable);
        if (_holdingHeavyObject)
        {
            //FlipLogic();
            _animator.Play("Pickup Two Handed");
        }
        else if (!_facingLeft)
        {
            _animator.Play("Pickup Right");
        }
        else
        {
            _animator.Play("Pickup Left");
        }
    }

    protected override void ThrowLogic()
    {
        //_throwAnimationStarted = true;
        //_animator.SetTrigger("Throw");
        if (!_facingLeft)
        {
            _animator.Play("Throw Right");
        }
        else
        {
            _animator.Play("Throw Left");
        }
        Throw();
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

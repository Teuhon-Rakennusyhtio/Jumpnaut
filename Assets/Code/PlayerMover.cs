using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] float _speed = 10f, _coyoteTime = 0.2f, _jumpForce = 10f, _jumpApex = 0.8f, _jumpFallSpeed = 2f, _fallAcceleration = 1f;
    [SerializeField] Vector2 _maxGravity;
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] PhysicsMaterial2D _standMaterial, _moveMaterial;
    public ChildDeviceManager Device;
    Vector2 _movement, _slopeNormalPerpendicular, _gravity, _moveInput, _cameraPosition;
    Rigidbody2D _rigidBody;
    Collider2D _collider;
    float _currentCoyoteTime = 0f, _jumpVelocity = 0f, _groundCastHeight;
    int _groundedFrames = 0;
    bool _grounded = true, _onSlope, _alreadyJumped = true;

    bool _jumpInput, _useInput, _catchInput, _pauseInput;
    public int Id;
    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _groundCastHeight = _collider.bounds.extents.y + 0.02f;
        _gravity = Vector2.zero;
        _cameraPosition = Vector2.zero;
        Camera.main.GetComponent<CameraMovement>().AddPlayer(this);
    }

    void Move()
    {
        if (_grounded)
        {
            Debug.DrawRay(transform.position, _slopeNormalPerpendicular, Color.red);
            _movement += new Vector2(-_moveInput.x * _speed * _slopeNormalPerpendicular.x,
                                     -_moveInput.x * _speed * _slopeNormalPerpendicular.y);
        }
        else
        {
            _movement += Vector2.right * _moveInput.x * _speed;
        }
    }

    void Jump()
    {
        _currentCoyoteTime -= Time.fixedDeltaTime;
        _movement += Vector2.up * _jumpVelocity;

        if (_jumpVelocity < 0f || _grounded)
        {
            _jumpVelocity = 0f;
        }
        else if (_jumpVelocity > 0f)
        {
            if (_jumpVelocity + _gravity.y > -1f)
            {
                _jumpVelocity -= _jumpForce * _jumpApex * Time.fixedDeltaTime;
            }
            else
            {
                _jumpVelocity -= _jumpForce * _jumpFallSpeed * Time.fixedDeltaTime;
            }
        }
        if (_jumpInput && !_alreadyJumped)
        {
            _alreadyJumped = true;
            _currentCoyoteTime = _coyoteTime; 
        }
        else if (!_jumpInput)
        {
            _alreadyJumped = false;
        }
        if (!_grounded) return;

        if (_currentCoyoteTime > 0f)
        {
            _grounded = false;
            _groundedFrames = 0;
            _gravity = Vector2.zero;
            _jumpVelocity = _jumpForce;
        }
    }

    void OpenPauseMenu()
    {
        if (_pauseInput) PauseMenu.Open(this);
    }

    void CheckIfGrounded()
    {
        _grounded = (Physics2D.Raycast(transform.position, Vector2.down, _groundCastHeight, _groundLayer)
                  || Physics2D.Raycast((Vector2)transform.position + (Vector2.right * _collider.bounds.extents.x), Vector2.down, _groundCastHeight, _groundLayer)
                  || Physics2D.Raycast((Vector2)transform.position + (Vector2.left * _collider.bounds.extents.x), Vector2.down, _groundCastHeight, _groundLayer));
        if (_groundedFrames < 5 && _grounded) _groundedFrames++;
        else if (!_grounded) _groundedFrames = 0;
        Debug.DrawRay(transform.position, Vector2.down * _groundCastHeight);
        Debug.DrawRay((Vector2)transform.position + (Vector2.right * _collider.bounds.extents.x), Vector2.down * _groundCastHeight);
        Debug.DrawRay((Vector2)transform.position + (Vector2.left * _collider.bounds.extents.x), Vector2.down *_groundCastHeight);
    }

    void CheckSlope()
    {
        float direction = new Vector2(_moveInput.x, 0f).normalized.x;
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position + new Vector2(_collider.bounds.extents.x * direction, 0f), Vector2.down, 1f, _groundLayer);
        RaycastHit2D hitBehind = Physics2D.Raycast((Vector2)transform.position + new Vector2(_collider.bounds.extents.x * -direction, 0f), Vector2.down, 1f, _groundLayer);
        if (!hit && !hitBehind) return;
        if (!hit)
        {
            hit = hitBehind;
        }
        else if (hitBehind && hitBehind.distance < hit.distance)
        {
            hit = hitBehind;
        }

        _slopeNormalPerpendicular = Vector2.Perpendicular(hit.normal).normalized;
    }

    void CalculateGravity()
    {
        if (_groundedFrames >= 5)
        {
            _gravity = Vector2.zero;
            return;
        }
        _gravity = Vector2.MoveTowards(_gravity, _maxGravity, _fallAcceleration * 50f * Time.fixedDeltaTime);
        //_gravity = _maxGravity;
    }

    void FixedUpdate()
    {
        if (GameManager.CurrentlyInUI) return;
        if (Device.GetMoveAnalogInput.magnitude > 0.2f) _moveInput = Device.GetMoveAnalogInput;
        else _moveInput = Device.GetMoveInput;
        _movement = Vector2.zero;

        if (_moveInput.x != 0f)
        {
            _collider.sharedMaterial = _moveMaterial;
        }
        else
        {
            _collider.sharedMaterial = _standMaterial;
        }

        GetInputs();
        CheckIfGrounded();
        CheckSlope();
        CalculateGravity();
        Move();
        Jump();
        OpenPauseMenu();
        _rigidBody.velocity = (_movement + _gravity) * 50f * Time.fixedDeltaTime;
        RaycastHit2D cameraPosition = Physics2D.Raycast(transform.position, Vector2.down, 3f, _groundLayer);
        if (cameraPosition) _cameraPosition = cameraPosition.point;
        else _cameraPosition = transform.position;
    }

    public Vector2 GetPlayerCameraPosition() => _cameraPosition;

    void GetInputs()
    {
        _jumpInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.jump];
        _useInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.use];
        _catchInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pickup];
        _pauseInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pause];
    }
}

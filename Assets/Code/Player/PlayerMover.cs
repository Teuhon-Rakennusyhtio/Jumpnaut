using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : MonoBehaviour, ILadderInteractable
{
    [SerializeField] float _speed = 10f, _climbingSpeed = 5f, _maxJumpBuffer = 0.2f, _maxCoyoteTime = 0.1f, _jumpForce = 10f, _jumpApex = 0.8f, _jumpFallSpeed = 2f, _fallAcceleration = 1f;
    [SerializeField] Vector2 _maxGravity;
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] PhysicsMaterial2D _standMaterial, _moveMaterial;
    public ChildDeviceManager Device;
    Vector2 _movement, _slopeNormalPerpendicular, _gravity, _moveInput, _cameraPosition;
    Rigidbody2D _rigidBody;
    Collider2D _collider;
    float _jumpBuffer = 0f, _coyoteTime = 0f, _jumpVelocity = 0f, _groundCastHeight, _ladderXCoord, _ladderBottom, _ladderTop;
    int _groundedFrames = 0;
    bool _grounded = true, _alreadyJumped = true, _climbingLadder = false, _nextToLadder = false, _insideGround = false;

    bool _jumpInput, _useInput, _catchInput, _pauseInput;
    public int Id;
    
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _groundCastHeight = _collider.bounds.extents.y + 0.05f;
        _gravity = Vector2.zero;
        _cameraPosition = Vector2.zero;
        Camera.main.GetComponent<CameraMovement>().AddPlayer(this);
    }

    public void OnLadderEnter(float xCoord)
    {
        _ladderXCoord = xCoord;
        _nextToLadder = true;
    }

    public void OnLadderExit()
    {
        _nextToLadder = false;
    }

    void Move()
    {
        if (_climbingLadder) return;
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

    void ClimbLadder()
    {
        if (!_nextToLadder)
        {
            _climbingLadder = false;
            return;
        }

        if (Mathf.Abs(_moveInput.x) > 0.3f)
        {
            _climbingLadder = false;
            return;
        }

        if (Mathf.Abs(_moveInput.y) > 0.2f)
        {
            if (!_climbingLadder)
            {
                Vector2? ladderBottom = Ladder.GetLadderBottom(_collider, _ladderXCoord);
                Vector2? ladderTop = Ladder.GetLadderTop(_collider, _ladderXCoord);
                if (ladderBottom == null || ladderTop == null) return;

                Vector2 ladderBottom2;
                Vector2 ladderTop2;
                ladderBottom2 = ((Vector2)ladderBottom + Vector2.zero); // This is stupid and I hate it. Why can't I just get the 'y' of a nullable Vector2?????
                ladderTop2 = ((Vector2)ladderTop + Vector2.zero);
                _ladderBottom = ladderBottom2.y;
                _ladderTop = ladderTop2.y;

                _climbingLadder = true;
                _collider.isTrigger = true;
                transform.position = new Vector2(_ladderXCoord, Mathf.Max(transform.position.y, _ladderBottom));
            }
            if ((transform.position.y > _ladderBottom && _moveInput.y < 0.1f) || (transform.position.y < _ladderTop && _moveInput.y > 0.1f))
            {
                _movement += Vector2.up * _moveInput.y * _climbingSpeed;
            }
        }
    }

    void Jump()
    {
        if (_climbingLadder)
        {
            _jumpVelocity = 0f;
            _alreadyJumped = true;
            return;
        }
        _jumpBuffer -= Time.fixedDeltaTime;
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
            _jumpBuffer = _maxJumpBuffer; 
        }
        else if (!_jumpInput)
        {
            _alreadyJumped = false;
        }
        if (!_grounded && _coyoteTime <= 0f) return;

        if (_jumpBuffer > 0f)
        {
            _coyoteTime = 0f;
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
        if (_groundedFrames < 5 && _grounded)
        {
            _groundedFrames++;
        }
        if (_grounded)
        {
            _coyoteTime = _maxCoyoteTime;
        }
        else
        {
            _groundedFrames = 0;
            _coyoteTime -= Time.fixedDeltaTime;
        }
        Debug.DrawRay(transform.position, Vector2.down * _groundCastHeight);
        Debug.DrawRay((Vector2)transform.position + (Vector2.right * _collider.bounds.extents.x), Vector2.down * _groundCastHeight);
        Debug.DrawRay((Vector2)transform.position + (Vector2.left * _collider.bounds.extents.x), Vector2.down *_groundCastHeight);

        if (_collider.isTrigger == true)
        {
            _grounded = false;
            _groundedFrames = 0;
        }
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

    void CheckIfInsideGround()
    {
        if (!_collider.isTrigger && !_climbingLadder) return;
        if (Physics2D.OverlapBox(transform.position, _collider.bounds.extents, 0, _groundLayer) == null)
        {
            _insideGround = false;
        }
        else
        {
            _insideGround = true;
        }
        if (!_climbingLadder && !_insideGround)
        {
            _collider.isTrigger = false;
        }
    }

    void CalculateGravity()
    {
        if (_groundedFrames >= 5 || _climbingLadder)
        {
            _gravity = Vector2.zero;
            return;
        }
        _gravity = Vector2.MoveTowards(_gravity, _maxGravity, _fallAcceleration * 50f * Time.fixedDeltaTime);
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
        ClimbLadder();
        Jump();
        OpenPauseMenu();
        CheckIfInsideGround();
        _rigidBody.velocity = (_movement + _gravity) * 50f * Time.fixedDeltaTime;
        RaycastHit2D cameraPosition = Physics2D.Raycast(transform.position, Vector2.down, 3f, _groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * 3f, Color.green);
        if (cameraPosition) _cameraPosition = cameraPosition.point;
        else _cameraPosition = transform.position;
    }

    void LateUpdate()
    {
        if (!_climbingLadder) return;
        if (transform.position.y < _ladderBottom)
        {
            transform.position = new Vector2(transform.position.x, _ladderBottom);
        }
        if (transform.position.y > _ladderTop)
        {
            transform.position = new Vector2(transform.position.x, _ladderTop);
        }
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

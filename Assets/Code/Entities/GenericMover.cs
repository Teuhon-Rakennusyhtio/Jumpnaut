using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Accessibility;

public abstract class GenericMover : MonoBehaviour, ILadderInteractable
{
    [SerializeField] float _speed = 7f, _climbingSpeed = 5f,
    _maxJumpBuffer = 0.2f, _maxCoyoteTime = 0.1f,
    _jumpForce = 13f, _jumpApex = 0.2f, _jumpFallSpeed = 3f,
    _fallAcceleration = 1f, _maxGravity = -15f,
    _groundAcceleration = 1f, _airAcceleration = 1f,
    _groundDecceleration = 1f, _airDecceleration = 1f;
    [SerializeField] protected LayerMask _groundLayer, _holdableLayer;
    [SerializeField] PhysicsMaterial2D _standMaterial, _moveMaterial;
    [SerializeField] protected Transform _handTransform;
    [SerializeField] protected GenericHealth _health;
    protected Holdable _heldItem;
    protected Vector2 _movement, _slopeNormalPerpendicular,
    _gravity, _moveInput, _previousPosition;
    Rigidbody2D _rigidBody;
    Collider2D _collider;
    float _jumpBuffer = 0f, _coyoteTime = 0f,
     _jumpVelocity = 0f, _groundCastHeight, _ladderXCoord,
     _ladderBottom, _ladderTop, _currentSpeed;
    protected int _groundedFrames = 0;
    bool _isInControl = true;
    protected bool _grounded = true, _alreadyJumped = true,
    _climbingLadder = false, _nextToLadder = false,
    _insideGround = false, _holdingSomething = false,
    _alreadyCaught = false, _facingLeft = false,
    _holdingHeavyObject = false, _holdingWeapon,
     _grabbedLadderThisFrame, _leftLadderThisFrame,
    _heldItemIsFlipalbe, _jumpedThisFrame, _holdingOut;

    protected bool _jumpInput, _useInput, _catchInput;
    
    public bool IsInControl { get { return _isInControl; } }
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _groundCastHeight = _collider.bounds.extents.y + 0.05f;
        _gravity = Vector2.zero;
        _previousPosition = transform.position;
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

        float acceleration = _grounded ? _groundAcceleration : _airAcceleration;
        float decceleration = _grounded ? _groundDecceleration : _airDecceleration;
        if (_moveInput.x == 0)
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, Time.fixedDeltaTime * decceleration);
        }
        else
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, _moveInput.x * _speed, Time.fixedDeltaTime * acceleration);
        }
        // If the entity is grounded and moving, walk along the slope
        if (_grounded)
        {
            Debug.DrawRay(transform.position, _slopeNormalPerpendicular, Color.red);
            _movement += new Vector2(
            -_currentSpeed * _slopeNormalPerpendicular.x,
            -_currentSpeed * _slopeNormalPerpendicular.y);
        }
        else
        {
            _movement += Vector2.right * _currentSpeed;
        }
    }

    protected virtual void FlipLogic()
    {
        if (_heldItemIsFlipalbe)
        {
            _handTransform.localScale = new Vector3((_facingLeft ? -1f : 1f), 1f, 1f);
        }
    }

    void ClimbLadder()
    {
        // ---------------------------------
        // If the entity has left the ladder
        // ---------------------------------
        if (!_nextToLadder || Mathf.Abs(_moveInput.x) > 0.5f || _holdingHeavyObject)
        {
            ExitLadder();
            return;
        }
        if (Mathf.Abs(_moveInput.y) > 0.2f)
        {
            // -------------------------------------------
            // If the entity just grabbed on to the ladder
            // -------------------------------------------
            if (!_climbingLadder)
            {
                EnterLadder();
            }
            
            if ((transform.position.y > _ladderBottom && _moveInput.y < 0.1f) || (transform.position.y < _ladderTop && _moveInput.y > 0.1f))
            {
                // Climbing ladder
                _movement += Vector2.up * _moveInput.y * _climbingSpeed;
            }
        }
    }

    protected virtual void EnterLadder()
    {
        // Get the bottom and the top of the ladder to stop the entity from climbing beyond them
        Vector2? ladderBottom = Ladder.GetLadderBottom(_collider, _ladderXCoord);
        Vector2? ladderTop = Ladder.GetLadderTop(_collider, _ladderXCoord);

        if (ladderBottom == null || ladderTop == null) return; // If the ladder does not have a top and a bottom it probably isn't a ladder and should not be climbed

        Vector2 ladderBottom2;
        Vector2 ladderTop2;
        ladderBottom2 = ((Vector2)ladderBottom + Vector2.zero); // This is stupid and I hate it. Why can't I just get the 'y' of a nullable Vector2?????
        ladderTop2 = ((Vector2)ladderTop + Vector2.zero);
        _ladderBottom = ladderBottom2.y;
        _ladderTop = ladderTop2.y;

        _climbingLadder = true;
        _collider.isTrigger = true; // The entity should not collide with the ground when climbing
        _movement = Vector2.zero;
        _currentSpeed = 0f;
        transform.position = new Vector2(_ladderXCoord, Mathf.Max(transform.position.y, _ladderBottom));
        _grabbedLadderThisFrame = true;
    }

    protected virtual void ExitLadder()
    {
        if (_climbingLadder) _jumpBuffer = 0f; // The entity should not jump off the ladder if they were going to do it before getting onto the ladder
            _climbingLadder = false;
        _leftLadderThisFrame = true;
    }

    protected virtual void Jump()
    {
        _coyoteTime = 0f;
        _grounded = false;
        _groundedFrames = 0;
        _gravity = Vector2.zero;
        _jumpVelocity = _jumpForce;
        _jumpedThisFrame = true;
    }

    protected void SetControl(bool toggle)
    {
        if (_isInControl == toggle) return;
        _isInControl = toggle;
        _climbingLadder = false;
        _collider.isTrigger = !toggle;
    }

    void JumpLogic()
    {
        // No jumping off the ladder but do remember that the entity has tried to jump already
        if (_climbingLadder)
        {
            _jumpVelocity = 0f;
            _alreadyJumped = true;
            return;
        }

        // The jump buffer gives the player more leniency on when the player presses jump
        _jumpBuffer -= Time.fixedDeltaTime;
        _movement += Vector2.up * _jumpVelocity;

        if (_grounded)
        {
            _jumpVelocity = 0f;
        }
        else if (_jumpVelocity > _maxGravity && _gravity.y == 0f)
        {
            if (Physics2D.OverlapBox((Vector2)transform.position + Vector2.up * _groundCastHeight, new Vector2(_collider.bounds.extents.x * 2 - 0.03f, 0.04f), 0f, _groundLayer)
            && _jumpVelocity > 0f)
            {
                _jumpVelocity = 0f;
                _jumpBuffer = 0f;
            }

            _jumpVelocity -= _jumpForce * Mathf.Lerp(_jumpApex, _jumpFallSpeed, Mathf.Abs(_jumpVelocity) * 0.1f) * Time.fixedDeltaTime;
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
            Jump();
        }
    }

    void TryToCatchOrThrow()
    {
        if (_climbingLadder) return; // Can't catch or throw stuff while on the ladder
        if (!_catchInput) _alreadyCaught = false;

        if (_catchInput && !_alreadyCaught)
        {
            // If the entity is holding something throw it, else try to pick up an item
            if (!_holdingSomething)
            {
                Collider2D holdableCollider = Physics2D.OverlapBox(transform.position, _collider.bounds.extents * 2, 0f, _holdableLayer);
                if (holdableCollider != null)
                {
                    Holdable holdable = holdableCollider.gameObject.GetComponent<Holdable>();
                    if (holdable.BeingHeld)
                    {
                        _holdingOut = true; // Can't pickup an item that someone is already holding
                    }
                    else
                    {
                        Catch(holdable);
                    }
                }
                else
                {
                    _holdingOut = true;
                }
            }
            else
            {
                Throw();
            }
        }
        else
        {
            _holdingOut = false;
        }
    }
    protected virtual void Catch(Holdable holdable)
    {
        _heldItem = holdable;
        _handTransform.localScale = new Vector3(1f, 1f, 1f);
        _heldItem.Pickup(_handTransform, this, _health, _facingLeft, ref _holdingHeavyObject, ref _heldItemIsFlipalbe, ref _holdingWeapon); // Puts the item in the entity's hand and checks if the object is heavy
        FlipLogic();
        _holdingSomething = true;
        _alreadyCaught = true;
        _holdingOut = false;
    }
    protected virtual void Throw()
    {
        Vector2 throwVector = Vector2.zero;
        if (!_grounded)
        {
            throwVector = Vector2.left;
        }
        else
        {
            throwVector = _slopeNormalPerpendicular;
        }
        Transform handParent = _handTransform.parent;
        Vector3 handPosition = _handTransform.localPosition;
        Quaternion handRotation = _handTransform.localRotation;
        _handTransform.parent = null;
        _handTransform.localPosition = transform.position + Vector3.up * (_collider.bounds.extents.y - 0.4f);
        _handTransform.localRotation = Quaternion.identity;
        _heldItem.Throw(throwVector * (_facingLeft ? 1f : -1f));
        _handTransform.parent = handParent;
        _handTransform.localPosition = handPosition;
        _handTransform.rotation = handRotation;
        _handTransform.localScale = Vector3.one;
        _alreadyCaught = true;
        _holdingSomething = false;
        _holdingHeavyObject = false;
        _holdingWeapon = false;
    }
    void CheckIfGrounded()
    {
        // Checks if the entity is climbing in which case they obviously aren't grounded
        if (_collider.isTrigger == true)
        {
            _grounded = false;
            _groundedFrames = 0;
            _coyoteTime = 0f;
            return;
        }

        // Raycast below to check for ground
        _grounded = (Physics2D.Raycast(transform.position, Vector2.down, _groundCastHeight, _groundLayer)
                  || Physics2D.Raycast((Vector2)transform.position + (Vector2.right * _collider.bounds.extents.x), Vector2.down, _groundCastHeight, _groundLayer)
                  || Physics2D.Raycast((Vector2)transform.position + (Vector2.left * _collider.bounds.extents.x), Vector2.down, _groundCastHeight, _groundLayer));
        if (_groundedFrames < 5 && _grounded)
        {
            // Grounded frames to stop the jitter that is caused by the entity landing when using this weird semi-dynamic rigidbody system
            _groundedFrames++;
        }
        if (_grounded)
        {
            // Coyote time lets the player jump a little while after walking of a platform to improve the game feel
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
        if (!_collider.isTrigger && !_climbingLadder) return; // This check is only relevant when the player is climbing

        if (Physics2D.OverlapBox(transform.position, _collider.bounds.extents, 0, _groundLayer) == null)
        {
            _insideGround = false;
        }
        else
        {
            _insideGround = true;
        }

        // If the entity is not inside ground or climbing a ladder they should be physical
        if (!_climbingLadder && !_insideGround)
        {
            _collider.isTrigger = false;
        }
    }

    void CalculateGravity()
    {
        // Gravity should not affect entites which are already on ground or are climbing
        if (_groundedFrames >= 5 || _climbingLadder || _jumpVelocity != 0f)
        {
            _gravity = Vector2.zero;
            return;
        }

        _gravity = Vector2.MoveTowards(_gravity, _maxGravity * Vector2.up, _fallAcceleration * 50f * Time.fixedDeltaTime);
    }

    protected virtual void FixedUpdate()
    {
        if (GameManager.CurrentlyInUI) return;
        _jumpedThisFrame = false;
        _leftLadderThisFrame = false;
        _grabbedLadderThisFrame = false;
        if (_isInControl)
        {
            _movement = Vector2.zero;

            // Stops the entity from sliding off of sloped surfaces
            if (_currentSpeed != 0f)
            {
                _collider.sharedMaterial = _moveMaterial;
            }
            else
            {
                _collider.sharedMaterial = _standMaterial;
            }

            GetInputs();
            CheckFacingLogic();
            CheckIfGrounded();
            CheckSlope();
            CalculateGravity();
            Move();
            ClimbLadder();
            JumpLogic();
            TryToCatchOrThrow();
            CheckIfInsideGround();
            _rigidBody.velocity = (_movement + _gravity) * 50f * Time.fixedDeltaTime;
            _previousPosition = transform.position;
        }
    }

    protected virtual void CheckFacingLogic()
    {
        bool oldFacing = _facingLeft;
        // Figure out which direction the entity is facing
        _facingLeft = _moveInput.x < 0f;

        if (oldFacing != _facingLeft) FlipLogic();
    }

    public virtual void DurabilityChanged()
    {

    }

    protected virtual void Animate(Animator anim)
    {
        anim.SetFloat("HorizontalSpeed", _movement.x);
        anim.SetFloat("VerticalSpeed", _jumpVelocity + _gravity.y);
        anim.SetFloat("HorizontalMoveInput", _moveInput.x);
        anim.SetFloat("VerticalMoveInput", transform.position.y + 0.1f < _ladderTop && transform.position.y - 0.1f > _ladderBottom ? _moveInput.y : 0f);
        anim.SetFloat("WalkSpeed", Mathf.Max(Mathf.Abs(_moveInput.x), 0.2f));
        anim.SetBool("Grounded", _grounded);
        anim.SetBool("HoldingOut", _holdingOut);
        anim.SetBool("FacingLeft", _facingLeft);
        if (_jumpedThisFrame)
        {
            anim.SetTrigger("Jump");
        }
        else
        {
            anim.ResetTrigger("Jump");
        }
        if (_leftLadderThisFrame)
        {
            anim.SetTrigger("LeftLadder");
        }
        else
        {
            anim.ResetTrigger("LeftLadder");
        }
    }

    void LateUpdate()
    {
        // Stops the entity from climbing off of the ladder
        if (_climbingLadder)
        {
            if (transform.position.y < _ladderBottom)
            {
                transform.position = new Vector2(transform.position.x, _ladderBottom);
            }
            if (transform.position.y > _ladderTop)
            {
                transform.position = new Vector2(transform.position.x, _ladderTop);
            }
        }
        CheckFacingLogic();
    }

    public Vector2 MoveInput {get { return _moveInput; }}
    public virtual void ClearHand()
    {
        if (_heldItem == null) return;
        _holdingSomething = false;
        _holdingHeavyObject = false;
        _holdingWeapon = false;
        _heldItem.transform.parent = null;
        _heldItem.transform.localScale = Vector3.one;
        _heldItem = null;
    }

    protected virtual void GetInputs()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericMover : MonoBehaviour, ILadderInteractable
{
    [SerializeField] float _speed = 7f, _climbingSpeed = 5f,
    _maxJumpBuffer = 0.2f, _maxCoyoteTime = 0.1f,
    _jumpForce = 13f, _jumpApex = 0.2f, _jumpFallSpeed = 3f,
    _fallAcceleration = 1f, _maxGravity = -15f;
    [SerializeField] protected LayerMask _groundLayer, _holdableLayer;
    [SerializeField] PhysicsMaterial2D _standMaterial, _moveMaterial;
    [SerializeField] Transform _handTransform;
    Holdable _heldItem;
    protected Vector2 _movement, _slopeNormalPerpendicular,
    _gravity, _moveInput;
    Rigidbody2D _rigidBody;
    Collider2D _collider;
    float _jumpBuffer = 0f, _coyoteTime = 0f,
     _jumpVelocity = 0f, _groundCastHeight, _ladderXCoord,
     _ladderBottom, _ladderTop;
    int _groundedFrames = 0;
    protected bool _grounded = true, _alreadyJumped = true,
    _climbingLadder = false, _nextToLadder = false,
    _insideGround = false, _holdingSomething = false,
    _alreadyCaught = false, _facingLeft = false,
    _holdingHeavyObject = false, _isInControl = true;

    protected bool _jumpInput, _useInput, _catchInput;
    
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _groundCastHeight = _collider.bounds.extents.y + 0.05f;
        _gravity = Vector2.zero;
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

        // Figure out which direction the player is facing
        if (_moveInput.x > 0f)
        {
            _facingLeft = false;
        }
        else if (_moveInput.x < 0f)
        {
            _facingLeft = true;
        }

        // If the entity is grounded and moving, walk along the slope
        if (_grounded)
        {
            Debug.DrawRay(transform.position, _slopeNormalPerpendicular, Color.red);
            _movement += new Vector2(
            -_moveInput.x * _speed * _slopeNormalPerpendicular.x,
            -_moveInput.x * _speed * _slopeNormalPerpendicular.y);
        }
        else
        {
            _movement += Vector2.right * _moveInput.x * _speed;
        }
    }

    void ClimbLadder()
    {
        // If the entity has left the ladder stop climbing
        if (!_nextToLadder || Mathf.Abs(_moveInput.x) > 0.5f || _holdingHeavyObject)
        {
            if (_climbingLadder) _jumpBuffer = 0f; // The entity should not jump off the ladder if they were going to do it before getting onto the ladder
            _climbingLadder = false;
            return;
        }
        if (Mathf.Abs(_moveInput.y) > 0.2f)
        {
            // If the entity just grabbed on to the ladder
            if (!_climbingLadder)
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
        // No jumping off the ladder but do remember that the entity has tried to jump already
        if (_climbingLadder)
        {
            _jumpVelocity = 0f;
            _alreadyJumped = true;
            return;
        }

        // The jump buffer exists for players so that the game feels more responsive
        _jumpBuffer -= Time.fixedDeltaTime;

        _movement += Vector2.up * _jumpVelocity;

        // Jump velocity should not push the entity down
        if (_jumpVelocity < 0f || _grounded)
        {
            _jumpVelocity = 0f;
        }

        else if (_jumpVelocity > 0f)
        {
            // When the entity is at the apex of the jump it should hang in the air for a while longer for the game feel
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

    void Catch()
    {
        if (_climbingLadder) return; // Can't catch or throw stuff while on the ladder
        if (!_catchInput) _alreadyCaught = false;

        if (_catchInput && !_alreadyCaught)
        {
            // If the entity is holding something throw it, else try to pick up an item
            if (!_holdingSomething)
            {
                Collider2D holdable = Physics2D.OverlapBox(transform.position, _collider.bounds.extents * 2, 0f, _holdableLayer);
                if (holdable != null)
                {
                    _heldItem = holdable.gameObject.GetComponent<Holdable>();
                    if (_heldItem.BeingHeld) return; // Can't pickup an item that someone is already holding
                    _holdingHeavyObject = _heldItem.Pickup(_handTransform); // Puts the item in the entity's hand and checks if the object is heavy
                    _holdingSomething = true;
                    _alreadyCaught = true;
                }
            }
            else
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
                _heldItem.Throw(throwVector * (_facingLeft ? 1f : -1f));
                _alreadyCaught = true;
                _holdingSomething = false;
                _holdingHeavyObject = false;
            }
        }
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
        if (_groundedFrames >= 5 || _climbingLadder)
        {
            _gravity = Vector2.zero;
            return;
        }

        _gravity = Vector2.MoveTowards(_gravity, _maxGravity * Vector2.up, _fallAcceleration * 50f * Time.fixedDeltaTime);
    }

    void FixedUpdate()
    {
        if (GameManager.CurrentlyInUI) return;
        if (_isInControl)
        {
            _movement = Vector2.zero;

            // Stops the entity from sliding off of sloped surfaces
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
            Catch();
            CheckIfInsideGround();
            _rigidBody.velocity = (_movement + _gravity) * 50f * Time.fixedDeltaTime;
        }
        FixedUpdateLogic();
    }

    protected virtual void FixedUpdateLogic()
    {

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
        LateUpdateLogic();
    }

    protected virtual void LateUpdateLogic()
    {
        
    }

    public Vector2 MoveInput {get { return _moveInput; }}
    public void ClearHand()
    {
        _holdingSomething = false;
        _heldItem = null;
    }

    protected virtual void GetInputs()
    {
        
    }
}

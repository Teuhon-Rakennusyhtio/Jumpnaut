using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Accessibility;

public abstract class GenericMover : MonoBehaviour, ILadderInteractable
{
    [SerializeField] protected float _speed = 7f, _climbingSpeed = 5f,
    _maxJumpBuffer = 0.2f, _maxCoyoteTime = 0.1f,
    _jumpForce = 13f, _jumpApex = 0.2f, _jumpFallSpeed = 3f,
    _fallAcceleration = 1f, _maxGravity = -15f,
    _groundAcceleration = 1f, _airAcceleration = 1f,
    _groundDecceleration = 1f, _airDecceleration = 1f;
    [SerializeField] protected LayerMask _groundLayer, _holdableLayer;
    [SerializeField] PhysicsMaterial2D _standMaterial, _moveMaterial;
    [SerializeField] protected Transform _handTransform, _headTransform;
    [SerializeField] protected GenericHealth _health;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected Transform _mainRig, _leftArm, _rightArm, _climbArm;
    protected Holdable _heldItem;
    protected Vector2 _movement, _slopeNormal, _slopeNormalPerpendicular,
    _gravity, _moveInput, _previousPosition;
    Rigidbody2D _rigidBody;
    protected Collider2D _collider;
    protected float _jumpBuffer = 0f, _coyoteTime = 0f,
     _jumpVelocity = 0f, _groundCastHeight, _ladderXCoord,
     _ladderBottom, _ladderTop, _currentSpeed, _currentWeaponCooldown,
     _weaponCooldown, _weaponAnimationSpeed;
    protected int _groundedFrames = 0;
    int _handAnimationBeforeFullBodyAnimation, _bodyAnimationBeforeFullBodyAnimation;
    bool _fullbodyAnimationIsPlaying;
    protected bool _grounded = true, _alreadyJumped = true,
    _climbingLadder = false, _nextToLadder = false,
    _insideGround = false, _holdingSomething = false,
    _alreadyCaught = false, _facingLeft = false,
    _holdingHeavyObject = false, _holdingWeapon,
     _grabbedLadderThisFrame, _leftLadderThisFrame,
    _heldItemIsFlipalbe, _jumpedThisFrame, _holdingOut,
    _alreadyUsed, _currentlyUsing, _isInControl = true;
    public SpriteRenderer _helmetMain, _helmetClimb;
    public AudioManager audioManager;

    protected string _weaponUseAnimation;
    protected bool _jumpInput, _useInput, _catchInput;
    
    public bool IsInControl { get { return _isInControl; } }
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _groundCastHeight = _collider.bounds.extents.y + 0.05f;
        _gravity = Vector2.zero;
        _previousPosition = transform.position;
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
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
        else if (_isInControl)
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, _moveInput.x * _speed, Time.fixedDeltaTime * acceleration);
        }
        else
        {
            _currentSpeed = 0f;
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

    protected virtual void UseWeaponLogic()
    {
        _currentWeaponCooldown -= Time.deltaTime;
        if (_useInput && _holdingWeapon && _currentWeaponCooldown < 0f && _isInControl)
        {
            if (!_alreadyUsed)
            {
                _alreadyUsed = true;
                _currentWeaponCooldown = _weaponCooldown;
                if (_weaponUseAnimation != "")
                {
                    _animator?.Play(_weaponUseAnimation + (_facingLeft ? " Left" : " Right"), -1, 0f);
                }
                StartUsingWeapon();
            }
            else
            {
                _currentlyUsing = true;
                ActivelyUsingWeapon();
            }
        }
        else
        {
            _alreadyUsed = false;
            StopUsingWeapon();
        }
    }

    protected virtual void StartUsingWeapon()
    {
        _heldItem.Attack();
        if (_heldItem.IsHelmet)
        {
            if (_helmetMain != null && _health.CurrentHealth < _health.MaxHealth)
            {
                _health.Heal(3);
                Destroy(_heldItem.gameObject);
                ClearHand();
                _animator?.Play("Equip Helmet" + (_facingLeft ? " Left" : " Right"), -1, 0f);
                _helmetMain.enabled = true;
                _helmetClimb.enabled = true;
            }
        }
    }

    protected virtual void ActivelyUsingWeapon()
    {

    }

    protected virtual void StopUsingWeapon()
    {

    }

    protected virtual void FlipLogic()
    {
        if (!_heldItemIsFlipalbe)
        {
            _handTransform.localScale = new Vector3((_facingLeft ? -1f : 1f), 1f, 1f);
        }
        if (!_holdingSomething && !_catchInput)
        {
            _animator?.Play("Empty Hand");
        }
        else if (!_heldItemIsFlipalbe && _handTransform.childCount > 0)
        {
            Transform heldItem = _handTransform.GetChild(0).transform;
            heldItem.localPosition = new Vector2(-heldItem.localPosition.x, heldItem.localPosition.y);
        }
        if (_holdingOut)
        {
            if (_facingLeft)
            {
                _animator?.Play("Hold Out Left", -1, 1f);
            }
            else
            {
                _animator?.Play("Hold Out Right", -1, 1f);
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
        if (Mathf.Abs(_moveInput.y) > 0.2f && _isInControl)
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

        _animator?.Play("Climbing");
        _handTransform.parent = _climbArm;
        _handTransform.localScale = Vector3.one;
        _handTransform.localPosition = Vector3.right * 0.45f;
        _handTransform.localRotation = Quaternion.identity;
    }

    protected virtual void ExitLadder()
    {
        if (_climbingLadder) _jumpBuffer = 0f; // The entity should not jump off the ladder if they were going to do it before getting onto the ladder
        _climbingLadder = false;
        _leftLadderThisFrame = true;

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

    protected virtual void Jump()
    {
        Debug.Log("jump" + Time.frameCount);
        _coyoteTime = 0f;
        _grounded = false;
        _groundedFrames = 0;
        _gravity = Vector2.zero;
        _jumpVelocity = _jumpForce;
        _jumpedThisFrame = true;
        if (_facingLeft)
        {
            _animator?.Play("Left Jump Start");
        }
        else
        {
            _animator?.Play("Right Jump Start");
        }
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
        // Also don't do jump logic when the entity is being moved by external forces.
        if (_climbingLadder || !_isInControl)
        {
            _jumpVelocity = 0f;
            _alreadyJumped = true;
            return;
        }

        // The jump buffer gives the player more leniency on when the player presses jump
        _jumpBuffer -= Time.fixedDeltaTime;
        _movement += Vector2.up * _jumpVelocity;

        if (_groundedFrames > 3f)
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
            _jumpBuffer = 0f;
            Jump();
        }
    }

    void TryToCatchOrThrow()
    {
        if (_climbingLadder) return; // Can't catch or throw stuff while on the ladder
        if (!_catchInput) _alreadyCaught = false;

        if (_catchInput && !_alreadyCaught && _isInControl)
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
        _heldItem.Pickup(_handTransform, this, _health, _facingLeft, ref _holdingHeavyObject, ref _heldItemIsFlipalbe, ref _holdingWeapon, ref _weaponCooldown, ref _weaponUseAnimation, ref _weaponAnimationSpeed); // Puts the item in the entity's hand and checks if the object is heavy
        FlipLogic();
        _holdingSomething = true;
        _alreadyCaught = true;
        _holdingOut = false;
        if (_holdingHeavyObject)
        {
            _animator?.Play("Pickup Two Handed");
        }
        else if (!_facingLeft)
        {
            _animator?.Play("Pickup Right");
        }
        else
        {
            _animator?.Play("Pickup Left");
        }
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
        if (!_facingLeft)
        {
            _animator?.Play("Throw Right");
        }
        else
        {
            _animator?.Play("Throw Left");
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
        _slopeNormal = hit.normal.normalized;
        _slopeNormalPerpendicular = Vector2.Perpendicular(hit.normal).normalized;
    }

    protected void CheckIfInsideGround()
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

    protected void CalculateGravity()
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

        _movement = Vector2.zero;

        GetInputs();
        JumpLogic();
        CheckFacingLogic();
        CheckIfGrounded();
        CheckSlope();
        // Stops the entity from sliding off of sloped surfaces
        if (_currentSpeed != 0f)
        {
            _collider.sharedMaterial = _moveMaterial;
        }
        else
        {
            _collider.sharedMaterial = _standMaterial;
        }
        if (_isInControl)
        {
            CalculateGravity();
            CheckIfInsideGround();
        }
        Move();
        UseWeaponLogic();
        ClimbLadder();
        TryToCatchOrThrow();

        /*if (_isInControl)
        {
            //_movement = Vector2.zero;

            // Stops the entity from sliding off of sloped surfaces
            if (_currentSpeed != 0f)
            {
                _collider.sharedMaterial = _moveMaterial;
            }
            else
            {
                _collider.sharedMaterial = _standMaterial;
            }
            CalculateGravity();
            Move();
            UseWeaponLogic();
            ClimbLadder();
            TryToCatchOrThrow();
            CheckIfInsideGround();
        }*/
        _rigidBody.velocity = (_movement + _gravity) * 50f * Time.fixedDeltaTime;
        _previousPosition = transform.position;
        if (_animator != null) Animate(_animator);
    }

    protected virtual void CheckFacingLogic()
    {
        bool oldFacing = _facingLeft;
        // Figure out which direction the entity is facing
        _facingLeft = _mainRig.localScale.x < 0f;
        
        //_facingLeft = _moveInput.x < 0f;

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
        anim.SetFloat("WeaponAnimationSpeed", _weaponAnimationSpeed);
        anim.SetBool("Grounded", _coyoteTime > 0f);
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

    protected void PlayFullBodyAnimation(string animation)
    {
        if (_animator == null) return;
        if (!_fullbodyAnimationIsPlaying)
        {
            _bodyAnimationBeforeFullBodyAnimation = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            _handAnimationBeforeFullBodyAnimation = _animator.GetCurrentAnimatorStateInfo(1).shortNameHash;
        }
        _fullbodyAnimationIsPlaying = true;
        _animator.CrossFade("Empty Hand", 0.1f);
        _animator.CrossFade(animation, 0.1f);
    }

    protected void EndFullBodyAnimation()
    {
        if (!_fullbodyAnimationIsPlaying) return;
        _fullbodyAnimationIsPlaying = false;
        _animator.CrossFade(_bodyAnimationBeforeFullBodyAnimation, 0.1f);
        _animator.CrossFade(_handAnimationBeforeFullBodyAnimation, 0.1f);
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
        _animator?.CrossFade("Empty Hand", 0.7f);
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

    public virtual void Die()
    {
        
    }

    public virtual void Damaged(float iFrames)
    {
        ExitLadder();
        //_coyoteTime = 0f;
        _animator?.Play((_facingLeft ? "Left " : "Right ") + "Fall");
        if (_health.CurrentHealth <= 1 && _helmetMain != null)
        {
            _helmetMain.enabled = false;
            _helmetClimb.enabled = false;
        }
    }
}

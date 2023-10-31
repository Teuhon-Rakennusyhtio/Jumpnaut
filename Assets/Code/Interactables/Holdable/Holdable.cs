using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DurabilityType
{
    none,
    digital,
    analog
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Holdable : MonoBehaviour
{
    [SerializeField] Vector2 _positionInHand;
    [SerializeField] protected Weapon _weapon;
    protected string _weaponUseAnimation;
    protected Collider2D _weaponCollider;
    protected GenericMover _holder;
    SpriteRenderer _renderer;
    protected Rigidbody2D _rigidBody;
    CircleCollider2D _collider;
    LayerMask _groundLayer;
    protected bool _thrown = false;
    float _xVelocity = 0f, _yVelocity = 0f, _outOfViewTime = 0f, _timeSinceThrown = 0f;
    public bool BeingHeld { get; set; }
    //public bool IsMeleeWeapon { get { return _isMeleeWeapon; } }
    Vector3 _realSize;
    
    [SerializeField] protected float _throwFallTime = 1f, _terminalVelocity = -15f, _fallAcceleration = 1f, _throwForce = 15f, _throwTorque = 1f;
    [SerializeField] protected bool _breaksOnImpact = false, _isHeavy = false, _flipable = true, _isWeapon = false;
    [SerializeField] DurabilityType _durabilityType = 0;
    [SerializeField] int _digitalDurability = 3;
    [SerializeField] float _analogDurability = 1f;
    [SerializeField] float _weaponCooldown;
    [SerializeField] Sprite _itemIcon;
    float _maxAnalogDurability;
    int _maxDigitalDurability;

    public Sprite ItemIcon { get { return _itemIcon; } }
    public int DigitalDurability { get { return _digitalDurability; } }
    public float AnalogDurability { get { return _analogDurability; } }
    public string WeaponUseAnimation { get { return _weaponUseAnimation; } }

    
    void Awake()
    {
        _maxAnalogDurability = _analogDurability;
        _maxDigitalDurability = _digitalDurability;
        _realSize = transform.localScale;
        _groundLayer = LayerMask.NameToLayer("Ground");
        _renderer = GetComponent<SpriteRenderer>();
        _rigidBody = GetComponent<Rigidbody2D>();

        if (_analogDurability == 0f)
        {
            Debug.LogError($"On item {gameObject.name}, the analog durability can't be zero!");
            _analogDurability = 1f;
        }
        if (_weapon != null)
            _weaponCollider = _weapon.GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        if (_thrown)
        {
            _timeSinceThrown += Time.fixedDeltaTime;


            // Let the "physics" take hold after the wanted duration of time has passed since the item was thrown.
            if (_timeSinceThrown > _throwFallTime && _yVelocity > _terminalVelocity)
            {
                _yVelocity = Mathf.MoveTowards(_yVelocity, _terminalVelocity, _fallAcceleration * 50f * Time.fixedDeltaTime);
                _xVelocity = Mathf.MoveTowards(_xVelocity, 0f, _fallAcceleration * 25f * Time.fixedDeltaTime);
            }

            _rigidBody.velocity = new Vector2(_xVelocity, _yVelocity);
            // If the thrown holdable hasn't been seen in 30 seconds it probably doesn't need to exist anymore.
            if (!_renderer.isVisible) _outOfViewTime += Time.fixedDeltaTime;
            else _outOfViewTime = 0f;
        }
        if (_outOfViewTime > 30f) Break();
    }

    public void Throw(Vector2 direction)
    {
        bool flipped = transform.lossyScale.x < 0f;
        transform.parent = null;
        transform.localScale = flipped ? new Vector3(-_realSize.x, _realSize.y, _realSize.z) : _realSize;
        //transform.localScale = _realSize;
        _rigidBody.isKinematic = false;
        Vector2 throwVector = direction * _throwForce;
        _xVelocity = throwVector.x;
        _yVelocity = throwVector.y;
        _rigidBody.AddTorque(_throwTorque * (direction.x < 0f ? 1f : -1f), ForceMode2D.Impulse);
        _timeSinceThrown = 0f;
        _thrown = true;
        BeingHeld = false;
        _holder = null;
        OnThrow(direction);
    }

    protected void RemoveDurability(int removedDurablity)
    {
        if (_weapon.Alignment == WeaponAlignment.enemy) return; // Enemies will not lose item durability
        removedDurablity = Mathf.Abs(removedDurablity);
        if (_durabilityType == DurabilityType.digital)
        {
            if (_digitalDurability <= 0) return;
            _digitalDurability -= removedDurablity;
            if (_digitalDurability <= 0)
            {
                _digitalDurability = 0;
                OnOutOfDurability();
            }
        }
        else if (_durabilityType == DurabilityType.analog)
        {
            if (_analogDurability <= 0f) return;
            _analogDurability -= removedDurablity;
            if (_analogDurability <= 0f)
            {
                _analogDurability = 0f;
                OnOutOfDurability();
            }
        }
        else
        {
            Debug.LogWarning($"Cannot remove durability from {gameObject.name} because it has no durability type!");
        }
        _holder?.DurabilityChanged();
    }

    protected void RemoveDurability(float removedDurablity)
    {
        if (_weapon.Alignment == WeaponAlignment.enemy) return; // Enemies will not lose item durability
        removedDurablity = Mathf.Abs(removedDurablity);
        if (_durabilityType == DurabilityType.digital)
        {
            Debug.LogWarning($"{gameObject.name} has digital durability and only integers are recommended to be removed from it! (tried to remove a float value {removedDurablity} from it)");
            if (_digitalDurability <= 0) return;
            _digitalDurability -= (int)removedDurablity;
            if (_digitalDurability <= 0)
            {
                _digitalDurability = 0;
                OnOutOfDurability();
            }
        }
        else if (_durabilityType == DurabilityType.analog)
        {
            if (_analogDurability <= 0f) return;
            _analogDurability -= removedDurablity;
            if (_analogDurability <= 0f)
            {
                _analogDurability = 0f;
                OnOutOfDurability();
            }
        }
        else
        {
            Debug.LogWarning($"Cannot remove durability from {gameObject.name} because it has no durability type!");
        }
        _holder?.DurabilityChanged();
    }

    protected virtual void OnThrow(Vector2 direction)
    {
        if (_weapon != null)
            _weapon.Thrown = true;
    }

    public void Attack()
    {
        OnAttack();
    }

    protected virtual void OnAttack()
    {
        
    }

    protected virtual void OnOutOfDurability()
    {
        Break();
    }

    public void Pickup(Transform hand, GenericMover holder, GenericHealth health, bool isFacingLeft, ref bool heavy, ref bool flipable, ref bool isWeapon, ref float weaponCooldown, ref string WeaponUseAnimation)
    {
        _thrown = false;
        _rigidBody.totalTorque = 0f;
        _rigidBody.freezeRotation = true;
        _rigidBody.freezeRotation = false;
        _rigidBody.isKinematic = true;
        _xVelocity = 0f;
        _yVelocity = 0f;
        _rigidBody.velocity = Vector2.zero;
        transform.parent = hand;
        transform.localScale = _realSize;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = _positionInHand;
        if (!_flipable)
        {
            transform.localPosition += (isFacingLeft ? Vector3.zero : Vector3.left * _positionInHand.x * 2);
        }
        _holder = holder;
        BeingHeld = true;
        OnPickup(hand, health);
        heavy = _isHeavy;
        flipable = _flipable;
        isWeapon = _isWeapon;
        weaponCooldown = _weaponCooldown;
        WeaponUseAnimation = _weaponUseAnimation;
    }

    protected virtual void OnPickup(Transform hand, GenericHealth health)
    {
        if (health != null && _weapon != null)
        {
            _weapon.Alignment = health.Alignment;
            _weapon.Thrown = false;
        } 
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (_thrown && collision.gameObject.layer == _groundLayer)
        {
            if (_breaksOnImpact)
            {
                _holder?.ClearHand();
                BeingHeld = true;
                Break();
            }
            else
            {
                if (collision.GetContact(0).point.y < transform.position.y)
                {
                    _xVelocity = 0f;
                }
                else
                {
                    _timeSinceThrown = _throwFallTime;
                }

            }
        }
    }

    public virtual void Break()
    {
        _holder?.ClearHand();
        Destroy(gameObject);
    }

    public void GetHoldableEventArgs(HoldableEventArgs args)
    {
        args.ItemIcon = _itemIcon;
        args.MaxDigitalDurability = _maxDigitalDurability;
        args.DurabilityType = _durabilityType;
        args.DigitalDurability = _digitalDurability;
        args.AnalogDurability = _analogDurability / _maxAnalogDurability;
    }
}

public class HoldableEventArgs : EventArgs
{
    public Sprite ItemIcon { get; set; }
    public DurabilityType DurabilityType { get; set; }
    public int MaxDigitalDurability { get; set; }
    public int DigitalDurability { get; set; }
    public float AnalogDurability { get; set; }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Holdable : MonoBehaviour
{
    [SerializeField] Vector2 _positionInHand;
    [SerializeField] protected Weapon[] _weapons;
    protected Collider2D[] _weaponColliders;
    GenericMover _holder;
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
    [Tooltip("0 = No durability, 1 = Digital durability, 2 >= Analog durability")]
    [SerializeField] int _durabilityType = 0;
    [SerializeField] int _digitalDurability = 3;
    [SerializeField] float _analogDurability = 1f;
    [SerializeField] Sprite _itemIcon;


    
    void Awake()
    {
        _realSize = transform.localScale;
        _groundLayer = LayerMask.NameToLayer("Ground");
        _renderer = GetComponent<SpriteRenderer>();
        _rigidBody = GetComponent<Rigidbody2D>();

        if (_analogDurability == 0f)
        {
            Debug.LogError($"On item {gameObject.name}, analog durability can't be zero!");
            _analogDurability = 1f;
        }
        if (_weapons == null) return;
        _weaponColliders = new Collider2D[_weapons.Length];
        for(int i = 0; i < _weapons.Length; i++)
        {
            _weaponColliders[i] = _weapons[i].GetComponent<Collider2D>();
        }
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

    protected virtual void OnThrow(Vector2 direction)
    {
        if (_weapons == null) return;
        foreach (Weapon weapon in _weapons)
        {
            weapon.Thrown = true;
        }
    }

    public void Pickup(Transform hand, GenericMover holder, bool isFacingLeft, ref bool heavy, ref bool flipable, ref bool isWeapon)
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
        OnPickup(hand);
        heavy = _isHeavy;
        flipable = _flipable;
        isWeapon = _isWeapon;
    }

    protected virtual void OnPickup(Transform hand)
    {
        if (_weapons == null) return;
        GenericHealth health = hand.parent.GetComponentInChildren<GenericHealth>();
        if (health != null)
        {
            foreach (Weapon weapon in _weapons)
            {
                weapon.Alignment = health.Alignment;
                weapon.Thrown = false;
            }
        } 
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (_thrown && collision.gameObject.layer == _groundLayer)
        {
            if (_breaksOnImpact)
            {
                if (_holder != null) _holder.ClearHand();
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
        Destroy(gameObject);
    }
}

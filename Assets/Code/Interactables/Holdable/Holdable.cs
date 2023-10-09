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
    SpriteRenderer _renderer;
    protected Rigidbody2D _rigidBody;
    CircleCollider2D _collider;
    LayerMask _groundLayer;
    protected bool _thrown = false;
    float _xVelocity = 0f, _yVelocity = 0f, _outOfViewTime = 0f, _timeSinceThrown = 0f;
    public bool BeingHeld = false;
    Vector3 _realSize;
    

    // These can be changed in inherited classes
    protected float _throwFallTime = 1f, _terminalVelocity = -15f, _fallAcceleration = 1f, _throwForce = 15f, _throwTorque = 0.5f;
    protected bool _breaksOnImpact = false, _isHeavy = false, _flipable = true;


    
    void Awake()
    {
        _realSize = transform.localScale;
        _groundLayer = LayerMask.NameToLayer("Ground");
        _renderer = GetComponent<SpriteRenderer>();
        _rigidBody = GetComponent<Rigidbody2D>();
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
        OnThrow(direction);
    }

    protected virtual void OnThrow(Vector2 direction)
    {

    }

    public void Pickup(Transform hand, ref bool heavy, ref bool flipable)
    {
        //bool flipped = transform.localScale.x < 0f;
        //transform.localScale = flipped ? new Vector3(-_realSize.x, _realSize.y, _realSize.z) : _realSize;
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
        transform.localPosition = Vector2.zero;
        BeingHeld = true;
        OnPickup(hand);
        heavy = _isHeavy;
        flipable = _flipable;

    }

    protected virtual void OnPickup(Transform hand)
    {

    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (_thrown && collision.gameObject.layer == _groundLayer)
        {
            if (_breaksOnImpact)
            {
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

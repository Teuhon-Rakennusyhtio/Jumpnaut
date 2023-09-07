using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class Holdable : MonoBehaviour
{
    SpriteRenderer _renderer;
    Rigidbody2D _rigidBody;
    LayerMask _groundLayer;
    bool _thrown = false;
    float _xVelocity = 0f, _yVelocity = 0f, _outOfViewTime = 0f, _timeSinceThrown = 0f;
    

    // These can be changed in inherited classes
    float _throwFallTime = 2f, _terminalVelocity = -15f, _fallAcceleration = 1f, _throwForce = 20f, _throwTorque = 1f;
    bool _breaksOnImpact = false;


    
    void Start()
    {
        _groundLayer = LayerMask.NameToLayer("Ground");
        _renderer = GetComponent<SpriteRenderer>();
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (_thrown)
        {
            _timeSinceThrown += Time.fixedDeltaTime;

            // Let the "physics" take hold after the wanted duration of time has passed since the item was thrown
            if (_timeSinceThrown > _throwFallTime && _yVelocity < _terminalVelocity)
            {
                _yVelocity = Mathf.MoveTowards(_yVelocity, _terminalVelocity, _fallAcceleration * 50f * Time.fixedDeltaTime);
                _xVelocity = Mathf.MoveTowards(_xVelocity, 0f, _fallAcceleration * 50f * Time.fixedDeltaTime);
            }


            // If the thrown holdable hasn't been seen in two seconds it probably doesn't need to exist anymore
            if (!_renderer.isVisible) _outOfViewTime += Time.fixedDeltaTime;
            else _outOfViewTime = 0f;
        }
        if (_outOfViewTime > 2f) Break();
    }

    public void Throw(Vector2 direction)
    {
        transform.parent = null;
        _rigidBody.isKinematic = false;
        _rigidBody.AddForce(direction * _throwForce, ForceMode2D.Impulse);
        _rigidBody.AddTorque(_throwTorque, ForceMode2D.Impulse);
        _thrown = true;
    }

    public void Pickup(Transform hand)
    {
        _rigidBody.isKinematic = true;
        transform.parent = hand;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector2.zero;
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
                _xVelocity = 0f;
            }
        }
    }

    void Break()
    {
        Destroy(gameObject);
    }
}

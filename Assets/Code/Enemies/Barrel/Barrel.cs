using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Barrel : MonoBehaviour, ILadderInteractable
{
    CircleCollider2D _collider;
    Rigidbody2D _rigidbody;
    Vector2 _slopeNormalPerpendicular, _gravity;
    bool _grounded, _climbingLadder;
    float _groundCastHeight = 0.01f, _direction = 1f;
    [SerializeField] float _speed = 1f, _maxGravity = -15f, _fallAcceleration = 1f;
    [SerializeField] LayerMask _groundLayer;
    void Start()
    {
        _collider = GetComponent<CircleCollider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _groundCastHeight += _collider.bounds.extents.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool oldGrounded = _grounded;
        _grounded = GroundCheck();
        

        if (!oldGrounded && _grounded)
        {
            _slopeNormalPerpendicular = SlopeCheck();
            if (_slopeNormalPerpendicular.y > 0f) _direction = 1f;
            else if (_slopeNormalPerpendicular.y < 0f) _direction = -1f;
        }

        CalculateGravity();
        if (!_grounded)
        {
            _rigidbody.velocity = _gravity;
        }
        else
        {
            _rigidbody.velocity = new Vector2(-_direction * _speed * _slopeNormalPerpendicular.x,
                                     -_direction * _speed * _slopeNormalPerpendicular.y);
            if (Physics2D.Raycast(transform.position, Vector2.right * _direction, _groundCastHeight, _groundLayer))
            {
                Break();
            }
        }
    }

    void Break()
    {
        Destroy(gameObject);
    }

    bool GroundCheck()
    {
        return (Physics2D.Raycast(transform.position, Vector2.down, _groundCastHeight, _groundLayer)
                  || Physics2D.Raycast((Vector2)transform.position + (Vector2.right * _collider.bounds.extents.x), Vector2.down, _groundCastHeight, _groundLayer)
                  || Physics2D.Raycast((Vector2)transform.position + (Vector2.left * _collider.bounds.extents.x), Vector2.down, _groundCastHeight, _groundLayer));
    }

    Vector2 SlopeCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, Vector2.down, 1f, _groundLayer);
        if (!hit) return Vector2.right;
        return Vector2.Perpendicular(hit.normal).normalized;
    }

    void CalculateGravity()
    {
        // Gravity should not affect entites which are already on ground or are climbing
        if (_grounded || _climbingLadder)
        {
            _gravity = Vector2.zero;
            return;
        }

        _gravity = Vector2.MoveTowards(_gravity, _maxGravity * Vector2.up, _fallAcceleration * 50f * Time.fixedDeltaTime);
    }

    public void OnLadderEnter(float xCoord)
    {

    }
    public void OnLadderExit()
    {

    }
}

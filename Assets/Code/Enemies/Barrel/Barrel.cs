using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour, ILadderInteractable
{
    CircleCollider2D _collider;
    Rigidbody2D _rigidbody;
    Vector2 _slopeNormalPerpendicular, _gravity, _ladderTop, _ladderBottom;
    bool _grounded, _nextToLadders, _goingDownLadder;
    float _groundCastHeight = 0.01f, _direction = 1f, _ladderXPosition = 0f;
    [SerializeField] float _speed = 1f, _ladderSpeed = 4f, _maxGravity = -15f, _fallAcceleration = 1f;
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
            _rigidbody.MovePosition(Physics2D.Raycast(transform.position, Vector2.down, 2f, _groundLayer).point + Vector2.up * _collider.bounds.extents.y);
        }

        CalculateGravity();
        if (!_grounded)
        {
            if (_goingDownLadder)
            {
                _rigidbody.MovePosition((Vector2)transform.position + Vector2.down * _ladderSpeed * Time.fixedDeltaTime);
                if (transform.position.y < _ladderBottom.y)
                {
                    _rigidbody.MovePosition(_ladderBottom);
                    _goingDownLadder = false;
                    _nextToLadders = false;
                    _collider.isTrigger = false;
                }
            }
            else
            {
                _rigidbody.velocity = _gravity;
            }
        }
        else
        {
            if (_nextToLadders && Mathf.Abs(transform.position.x - _ladderXPosition) < 0.1f && !_goingDownLadder)
            {
                _goingDownLadder = true;
                _grounded = false;
                _rigidbody.velocity = Vector2.zero;
                _rigidbody.MovePosition(_ladderTop);
                _collider.isTrigger = true;
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
    }

    void Break()
    {
        Destroy(gameObject);
    }

    bool GroundCheck()
    {
        if (_goingDownLadder) return false;
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
        if (_grounded || _goingDownLadder)
        {
            _gravity = Vector2.zero;
            return;
        }

        _gravity = Vector2.MoveTowards(_gravity, _maxGravity * Vector2.up, _fallAcceleration * 50f * Time.fixedDeltaTime);
    }

    public void OnLadderEnter(float xCoord)
    {
        Vector2? nullableLadderTop = Ladder.GetLadderTop(_collider, xCoord);
        if (nullableLadderTop == null) return;
        _ladderTop = Vector2.zero + (Vector2)nullableLadderTop;
        _ladderBottom = Vector2.zero + (Vector2)Ladder.GetLadderBottom(_collider, xCoord);
        if (Random.Range(0f, 1f) > 0.49f && Mathf.Abs(transform.position.y - _ladderTop.y) < 0.2f)
        {
            _nextToLadders = true;
            _ladderXPosition = xCoord;
        }
    }
    public void OnLadderExit()
    {
        _nextToLadders = false;
    }
}

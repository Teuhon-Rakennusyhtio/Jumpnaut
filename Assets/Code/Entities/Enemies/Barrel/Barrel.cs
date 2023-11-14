using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Barrel : MonoBehaviour, ILadderInteractable
{
    CircleCollider2D _collider;
    Rigidbody2D _rigidbody;
    Vector2 _slopeNormalPerpendicular;
    Vector2 _gravity;
    Vector2 _ladderTop;
    Vector2 _ladderBottom;
    bool _grounded;
    bool _nextToLadders;
    bool _goingDownLadder;
    float _groundRaycastDistance = 0.01f;
    float _direction;
    float _ladderXPosition;
    const bool RollingAnimation = false, LadderAnimation = true;
    [Tooltip("The speed at which the barrel moves while on the ground.")]
    [SerializeField] float _speed = 1f;
    [Tooltip("The speed at which the barrel moves while on the stairs.")]
    [SerializeField] float _ladderSpeed = 4f;
    [Tooltip("The terminal velocity of the barrel.")]
    [SerializeField] float _maxGravity = -15f;
    [Tooltip("The speed at which the barrel's falling speed is approaching terminal velocity")]
    [SerializeField] float _fallAcceleration = 1f;
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] SpriteRenderer _rollingSprite, _ladderSprite;
    void Start()
    {
        _collider = GetComponent<CircleCollider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _groundRaycastDistance += _collider.bounds.extents.y;
    }

    void FixedUpdate()
    {
        CalculateGravity();

        bool wasGrounded = _grounded;
        _grounded = GroundCheck();

        if (!wasGrounded && _grounded)
        {
            PlaceBarrelOnGround();
        }
        
        if (!_grounded)
        {
            if (_goingDownLadder)
            {
                GoDownLadder();
            }
            else
            {
                Fall();
            }
        }
        else
        {
            float distanceToLadder = Mathf.Abs(transform.position.x - _ladderXPosition);
            if (_nextToLadders && distanceToLadder < 0.1f && !_goingDownLadder)
            {
                GetOnLadder();
            }
            else
            {
                Move();
                bool wallHit = Physics2D.Raycast(transform.position,
                                                 Vector2.right * _direction,
                                                 _groundRaycastDistance,
                                                 _groundLayer);
                if (wallHit)
                {
                    Break();
                }
            }
        }
    }

    void PlaceBarrelOnGround()
    {
        _slopeNormalPerpendicular = SlopeCheck();
        if (_slopeNormalPerpendicular.y > 0f)
            _direction = 1f;
        else if (_slopeNormalPerpendicular.y < 0f)
            _direction = -1f;

        Vector2 ground = Physics2D.Raycast(transform.position,
                                           Vector2.down,
                                           2f,
                                           _groundLayer).point;
        Vector2 offset = Vector2.up * _collider.bounds.extents.y;
        _rigidbody.MovePosition(ground + offset);
    }

    void GetOnLadder()
    {
        _goingDownLadder = true;
        _grounded = false;
        _rigidbody.velocity = Vector2.zero;
        _rigidbody.MovePosition(_ladderTop);
        _collider.isTrigger = true;
        ChangeAnimation(LadderAnimation);
    }

    void GoDownLadder()
    {
        float speed = _ladderSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition((Vector2)transform.position + Vector2.down * speed);

        // When the bottom is reached get off the ladder.
        if (transform.position.y < _ladderBottom.y)
        {
            ChangeAnimation(RollingAnimation);
            _rigidbody.MovePosition(_ladderBottom);
            _goingDownLadder = false;
            _nextToLadders = false;
            _collider.isTrigger = false;
        }
    }

    void Fall()
    {
        _rigidbody.velocity = _gravity;
    }

    void Move()
    {
        _rigidbody.velocity = new Vector2(
                        -_direction * _speed * _slopeNormalPerpendicular.x,
                        -_direction * _speed * _slopeNormalPerpendicular.y);
    }

    public void Break()
    {
        // TODO: make a breaking animation
        Destroy(gameObject);
    }

    bool GroundCheck()
    {
        if (_goingDownLadder) return false;
        return (Physics2D.Raycast(
                transform.position, Vector2.down,
                _groundRaycastDistance, _groundLayer)
            || Physics2D.Raycast(
                (Vector2)transform.position +
                (Vector2.right * _collider.bounds.extents.x),
                Vector2.down, _groundRaycastDistance, _groundLayer)
            || Physics2D.Raycast(
                (Vector2)transform.position +
                (Vector2.left * _collider.bounds.extents.x),
                Vector2.down, _groundRaycastDistance, _groundLayer));
    }

    Vector2 SlopeCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            (Vector2)transform.position,
            Vector2.down, 1f, _groundLayer);
        
        if (!hit) return Vector2.right;
        return Vector2.Perpendicular(hit.normal).normalized;
    }

    void CalculateGravity()
    {
        // Gravity should not affect barrels which are already on ground or are climbing
        if (_grounded || _goingDownLadder)
        {
            _gravity = Vector2.zero;
            return;
        }

        _gravity = Vector2.MoveTowards(_gravity, _maxGravity * Vector2.up, _fallAcceleration * 50f * Time.fixedDeltaTime);
    }

    void ChangeAnimation(bool animation)
    {
        _rollingSprite.enabled = !animation;
        _ladderSprite.enabled = animation;
    }

    public void OnLadderEnter(float xCoord)
    {
        Vector2? nullableLadderTop = Ladder.GetLadderTop(_collider, xCoord);
        Vector2? nullableLadderBottom = Ladder.GetLadderBottom(_collider, xCoord);
        if (nullableLadderTop == null) return;
        _ladderTop = Vector2.zero + (Vector2)nullableLadderTop;
        _ladderBottom = Vector2.zero + (Vector2)nullableLadderBottom;
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

    public void Push(float direction)
    {
        _direction = direction;
    }

    void Update()
    {
        _rollingSprite.transform.localRotation = Quaternion.Euler(0, 0, -1000 * _direction * Time.time);
    }
}

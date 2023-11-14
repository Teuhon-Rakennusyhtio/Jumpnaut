using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum EnemyState
{
    StandingStill,
    Patrolling,
    ChasingPlayer,
    EngagingPlayer,
    EscapingPlayer,
    SearchingForWeapon,
    ThrowingBarrels
}

public class EnemyMover : GenericMover
{
    const bool ToLeft = false;
    const bool ToRight = true;
    [SerializeField] GameObject _barrel;
    [SerializeField] LayerMask _ladderLayer, _entityLayer;
    [SerializeField] bool _patrolsWhenIdle = true;
    [SerializeField] bool _targetsPlayers = true;
    [SerializeField] bool _canUseLadders = true;
    //[SerializeField] bool _canJump = true;
    [SerializeField] bool _canUseWeapons = true;
    [SerializeField] bool _canThrowWeapons = true;
    [SerializeField] bool _canCatchWeapons = true;
    [SerializeField] bool _canSearchForWeapons = true;
    [SerializeField] bool _runsAwayWhenCloseToPlayerWhenWithoutWeapon = true;
    [SerializeField] bool _canThrowBarrels = false;
    [SerializeField] float _maxAggroTime = 5f;
    [SerializeField] float _aggroRadius = 3f;
    [SerializeField] float _attackDelay = 0.2f;
    [SerializeField] GameObject _spawnItem;
    [SerializeField] Transform _preDefinedPatrolPoint1;
    [SerializeField] Transform _preDefinedPatrolPoint2;
    [SerializeField] bool _barrelDirectionIsToTheLeft;
    Vector2 _patrolPoint1, _patrolPoint2;
    bool _newLadderFound = false, _ladderDirection;
    Vector2 _potentialLadderPos;
    Vector2 _waypoint, _ladderTarget;
    Transform _target;
    Vector2 _targetOldPosition;
    float _aggroTime, _idleTime, _attackTime;
    Holdable _targetHoldable;
    EnemyState _enemyState = EnemyState.StandingStill;
    Collider2D[] _nearbyPlayers;
    Transform _closestPlayer;
    bool _isDead = false;

    void Start()
    {
        if (_spawnItem != null)
        {
            Instantiate(_spawnItem, transform.position, Quaternion.identity);
            _catchInput = true;
        }
        if (_canThrowBarrels)
        {
            SetState(EnemyState.ThrowingBarrels);
        }
    }

    protected override void GetInputs()
    {
        if (!_isInControl) return;
        //_jumpInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.jump];
        //_useInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.use];
        //_catchInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pickup];
        //_pauseInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pause];
        _catchInput = false;
        _useInput = false;

        // Player proximity check
        if ((int)(Time.time * 10) % 9 == 0)
        {
            CheckForNearbyPlayers();
            if (_nearbyPlayers.Length > 0)
            {
                if (!_holdingSomething && _runsAwayWhenCloseToPlayerWhenWithoutWeapon)
                {
                    SetState(EnemyState.EscapingPlayer);
                }
                else if (_targetsPlayers && !_climbingLadder)
                {
                    SetState(EnemyState.EngagingPlayer);
                }
            }
        }

        switch (_enemyState)
        {
            case EnemyState.StandingStill:
                if (WeaponCheck())
                {
                    SetState(EnemyState.SearchingForWeapon);
                }
                else if (_patrolsWhenIdle && _idleTime > 0.2f)
                    SetState(EnemyState.Patrolling);
                break;
            case EnemyState.Patrolling:
                if (((Vector2)transform.position - _waypoint).sqrMagnitude < 0.01f)
                {
                    if (_waypoint == _patrolPoint1)
                    {
                        _waypoint = _patrolPoint2;
                    }
                    else
                    {
                        _waypoint = _patrolPoint1;
                    }
                }
                _moveInput = (Vector2.right * (_waypoint - (Vector2)transform.position).x).normalized * 0.5f;
                if (WeaponCheck())
                    SetState(EnemyState.SearchingForWeapon);
                break;
            case EnemyState.ChasingPlayer:
                _aggroTime -= Time.deltaTime;
                if (_aggroTime <= 0f && !_climbingLadder)
                    SetState(EnemyState.StandingStill);
                else
                    PathFindToTarget();
                break;
            case EnemyState.EngagingPlayer:
                _attackTime -= Time.deltaTime;
                Vector2 target = Vector2.zero;
                bool preferMelee;
                if (_facingLeft)
                {
                    if (_patrolPoint1.x < _closestPlayer.position.x)
                    {
                        target = _closestPlayer.position;
                        preferMelee = true;
                    }
                    else
                    {
                        target = _patrolPoint1;
                        preferMelee = false;
                    }
                    if (_closestPlayer.position.x > transform.position.x)
                        SetState(EnemyState.ChasingPlayer);
                }
                else
                {
                    if (_patrolPoint2.x > _closestPlayer.position.x)
                    {
                        target = _closestPlayer.position;
                        preferMelee = true;
                    }
                    else
                    {
                        target = _patrolPoint2;
                        preferMelee = false;
                    }
                    if (_closestPlayer.position.x < transform.position.x)
                        SetState(EnemyState.ChasingPlayer);
                }
                Vector2 fightPosition = target + Vector2.right * (_facingLeft ? 1f : -1f);
                if (NearToPoint(fightPosition) && _attackTime <= 0f)
                {
                    _attackTime = _attackDelay;
                    if (_holdingSomething && _heldItem != null)
                    {
                        if (preferMelee && _canUseWeapons && _holdingWeapon)
                        {
                            _useInput = true;
                        }
                        else if (_canThrowWeapons && Physics2D.Raycast(transform.position, _slopeNormalPerpendicular, _aggroRadius * 0.8f, _entityLayer))
                        {
                            _catchInput = true;
                        }
                    }
                    else
                    {
                        // TODO: Funny animation
                    }
                }
                else if ((!_canUseWeapons || !_holdingWeapon) && _attackTime <= 0f && _canThrowWeapons && Physics2D.Raycast(transform.position, _slopeNormalPerpendicular, _aggroRadius, _entityLayer))
                {
                    _catchInput = true;
                }
                _movement += Vector2.right * (fightPosition - (Vector2)transform.position).normalized.x * _speed * 0.9f;
                if (_nearbyPlayers.Length == 0)
                {
                    SetState(EnemyState.ChasingPlayer);
                }
                break;
            case EnemyState.EscapingPlayer:
                _moveInput = (Vector2.left * (_closestPlayer.position - transform.position).x).normalized * 1f;
                if (((Vector2)(_closestPlayer.position - transform.position)).sqrMagnitude > 300f)
                {
                    SetState(EnemyState.StandingStill);
                }
                break;
            case EnemyState.SearchingForWeapon:
                if (_targetHoldable.BeingHeld)
                    _target = null;
                PathFindToTarget();
                if (_target != null && NearToPoint((Vector2)_target.position, 0.5f))
                {
                    _catchInput = true;
                }
                if (_holdingSomething)
                {
                    SetState(EnemyState.StandingStill);
                }
                break;
            case EnemyState.ThrowingBarrels:
                if (_idleTime > _attackDelay)
                {
                    _idleTime = 0f;
                    GameObject newBarrel = Instantiate(_barrel, transform.position - Vector3.up * _collider.bounds.extents.y / 2, Quaternion.identity);
                    newBarrel.GetComponent<Barrel>().Push(_barrelDirectionIsToTheLeft ? -1f : 1f);
                }
                break;
        }
    }

    void SetState(EnemyState state)
    {
        if (_enemyState == state) return;

        _idleTime = 0f;
        _aggroTime = 0f;
        switch (state)
        {
            case EnemyState.StandingStill:
                _moveInput = Vector2.zero;
                break;
            case EnemyState.Patrolling:
                if (_grounded)
                {
                    CalculatePatrolPoints();
                    _waypoint = _facingLeft ? _patrolPoint1 : _patrolPoint2;
                }
                else
                {
                    SetState(EnemyState.StandingStill);
                    return;
                }
                break;
            case EnemyState.ChasingPlayer:
                _aggroTime = _maxAggroTime;
                _target = _closestPlayer;
                _waypoint = transform.position;
                break;
            case EnemyState.EngagingPlayer:
                _attackTime = _attackDelay;
                _moveInput = Vector2.zero;
                if (_grounded)
                {
                    CalculatePatrolPoints();
                }
                else
                {
                    SetState(EnemyState.ChasingPlayer);
                    return;
                }
                break;
            case EnemyState.EscapingPlayer:
                CheckForNearbyPlayers();
                break;
            case EnemyState.SearchingForWeapon:
                _target = null;
                Collider2D[] holdableColliders = Physics2D.OverlapCircleAll(transform.position, 10f, _holdableLayer);
                foreach (Collider2D holdableCollider in holdableColliders)
                {
                    Holdable holdable = holdableCollider.GetComponent<Holdable>();
                    if (!_holdingSomething && !holdable.BeingHeld && holdable.GetType() != typeof(Helmet))
                    {
                        _target = holdable.transform;
                        _waypoint = transform.position;
                        _targetHoldable = holdable;
                    }
                }
                if (_target == null)
                {
                    SetState(EnemyState.StandingStill);
                    return;
                }
                break;
            case EnemyState.ThrowingBarrels:
                if (_barrelDirectionIsToTheLeft)
                    TurnToDirection(-1f);
                break;
        }
        _enemyState = state;
    }

    void Update()
    {
        Debug.DrawLine(_patrolPoint1, _patrolPoint2, Color.green);
        _idleTime += Time.deltaTime;
        if (_isDead)
        {
            if (transform.position.y < Camera.main.transform.position.y - 50f)
            {
                Destroy(gameObject);
            }
        }
    }

    void CheckForNearbyPlayers()
    {
        Collider2D[] nearbyEntities = Physics2D.OverlapBoxAll((Vector2)transform.position + Vector2.right * (_aggroRadius / 2 * (_facingLeft ? -1f : 1f)), new Vector2(_aggroRadius, 4), 0, _entityLayer);
        int nearbyPlayerCount = 0;
        for (int i = 0; i < nearbyEntities.Length; i++)
        {
            if (nearbyEntities[i].CompareTag("Player")) nearbyPlayerCount++;
        }
        _nearbyPlayers = new Collider2D[nearbyPlayerCount];
        nearbyPlayerCount = 0;
        for (int i = 0; i < nearbyEntities.Length; i++)
        {
            if (nearbyEntities[i].CompareTag("Player"))
            {
                _nearbyPlayers[nearbyPlayerCount] = nearbyEntities[i];
                if (_closestPlayer == null || Vector2.Distance(transform.position, _closestPlayer.position) > Vector2.Distance(transform.position, nearbyEntities[i].transform.position))
                    _closestPlayer = nearbyEntities[i].transform;
                nearbyPlayerCount++;
            }
        }
        
    }

    void CalculatePatrolPoints()
    {
        if (_preDefinedPatrolPoint1 != null &&
            (((Vector2)transform.position - (Vector2)_preDefinedPatrolPoint1.position).sqrMagnitude < 4f ||
            ((Vector2)transform.position - (Vector2)_preDefinedPatrolPoint2.position).sqrMagnitude < 4f)
        )
        {
            _patrolPoint1 = _preDefinedPatrolPoint1.position;
            _patrolPoint2 = _preDefinedPatrolPoint2.position;
        }
        else
        {
            _patrolPoint1 = FindEndOfPlatformTargetOrLadder(ToLeft, true, false);
            _patrolPoint2 = FindEndOfPlatformTargetOrLadder(ToRight, true, false);
        }
    }

    //[SerializeField] GameObject testteastet;
    Vector2 FindEndOfPlatformTargetOrLadder(bool direction, bool ignoreLadders = false, bool stopAtTarget = true)
    {
        float castDirection = 0.05f * (direction ? -1f : 1f);
        Vector2 target = (Vector2)transform.position;
        Vector2 slopeNormal = _slopeNormal;
        Vector2 oldSlopeNormal = slopeNormal;
        Vector2 slopeNormalPerpendicular = _slopeNormalPerpendicular;
        for (int i = 0; i < 1000; i++)
        {
            target += slopeNormalPerpendicular * castDirection;
            RaycastHit2D raycast = Physics2D.Raycast(target, slopeNormal, -_collider.bounds.extents.y - 0.3f, _groundLayer);

            // Set the waypoint to be the target if it's close
            if (stopAtTarget && ((Vector2)_target.position - target).sqrMagnitude < 3f)
            {
                return (Vector2)_target.position;
            }

            // Check for ladders if appropriate
            if (!ignoreLadders && _canUseLadders && _target != null)
            {
                RaycastHit2D ladderRaycast = Physics2D.Raycast(target, Vector2.down, _collider.bounds.extents.y + 0.5f, _ladderLayer);
                if (ladderRaycast)
                {
                    if (!_newLadderFound || Mathf.Abs(ladderRaycast.point.x - _target.position.x) < Mathf.Abs(_potentialLadderPos.x - _target.position.x))
                    {
                        float ladderPosition = (int)(ladderRaycast.point.x) + 0.5f;

                        if (!Physics2D.OverlapPoint(new Vector2(ladderPosition, ladderRaycast.point.y), _ladderLayer))
                        {
                            ladderPosition -= 1;
                            if (!Physics2D.OverlapPoint(new Vector2(ladderPosition, ladderRaycast.point.y), _ladderLayer))
                            {
                                ladderPosition += 2;
                                if (!Physics2D.OverlapPoint(new Vector2(ladderPosition, ladderRaycast.point.y), _ladderLayer))
                                {
                                    ladderPosition -= 3;
                                }   
                            }    
                        }
                        Vector2? ladderTop = Ladder.GetLadderTop(_collider, ladderPosition, target.y - transform.position.y);
                        Vector2? ladderBottom = Ladder.GetLadderBottom(_collider, ladderPosition, target.y - transform.position.y);
                        if (ladderTop != null && ladderBottom != null)
                        {
                            float ladderTopYCoord = ((Vector2)ladderTop).y;
                            float ladderBottomYCoord = ((Vector2)ladderBottom).y;
                            bool targetIsBelow = target.y > _target.position.y;
                            if ((targetIsBelow && ladderBottomYCoord < target.y && Mathf.Abs(ladderBottomYCoord - target.y) > 0.5f) ||
                                (!targetIsBelow && ladderTopYCoord > target.y))
                            {
                                _newLadderFound = true;
                                _ladderDirection = direction;
                                _potentialLadderPos = ladderRaycast.point;
                                _ladderTarget = new Vector2(ladderPosition, target.y);
                            }
                        }
                    }
                }
            }


            if (!raycast || Physics2D.OverlapBox(target, _collider.bounds.extents * 0.8f, 0, _groundLayer))
            {
                i = 1000;
            }
            else
            {
                oldSlopeNormal = slopeNormal;
                slopeNormal = Physics2D.Raycast(target, Vector2.down, _collider.bounds.extents.y + 0.5f, _groundLayer).normal.normalized;
                slopeNormalPerpendicular = Vector2.Perpendicular(slopeNormal);
            }
            

            if (i == 999)
            {
                Debug.LogError($"{gameObject.name} could not find the end of the platform!");
                return direction ? Vector2.left : Vector2.right;
            }
        }
        slopeNormalPerpendicular = Vector2.Perpendicular(oldSlopeNormal);
        target += slopeNormalPerpendicular * (0.4f + _collider.bounds.extents.x) * (direction ? 1f : -1f);

        //if (_newLadderFound)
        //    target = ladderTarget;
        return target;
    }

    void PathFindToTarget()
    {
        _moveInput = Vector2.zero;

        // Can't pathfind to target because it doesn't exist anymore
        if (_target == null)
        {
            SetState(EnemyState.StandingStill);
            return;
        }

        Debug.DrawLine(transform.position, _waypoint, Color.red);

        // Move to the waypoint
        if (_waypoint != Vector2.zero && !NearToPoint(_waypoint) && !(((Vector2)_target.position - _targetOldPosition).sqrMagnitude > 0.2f && _waypoint == _targetOldPosition)) //&& !(((Vector2)_target.position - _targetOldPosition).sqrMagnitude > 0.2f && _waypoint == _targetOldPosition)
        {
            if ((_waypoint - (Vector2)transform.position).sqrMagnitude > 0.04f)
                _moveInput = (_waypoint - (Vector2)transform.position).normalized;
        }

        // Find a new waypoint
        else
        {
            if (_nextToLadder && _waypoint == _ladderTarget && !_climbingLadder)
            {
                _newLadderFound = false;
                _moveInput = Vector2.up;
                Vector2? ladderEnd = _target.position.y > transform.position.y ? Ladder.GetLadderTop(_collider, _ladderXCoord) : Ladder.GetLadderBottom(_collider, _ladderXCoord);
                _waypoint = (Vector2)ladderEnd;
            }
            else
            {
                Vector2 oldWaypoint = _waypoint;
                //_moveInput = Vector2.right;
                _newLadderFound = false;
                Vector2 leftWaypoint = FindEndOfPlatformTargetOrLadder(ToLeft);
                Vector2 rightWaypoint = FindEndOfPlatformTargetOrLadder(ToRight);
                if (leftWaypoint == (Vector2)_target.position)
                {
                    _newLadderFound = false;
                    _waypoint = leftWaypoint;
                }
                else if (rightWaypoint == (Vector2)_target.position)
                {
                    _newLadderFound = false;
                    _waypoint = rightWaypoint;
                }
                else if (_newLadderFound)
                {
                    //_waypoint = _ladderDirection ? rightWaypoint : leftWaypoint;
                    _waypoint = _ladderTarget;
                }
                else if (Mathf.Abs(leftWaypoint.x - _target.position.x) < Mathf.Abs(rightWaypoint.x - _target.position.x))
                {
                    _waypoint = leftWaypoint;
                }
                else
                {
                    _waypoint = rightWaypoint;
                }

                // Hacky solution to prevent the enemy from getting stuck on ladders
                if (_waypoint == oldWaypoint && _climbingLadder)
                {
                    _moveInput = Vector2.right;
                }
            }
            _targetOldPosition = _target.position;
        }
    }

    bool CheckForWeaponsNearby()
    {
        bool weaponsNearby = false;
        Collider2D[] holdableColliders = Physics2D.OverlapCircleAll(transform.position, 10f, _holdableLayer);
        foreach (Collider2D holdableCollider in holdableColliders)
        {
            Holdable holdable = holdableCollider.GetComponent<Holdable>();
            if (!_holdingSomething && !holdable.BeingHeld && holdable.GetType() != typeof(Helmet))
            {
                weaponsNearby = true;
            }
        }
        return weaponsNearby;
    }

    bool WeaponCheck()
    {
        if (!_canSearchForWeapons)
            return false;
        if (_holdingSomething)
            return false;
        if ((int)(Time.time * 10) % 8 != 0)
            return false;
        if (!CheckForWeaponsNearby())
            return false;
        return true;
        
    }

    bool NearToPoint(Vector2 point, float nearness = 0.08f)
    {
        return ((Vector2)transform.position - point).sqrMagnitude < nearness;
    }

    void TurnToDirection(float direction)
    {
        StartCoroutine(IETurnToDirection(direction));
    }

    IEnumerator IETurnToDirection(float direction)
    {
        _moveInput = Vector2.right * direction;
        yield return new WaitForSeconds(0.01f);
        _moveInput = Vector2.zero;
    }

    public override void Damaged(float iFrames)
    {
        base.Damaged(iFrames);
    }
    public override void Die()
    {
        _isInControl = false;
        _useRigidbodyNormally = true;
        _collider.isTrigger = true;
        _rigidBody.gravityScale = 3f;
        _rigidBody.angularDrag = 1f;
        _rigidBody.mass = 1f;
        _rigidBody.drag = 1f;
        _rigidBody.constraints = RigidbodyConstraints2D.None;
        float direction = (_facingLeft && _enemyState == EnemyState.EngagingPlayer) || (!_facingLeft && _enemyState != EnemyState.EngagingPlayer) ? 1 : -1;
        _facingLeft = direction > 0;
        _rigidBody.AddTorque(-direction * 50f);
        _rigidBody.AddForce(new Vector2(direction, 3f) * 6f, ForceMode2D.Impulse);
    }
}



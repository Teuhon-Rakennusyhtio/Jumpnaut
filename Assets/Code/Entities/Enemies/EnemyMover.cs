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
    List<Vector2> _itemPositionBlacklist;
    List<Vector2> _waypoints;
    int _currentWaypointIndex = 0;
    bool _targetReached = true;

    void Start()
    {
        _itemPositionBlacklist = new List<Vector2>();
        _waypoints = new List<Vector2>();
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
        

        switch (_enemyState)
        {
            case EnemyState.StandingStill:
                if (_patrolsWhenIdle && _idleTime > 0.2f)
                    SetState(EnemyState.Patrolling);
                WeaponCheck();
                PlayerCheck();
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
                WeaponCheck();
                PlayerCheck();
                break;


            case EnemyState.ChasingPlayer:
                _aggroTime -= Time.deltaTime;
                if (_aggroTime <= 0f && !_climbingLadder)
                {
                    SetState(EnemyState.StandingStill);
                }
                else
                {
                    //PathFindToTarget();
                    FollowWaypoints();
                    /*if (((Vector2)_target.position - _waypoints[^1]).sqrMagnitude > 0.3f)
                    {
                        CreatePathToTarget(_target);
                    }*/
                    WeaponCheck();
                    PlayerCheck();
                }
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
                bool aimedAtAPlayer = false;
                RaycastHit2D[] entitiesHit = Physics2D.RaycastAll(transform.position, _slopeNormalPerpendicular, _aggroRadius, _entityLayer);
                foreach (RaycastHit2D entityHit in entitiesHit)
                {
                    if (entityHit.collider.CompareTag("Player"))
                        aimedAtAPlayer = true;
                }
                if (NearToPoint(fightPosition, 0.2f) && _attackTime <= 0f)
                {
                    _attackTime = _attackDelay;
                    if (_holdingSomething && _heldItem != null)
                    {
                        if (preferMelee && _canUseWeapons && _holdingWeapon)
                        {
                            _useInput = true;
                        }
                        else if (_canThrowWeapons && aimedAtAPlayer)
                        {
                            _catchInput = true;
                        }
                    }
                    else
                    {
                        // TODO: Funny animation
                    }
                }
                else if ((!_canUseWeapons || !_holdingWeapon) && _attackTime <= 0f && _canThrowWeapons && aimedAtAPlayer)
                {
                    _catchInput = true;
                }
                _movement += Vector2.right * (fightPosition - (Vector2)transform.position).normalized.x * _speed * 0.9f;
                if (_nearbyPlayers.Length == 0)
                {
                    SetState(EnemyState.ChasingPlayer);
                }
                WeaponCheck();
                PlayerCheck();
                break;


            case EnemyState.EscapingPlayer:
                _moveInput = (Vector2.left * (_closestPlayer.position - transform.position).x).normalized * 1f;
                if (((Vector2)(_closestPlayer.position - transform.position)).sqrMagnitude > 300f)
                {
                    SetState(EnemyState.StandingStill);
                }
                WeaponCheck();
                PlayerCheck();
                break;


            case EnemyState.SearchingForWeapon:
                /*if (_targetHoldable.BeingHeld)
                    _target = null;
                PathFindToTarget();
                if (_target != null && NearToPoint((Vector2)_target.position, 0.5f))
                {
                    _catchInput = true;
                }
                if (_holdingSomething)
                {
                    SetState(EnemyState.StandingStill);
                }*/
                if (_holdingSomething)
                {
                    SetState(EnemyState.StandingStill);
                }
                else if (_targetReached)
                {
                    _catchInput = true;
                }
                FollowWaypoints();
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
                CreatePathToTarget(_target);
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
                _itemPositionBlacklist.Clear();
                break;
            case EnemyState.SearchingForWeapon:
                /*_target = null;
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
                }*/
                break;
            case EnemyState.ThrowingBarrels:
                if (_barrelDirectionIsToTheLeft)
                    TurnToDirection(-1f);
                break;
        }
        _enemyState = state;
        Debug.Log(state);
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

    void CheckForPlayersNearby()
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

    void PlayerCheck()
    {
        if ((int)(Time.time * 10) % 9 != 0) return;
        CheckForPlayersNearby();
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
            _patrolPoint1 = FindEndOfPlatformTargetOrLadder(transform.position, ToLeft, true, false);
            _patrolPoint2 = FindEndOfPlatformTargetOrLadder(transform.position, ToRight, true, false);
        }
    }

    //[SerializeField] GameObject testteastet;
    Vector2 FindEndOfPlatformTargetOrLadder(Vector2 startPosition, bool direction, bool ignoreLadders = false, bool stopAtTarget = true, Transform targetPosition = null)
    {
        if (targetPosition == null) targetPosition = _target;
        float castDirection = 0.05f * (direction ? -1f : 1f);
        Vector2 target = startPosition;
        Vector2 slopeNormal = _slopeNormal;
        Vector2 oldSlopeNormal = slopeNormal;
        Vector2 slopeNormalPerpendicular = _slopeNormalPerpendicular;
        for (int i = 0; i < 1000; i++)
        {
            target += slopeNormalPerpendicular * castDirection;
            RaycastHit2D raycast = Physics2D.Raycast(target, slopeNormal, -_collider.bounds.extents.y - 0.3f, _groundLayer);

            // Set the waypoint to be the target if it's close
            if (stopAtTarget && ((Vector2)targetPosition.position - target).sqrMagnitude < 3f)
            {
                return (Vector2)targetPosition.position;
            }

            // Check for ladders if appropriate
            if (!ignoreLadders && _canUseLadders && targetPosition != null)
            {
                RaycastHit2D ladderRaycast = Physics2D.Raycast(target, Vector2.down, _collider.bounds.extents.y + 0.5f, _ladderLayer);
                if (ladderRaycast)
                {
                    if (!_newLadderFound || Mathf.Abs(ladderRaycast.point.x - targetPosition.position.x) < Mathf.Abs(_potentialLadderPos.x - targetPosition.position.x))
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
                            bool targetIsBelow = target.y > targetPosition.position.y;
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
                Vector2 leftWaypoint = FindEndOfPlatformTargetOrLadder(transform.position, ToLeft);
                Vector2 rightWaypoint = FindEndOfPlatformTargetOrLadder(transform.position, ToRight);
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

    void CheckForWeaponsNearby()
    {
        Collider2D[] holdableColliders = Physics2D.OverlapCircleAll(transform.position, 10f, _holdableLayer);
        foreach (Collider2D holdableCollider in holdableColliders)
        {
            Holdable holdable = holdableCollider.GetComponent<Holdable>();
            if (!_holdingSomething && !holdable.BeingHeld && !holdable.Thrown && holdable.GetType() != typeof(Helmet) && !_itemPositionBlacklist.Contains((Vector2)holdable.transform.position))
            {
                _targetHoldable = holdable;
                if (CreatePathToTarget(_targetHoldable.transform))
                {
                    SetState(EnemyState.SearchingForWeapon);
                    return;
                }
                else
                {
                    _itemPositionBlacklist.Add(_targetHoldable.transform.position);
                }
            }
        }

    }

    bool CreatePathToTarget(Transform target, bool hasToGetToTarget = true)
    {
        Vector2 pathPoint = transform.position;
        List<Vector2> waypoints = new()
        {
            pathPoint
        };
        bool foundEndOfPath = false;
        if (_climbingLadder)
            _ladderTarget = pathPoint;
        for (int i = 0; i < 10; i++)
        {
            if (foundEndOfPath) break;
            
            _newLadderFound = false;
            if (waypoints.Count > 0)
                pathPoint = waypoints[^1];
            if (waypoints.Count > 0 && waypoints[^1] == _ladderTarget)
            {
                Vector2? ladderEnd = target.position.y > pathPoint.y ? Ladder.GetLadderTop(_collider, _ladderTarget.x, pathPoint.y - transform.position.y) : Ladder.GetLadderBottom(_collider, _ladderTarget.x, pathPoint.y - transform.position.y);
                if (ladderEnd != null)
                    waypoints.Add((Vector2)ladderEnd);
            }
            else
            {    
                Vector2 newWaypoint = Vector2.zero;
                Vector2 leftWaypoint = FindEndOfPlatformTargetOrLadder(pathPoint, ToLeft, false, true, target);
                Vector2 rightWaypoint = FindEndOfPlatformTargetOrLadder(pathPoint, ToRight, false, true, target);
                if (leftWaypoint == (Vector2)target.position)
                {
                    _newLadderFound = false;
                    newWaypoint = leftWaypoint;
                }
                else if (rightWaypoint == (Vector2)target.position)
                {
                    _newLadderFound = false;
                    newWaypoint = rightWaypoint;
                }
                else if (_newLadderFound)
                {
                    newWaypoint = _ladderTarget;
                }
                else if (Mathf.Abs(leftWaypoint.x - target.position.x) < Mathf.Abs(rightWaypoint.x - target.position.x))
                {
                    newWaypoint = leftWaypoint;
                }
                else
                {
                    Debug.Log("stupid");
                    newWaypoint = rightWaypoint;
                }

                Debug.Log(newWaypoint + " " + target.position + " " + _newLadderFound);

                if (newWaypoint == (Vector2)target.position)
                {
                    waypoints.Add(newWaypoint);
                    foundEndOfPath = true;
                    _waypoints.Clear();
                    _waypoints = waypoints;
                    _currentWaypointIndex = 0;
                    _targetReached = false;
                }
                else if (waypoints.Count > 0 && newWaypoint == waypoints[^1])
                {
                    Debug.Log($"{gameObject.name} did not find a path to {target.name}...");
                    if (!hasToGetToTarget)
                    {
                        _waypoints.Clear();
                        _waypoints = waypoints;
                        _currentWaypointIndex = 0;
                        _targetReached = false;
                    }
                    return false;
                }
                else
                {
                    waypoints.Add(newWaypoint);
                }
            }
            if (i == 9)
            {
                if (!hasToGetToTarget)
                {
                    _waypoints.Clear();
                    _waypoints = waypoints;
                    _currentWaypointIndex = 0;
                    _targetReached = false;
                }
                return false;
            }
        }
        return true;
    }

    void FollowWaypoints()
    {
        //Debug.Log($"{_currentWaypointIndex} {_waypoints.Count} {gameObject.name}");
        if (!_targetReached)
            _moveInput = (_waypoints[_currentWaypointIndex + 1] - _waypoints[_currentWaypointIndex]).normalized;
        else
            _moveInput = Vector2.zero;
        if (((Vector2)transform.position - _waypoints[_currentWaypointIndex + 1]).sqrMagnitude < 0.1f && !_targetReached)
        {
            if (_currentWaypointIndex == _waypoints.Count - 2)
                _targetReached = true;
            else
                _currentWaypointIndex++;
        }
        for (int i = 0; i < _waypoints.Count - 1; i++)
        {
            Debug.DrawLine(_waypoints[i], _waypoints[i + 1], Color.magenta);
        }
    }

    void WeaponCheck()
    {
        if (!_canSearchForWeapons)
            return;
        if (_holdingSomething)
            return;
        if ((int)(Time.time * 10) % 8 != 0)
            return;
        CheckForWeaponsNearby();
        
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
        _AudioManager?.PlaySFX(_AudioManager.enemyDeath);
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



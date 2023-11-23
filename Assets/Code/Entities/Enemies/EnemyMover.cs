using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    bool _hasNotMovedThisPhysicsUpdate = false;
    Vector3 _oldPosition;
    Vector2 _approximatePlayerOldPosition;

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


        if (!_isInControl) return;
        _catchInput = false;
        _useInput = false;
        _strafeMovement = Vector2.zero;
        
        switch (_enemyState)
        {
            case EnemyState.StandingStill:
                StandingStill();
                break;
            case EnemyState.Patrolling:
                Patrolling();
                break;
            case EnemyState.ChasingPlayer:
                ChasingPlayer();
                break;
            case EnemyState.EngagingPlayer:
                EngagingPlayer();
                break;
            case EnemyState.EscapingPlayer:
                EscapingPlayer();
                break;
            case EnemyState.SearchingForWeapon:
                SearchingForWeapon();
                break;
            case EnemyState.ThrowingBarrels:
                ThrowingBarrels();
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
                    _hasNotMovedThisPhysicsUpdate = false;
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
                BetterCreatePathToTarget(_target.position);
                _approximatePlayerOldPosition = RoundVector(_target.position);
                _targetReached = false;
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
                // Path is created in the weapon check method
                _targetReached = false;
                break;
            case EnemyState.ThrowingBarrels:
                if (_barrelDirectionIsToTheLeft)
                    TurnToDirection(-1f);
                break;
        }
        _enemyState = state;
        //Debug.Log(state);
    }

    void StandingStill()
    {
        if (_patrolsWhenIdle && _idleTime > 0.2f)
            SetState(EnemyState.Patrolling);
        WeaponCheck();
        PlayerCheck();
    }

    void Patrolling()
    {
        if (((Vector2)transform.position - _waypoint).sqrMagnitude < 0.5f)
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
        else if (_hasNotMovedThisPhysicsUpdate)
        {
            _hasNotMovedThisPhysicsUpdate = false;
            CalculatePatrolPoints();
            _waypoint = _patrolPoint1;
        }

        _moveInput = (Vector2.right * (_waypoint - (Vector2)transform.position).x).normalized * 0.5f;
        WeaponCheck();
        PlayerCheck();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        _hasNotMovedThisPhysicsUpdate = _oldPosition == transform.position;
        _oldPosition = transform.position;
    }

    void ChasingPlayer()
    {
        _aggroTime -= Time.deltaTime;
        if (_aggroTime <= 0f && !_climbingLadder)
        {
            SetState(EnemyState.StandingStill);
        }
        else
        {
            if (RoundVector(_target.position) != _approximatePlayerOldPosition)
            {
                _approximatePlayerOldPosition = RoundVector(_target.position);
                BetterCreatePathToTarget(_target.position);
                _targetReached = false;
            }
            //PathFindToTarget();
            FollowWaypoints();
            /*if (((Vector2)_target.position - _waypoints[^1]).sqrMagnitude > 0.3f)
            {
                CreatePathToTarget(_target);
            }*/
            WeaponCheck();
            PlayerCheck();
        }
    }

    // TODO: Make this method not suck so much
    void EngagingPlayer()
    {
        /*
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
        _strafeMovement = Vector2.left * (fightPosition - (Vector2)transform.position).normalized.x * _speed * 0.9f;
        */
        if (_nearbyPlayers.Length == 0)
        {
            SetState(EnemyState.ChasingPlayer);
        }
        WeaponCheck();
        PlayerCheck();
    }

    void EscapingPlayer()
    {
        _moveInput = (Vector2.left * (_closestPlayer.position - transform.position).x).normalized * 1f;
        if (((Vector2)(_closestPlayer.position - transform.position)).sqrMagnitude > 300f)
        {
            SetState(EnemyState.StandingStill);
        }
        WeaponCheck();
        PlayerCheck();
    }

    void SearchingForWeapon()
    {
        if (_holdingSomething)
        {
            SetState(EnemyState.StandingStill);
        }
        else if (_targetReached)
        {
            _catchInput = true;
        }
        FollowWaypoints();
    }

    void ThrowingBarrels()
    {
        if (_idleTime > _attackDelay)
        {
            _idleTime = 0f;
            GameObject newBarrel = Instantiate(_barrel, transform.position - Vector3.up * _collider.bounds.extents.y / 2, Quaternion.identity);
            newBarrel.GetComponent<Barrel>().Push(_barrelDirectionIsToTheLeft ? -1f : 1f);
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
        FindPatrolPoints(ref _patrolPoint1, ref _patrolPoint2, transform.position);
    }
    void FindPatrolPoints(ref Vector2 point1, ref Vector2 point2, Vector2 castPosition)
    {
        float castDirection = 0.05f;
        //castPosition = transform.position;
        Vector2 startPositionSlopeNormal = Physics2D.Raycast(castPosition, Vector2.down, _collider.bounds.extents.y + 0.5f, _groundLayer).normal.normalized;
        Vector2 slopeNormal = startPositionSlopeNormal;
        Vector2 oldSlopeNormal = slopeNormal;
        Vector2 slopeNormalPerpendicular = Vector2.Perpendicular(slopeNormal);
        Vector2 startCastPosition = castPosition;
        for (int i = 0; i < 1000; i++)
        {
            // Move forwards
            castPosition += slopeNormalPerpendicular * castDirection;
            // Check for ground
            RaycastHit2D raycast = Physics2D.Raycast(castPosition, slopeNormal, -_collider.bounds.extents.y - 0.3f, _groundLayer);
            // If there is no ground or we are inside wall, stop here
            if (!raycast || Physics2D.OverlapBox(castPosition, _collider.bounds.extents * 0.9f, 0, _groundLayer))
            {
                i = 1000;
            }
            // Otherwise get the angle of the ground
            else
            {
                oldSlopeNormal = slopeNormal;
                slopeNormal = Physics2D.Raycast(castPosition, Vector2.down, _collider.bounds.extents.y + 0.5f, _groundLayer).normal.normalized;
                slopeNormalPerpendicular = Vector2.Perpendicular(slopeNormal);
            }
        }

        // Go a bit back to not fall
        castDirection *= -1;
        slopeNormal = oldSlopeNormal;
        for (int i = 0; i < 5; i++)
        {
            castPosition += slopeNormalPerpendicular * castDirection;
            slopeNormal = Physics2D.Raycast(castPosition, Vector2.down, _collider.bounds.extents.y + 0.5f, _groundLayer).normal.normalized;
            slopeNormalPerpendicular = Vector2.Perpendicular(slopeNormal);
        }
        point2 = castPosition;

        castPosition = startCastPosition;
        slopeNormal = startPositionSlopeNormal;
        slopeNormalPerpendicular = Vector2.Perpendicular(startPositionSlopeNormal);

        for (int i = 0; i < 1000; i++)
        {
            // Move forwards
            castPosition += slopeNormalPerpendicular * castDirection;
            // Check for ground
            RaycastHit2D raycast = Physics2D.Raycast(castPosition, slopeNormal, -_collider.bounds.extents.y - 0.3f, _groundLayer);
            // If there is no ground or we are inside wall, stop here
            if (!raycast || Physics2D.OverlapBox(castPosition, _collider.bounds.extents * 0.9f, 0, _groundLayer))
            {
                i = 1000;
            }
            // Otherwise get the angle of the ground
            else
            {
                oldSlopeNormal = slopeNormal;
                slopeNormal = Physics2D.Raycast(castPosition, Vector2.down, _collider.bounds.extents.y + 0.5f, _groundLayer).normal.normalized;
                slopeNormalPerpendicular = Vector2.Perpendicular(slopeNormal);
            }
        }

        // Go a bit back to not fall
        castDirection *= -1;
        slopeNormal = oldSlopeNormal;
        for (int i = 0; i < 5; i++)
        {
            castPosition += slopeNormalPerpendicular * castDirection;
            slopeNormal = Physics2D.Raycast(castPosition, Vector2.down, _collider.bounds.extents.y + 0.5f, _groundLayer).normal.normalized;
            slopeNormalPerpendicular = Vector2.Perpendicular(slopeNormal);
        }
        point1 = castPosition;
    }
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

    void CheckForWeaponsNearby()
    {
        Collider2D[] holdableColliders = Physics2D.OverlapCircleAll(transform.position, 10f, _holdableLayer);
        foreach (Collider2D holdableCollider in holdableColliders)
        {
            Holdable holdable = holdableCollider.GetComponent<Holdable>();
            if (!_holdingSomething && !holdable.BeingHeld && !holdable.Thrown && holdable.GetType() != typeof(Helmet) && !_itemPositionBlacklist.Contains(RoundVector(holdable.transform.position)))
            {
                _targetHoldable = holdable;
                if (BetterCreatePathToTarget(_targetHoldable.transform.position))
                {
                    SetState(EnemyState.SearchingForWeapon);
                    return;
                }
                else
                {
                    _itemPositionBlacklist.Add(RoundVector(_targetHoldable.transform.position));
                }
            }
        }

    }

    bool BetterCreatePathToTarget(Vector3 target)
    {
        _currentWaypointIndex = 0;
        Vector2 pathPoint = transform.position;
        List<Vector2> waypoints = new()
        {
            pathPoint
        };
        List<Vector2> ladderPoints = new();

        if (_climbingLadder)
        {
            // If we are climbing a ladder then get to the end of it
            if (target.y > pathPoint.y)
            {
                Vector2? nullableLadderTop = Ladder.GetLadderTop(_collider, transform.position.x);
                if (nullableLadderTop != null)
                {
                    waypoints.Add((Vector2)nullableLadderTop);
                }
            }
            else
            {
                Vector2? nullableLadderBottom = Ladder.GetLadderBottom(_collider, transform.position.x);
                if (nullableLadderBottom != null)
                {
                    waypoints.Add((Vector2)nullableLadderBottom);
                }
            }
        }

        for (int i = 0; i < 10; i++)
        {
            ladderPoints.Clear();
            // Create patrol points and from the first one, that is the left one, check for ladders along the route
            Vector2 startPoint = Vector2.zero;
            Vector2 endPoint = Vector2.zero;
            FindPatrolPoints(ref startPoint, ref endPoint, waypoints[^1]);
            Vector2 closestPoint = startPoint;


            float castDirection = 0.05f;
            Vector2 castPosition = startPoint;
            Vector2 slopeNormal = Physics2D.Raycast(castPosition, Vector2.down, _collider.bounds.extents.y + 0.5f, _groundLayer).normal.normalized;
            Vector2 slopeNormalPerpendicular = Vector2.Perpendicular(slopeNormal);
            for (int j = 0; j < 2000; j++)
            {
                // Move forwards
                castPosition += slopeNormalPerpendicular * castDirection;

                // Check for target
                if ((castPosition - (Vector2)target).sqrMagnitude < 0.5f)
                {
                    waypoints.Add(target);
                    _waypoints = waypoints;
                    return true;
                }

                // Check for ladders if not holding a heavy object
                if (!_holdingHeavyObject && Physics2D.OverlapBox(castPosition, _collider.bounds.extents * 1.3f, 0, _ladderLayer))
                {
                    // Middle of the ladder x position
                    float ladderXPositionPlus = (int)castPosition.x + 0.5f;
                    float ladderXPositionMinus = (int)castPosition.x - 0.5f;
                    float ladderXPosition = Mathf.Abs(ladderXPositionPlus - castPosition.x) < Mathf.Abs(ladderXPositionMinus - castPosition.x) ? ladderXPositionPlus : ladderXPositionMinus;

                    // Middle of the ladder y position
                    float ladderYPositionPlus = (int)castPosition.y + 0.5f;
                    float ladderYPositionMinus = (int)castPosition.y - 0.5f;
                    float ladderYPosition = Mathf.Abs(ladderYPositionPlus - castPosition.y) < Mathf.Abs(ladderXPositionMinus - castPosition.y) ? ladderYPositionPlus : ladderYPositionMinus;
                    ladderYPosition = Physics2D.Raycast(castPosition + Vector2.up * 0.5f, Vector2.down, _collider.bounds.extents.y + 2f, _groundLayer).point.y + _collider.bounds.extents.y;

                    Vector2 ladderPosition = new(ladderXPosition, ladderYPosition);
                    if (!ladderPoints.Contains(ladderPosition))
                    {
                        ladderPoints.Add(ladderPosition);
                    }

                }

                // Check if we are closer to the target than we were previously
                if ((castPosition - (Vector2)target).sqrMagnitude < (closestPoint - (Vector2)target).sqrMagnitude)
                {
                    closestPoint = castPosition;
                }

                // Get the correct slope normal
                slopeNormal = Physics2D.Raycast(castPosition, Vector2.down, _collider.bounds.extents.y + 0.5f, _groundLayer).normal.normalized;
                slopeNormalPerpendicular = Vector2.Perpendicular(slopeNormal);

                // We are at the end and should stop now
                if ((castPosition - endPoint).sqrMagnitude < 0.5f)
                {
                    j = 2000;
                }
            }

            Vector2 ladderPoint = Vector2.zero;
            Vector2 ladderEnd = Vector2.zero;
            Vector2 bestLadderEnd = Vector2.zero;

            // If the target is above this point check if the ladder goes up
            if (target.y > castPosition.y)
            {
                bestLadderEnd = Vector2.negativeInfinity;

                foreach (Vector2 potentialLadderPoint in ladderPoints)
                {
                    Vector2? nullableLadderTop = Ladder.GetLadderTop(_collider, potentialLadderPoint.x, potentialLadderPoint.y - transform.position.y);
                    if (nullableLadderTop != null && ((Vector2)nullableLadderTop + Vector2.zero).y > castPosition.y + 1f)
                    {
                        ladderEnd = (Vector2)nullableLadderTop;
                        if (((Vector2)target - bestLadderEnd).sqrMagnitude > ((Vector2)target - ladderEnd).sqrMagnitude)
                        {
                            ladderPoint = potentialLadderPoint;
                            bestLadderEnd = ladderEnd;
                        }
                    }
                }
            }
            else
            {
                bestLadderEnd = Vector2.positiveInfinity;

                foreach (Vector2 potentialLadderPoint in ladderPoints)
                {
                    Vector2? nullableLadderBottom = Ladder.GetLadderBottom(_collider, potentialLadderPoint.x, potentialLadderPoint.y - transform.position.y);
                    if (nullableLadderBottom != null && ((Vector2)nullableLadderBottom + Vector2.zero).y < castPosition.y - 1f)
                    {
                        ladderEnd = (Vector2)nullableLadderBottom;
                        if (((Vector2)target - bestLadderEnd).sqrMagnitude > ((Vector2)target - ladderEnd).sqrMagnitude)
                        {
                            ladderPoint = potentialLadderPoint;
                            bestLadderEnd = ladderEnd;
                        }
                    }
                }
            }

            // If there were no good ladders and we have not already returned from this method because we found the target, the closest point will have to do
            if (ladderPoint == Vector2.zero)
            {
                waypoints.Add(closestPoint);
                _waypoints = waypoints;
                return false;
            }
            else
            {
                waypoints.Add(ladderPoint);
                waypoints.Add(ladderEnd);
            }

        }
        return false;
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
        if (_waypoints.Count < 2) return;
        if (!_targetReached)
            _moveInput = (_waypoints[_currentWaypointIndex + 1] - _waypoints[_currentWaypointIndex]).normalized;
        else
            _moveInput = Vector2.zero;
        if (((Vector2)transform.position - _waypoints[_currentWaypointIndex + 1]).sqrMagnitude < 0.05f && !_targetReached)
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
    Vector2 RoundVector(Vector2 vector)
    {
        return new Vector2((int)vector.x, (int)vector.y);
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
        _isDead = true;
    }
}



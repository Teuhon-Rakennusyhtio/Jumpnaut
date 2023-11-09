using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] LayerMask _ladderLayer;
    [SerializeField] bool _patrolsWhenIdle = true;
    [SerializeField] bool _targetsPlayers = true;
    [SerializeField] bool _canUseLadders = true;
    [SerializeField] bool _canJump = true;
    [SerializeField] bool _canUseWeapons = true;
    [SerializeField] bool _canThrowWeapons = true;
    [SerializeField] bool _canCatchWeapons = true;
    [SerializeField] bool _canSearchForWeapons = true;
    [SerializeField] bool _runsAwayWhenCloseToPlayerWhenWithoutWeapon = true;
    [SerializeField] bool _canThrowBarrels = false;
    [SerializeField] float _maxAggroTime = 5f;
    [SerializeField] float _aggroRadius = 3f;
    [SerializeField] Transform _preDefinedPatrolPoint1;
    [SerializeField] Transform _preDefinedPatrolPoint2;
    Vector2 _patrolPoint1, _patrolPoint2;
    bool _newLadderFound = false, _ladderDirection;
    Vector2 _potentialLadderPos;
    Vector2 _waypoint;
    Transform _target;
    Vector2 _targetOldPosition;
    float _aggroTime, _idleTime;
    Holdable _targetHoldable;
    EnemyState _enemyState = EnemyState.StandingStill;

    /*void Start()
    {
        StartCoroutine(Tester());
    }


    IEnumerator Tester()
    {
        yield return new WaitForSeconds(2);
        //Debug.Log(FindEndOfPlatformOrLadder(ToLeft));
        SetState(EnemyState.Patrolling);
    }*/

    protected override void GetInputs()
    {
        //_jumpInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.jump];
        //_useInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.use];
        //_catchInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pickup];
        //_pauseInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pause];
        _catchInput = false;
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
                break;
            case EnemyState.EngagingPlayer:
                break;
            case EnemyState.EscapingPlayer:
                break;
            case EnemyState.SearchingForWeapon:
                if (_targetHoldable.BeingHeld)
                    _target = null;
                PathFindToTarget();
                if (_target != null && NearToPoint((Vector2)_target.position, 0.5f))
                {
                    _catchInput = true;
                }
                if (_heldItem != null)
                {
                    SetState(EnemyState.StandingStill);
                }
                break;
            case EnemyState.ThrowingBarrels:
                break;
        }

        //_catchInput = true;
        //_moveInput = Vector2.right * Random.Range(-1f, 1f);

        
        //Debug.Log((_waypoint - (Vector2)transform.position).x);
    }

    void SetState(EnemyState state)
    {
        _idleTime = 0f;
        switch (state)
        {
            case EnemyState.StandingStill:
                _moveInput = Vector2.zero;
                break;
            case EnemyState.Patrolling:
                if (_grounded)
                {
                    CalculatePatrolPoints();
                    _waypoint = _patrolPoint1;
                }
                else
                {
                    SetState(EnemyState.StandingStill);
                    return;
                }
                break;
            case EnemyState.ChasingPlayer:
                break;
            case EnemyState.EngagingPlayer:
                break;
            case EnemyState.EscapingPlayer:
                break;
            case EnemyState.SearchingForWeapon:
                _target = null;
                Collider2D[] holdableColliders = Physics2D.OverlapCircleAll(transform.position, 10f, _holdableLayer);
                foreach (Collider2D holdableCollider in holdableColliders)
                {
                    Holdable holdable = holdableCollider.GetComponent<Holdable>();
                    if (holdable != null && !holdable.BeingHeld && holdable.GetType() != typeof(Helmet))
                    {
                        _target = holdable.transform;
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
                break;
        }
        _enemyState = state;
    }

    void Update()
    {
        Debug.DrawLine(_patrolPoint1, _patrolPoint2, Color.red);
        _idleTime += Time.deltaTime;
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

            if (stopAtTarget && ((Vector2)_target.position - target).sqrMagnitude < 0.5f)
            {
                return (Vector2)_target.position;
            }

            if (!ignoreLadders && _target != null)
            {
                RaycastHit2D ladderRaycast = Physics2D.Raycast(target, Vector2.down, _collider.bounds.extents.y + 0.5f, _ladderLayer);
                if (ladderRaycast)
                {
                    if (!_newLadderFound || Mathf.Abs(ladderRaycast.point.x - _target.position.x) < Mathf.Abs(_potentialLadderPos.x - _target.position.x))
                    {
                        float ladderPosition = (int)(ladderRaycast.point.x) + 0.5f;
                        if (!Physics2D.OverlapPoint(new Vector2(ladderPosition, ladderRaycast.point.y)))
                        {
                            ladderPosition -= 1;
                            if (!Physics2D.OverlapPoint(new Vector2(ladderPosition, ladderRaycast.point.y)))
                                ladderPosition += 2;
                        }
                        //Debug.Log(ladderPosition);
                        Vector2? ladderTop = Ladder.GetLadderTop(_collider, ladderPosition);
                        Vector2? ladderBottom = Ladder.GetLadderTop(_collider, ladderPosition);
                        if (ladderTop != null && ladderBottom != null)
                        {
                            float ladderTopYCoord = ((Vector2)ladderTop).y;
                            float ladderBottomYCoord = ((Vector2)ladderBottom).y;
                            bool targetIsBelow = target.y > _target.position.y;
                            if ((targetIsBelow && ladderBottomYCoord < target.y) ||
                                (!targetIsBelow && ladderTopYCoord > target.y))
                            {
                                _newLadderFound = true;
                                _ladderDirection = direction;
                                _potentialLadderPos = ladderRaycast.point;
                            }
                        }
                    }
                }
            }

            if (!raycast || Physics2D.OverlapPoint(target, _groundLayer))
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
        return target;
    }

    void PathFindToTarget()
    {
        // Can't pathfind to target because it doesn't exist anymore
        // Or can't walk to the target
        if (_target == null || !_climbingLadder && _waypoint.y > transform.position.y)
        {
            SetState(EnemyState.StandingStill);
            return;
        }

        Debug.DrawLine(transform.position, _waypoint, Color.red);

        // Move to the waypoint
        if (_waypoint != Vector2.zero && !NearToPoint(_waypoint) && ((Vector2)_target.position - _targetOldPosition).sqrMagnitude < 0.2f)
        {
            if (_newLadderFound && _nextToLadder && Mathf.Abs(_potentialLadderPos.x - transform.position.x) < 0.1f)
            {
                _newLadderFound = false;
                _moveInput = Vector2.up;
                Vector2? ladderEnd = _target.position.y > transform.position.y ? Ladder.GetLadderTop(_collider, _ladderXCoord) : Ladder.GetLadderBottom(_collider, _ladderXCoord);
                _waypoint = (Vector2)ladderEnd;
            }
            else if (!_climbingLadder)
            {
                _moveInput = (Vector2.right * (_waypoint - (Vector2)transform.position).x).normalized;
            }
            else if (_climbingLadder)
            {
                _moveInput = (Vector2.up * (_waypoint - (Vector2)transform.position).y).normalized;
            }
        }

        // Find a new waypoint
        else
        {
            _moveInput = Vector2.right;
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
                _waypoint = _ladderDirection ? rightWaypoint : leftWaypoint;
            }
            else if (Mathf.Abs(leftWaypoint.x - _target.position.x) < Mathf.Abs(rightWaypoint.x - _target.position.x))
            {
                _waypoint = leftWaypoint;
            }
            else
            {
                _waypoint = rightWaypoint;
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
            if (holdable != null && !holdable.BeingHeld && holdable.GetType() != typeof(Helmet))
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
        if (_heldItem != null)
            return false;
        if ((int)(Time.time * 10) % 8 != 0)
            return false;
        if (!CheckForWeaponsNearby())
            return false;
        return true;
        
    }

    bool NearToPoint(Vector2 point, float nearness = 0.01f)
    {
        return ((Vector2)transform.position - point).sqrMagnitude < nearness;
    }
}



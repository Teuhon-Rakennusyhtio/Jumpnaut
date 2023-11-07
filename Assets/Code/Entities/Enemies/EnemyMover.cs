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
    Vector2 _target, _waypoint;
    float _aggroTime;
    EnemyState _enemyState = EnemyState.StandingStill;

    void Start()
    {
        StartCoroutine(Tester());
    }

    IEnumerator Tester()
    {
        yield return new WaitForSeconds(2);
        //Debug.Log(FindEndOfPlatformOrLadder(ToLeft));
        CalculatePatrolPoints();
        _waypoint = _patrolPoint1;
        _enemyState = EnemyState.Patrolling;
    }

    protected override void GetInputs()
    {
        //_jumpInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.jump];
        //_useInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.use];
        //_catchInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pickup];
        //_pauseInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pause];

        switch (_enemyState)
        {
            case EnemyState.StandingStill:
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
                _moveInput = (Vector2.right * (_waypoint - (Vector2)transform.position).x).normalized * 0.7f;
                break;
            case EnemyState.ChasingPlayer:
                break;
            case EnemyState.EngagingPlayer:
                break;
            case EnemyState.EscapingPlayer:
                break;
            case EnemyState.SearchingForWeapon:
                break;
            case EnemyState.ThrowingBarrels:
                break;
        }

        _catchInput = true;
        //_moveInput = Vector2.right * Random.Range(-1f, 1f);

        
        //Debug.Log((_waypoint - (Vector2)transform.position).x);
    }

    void Update()
    {
        
    }

    void CalculatePatrolPoints()
    {
        if (((Vector2)transform.position - _patrolPoint1).sqrMagnitude < 4f ||
            ((Vector2)transform.position - _patrolPoint2).sqrMagnitude < 4f
        )
        {
            _patrolPoint1 = _preDefinedPatrolPoint1.position;
            _patrolPoint2 = _preDefinedPatrolPoint2.position;
        }
        else
        {
            _patrolPoint1 = FindEndOfPlatformOrLadder(ToLeft, true);
            _patrolPoint2 = FindEndOfPlatformOrLadder(ToRight, true);
        }
    }

    //[SerializeField] GameObject testteastet;
    Vector2 FindEndOfPlatformOrLadder(bool direction, bool ignoreLadders = false)
    {
        float castDirection = 0.05f * (direction ? -1f : 1f);
        Vector2 target = (Vector2)transform.position;
        Vector2 slopeNormal = _slopeNormal;
        Vector2 slopeNormalPerpendicular = _slopeNormalPerpendicular;
        for (int i = 0; i < 1000; i++)
        {
            target += slopeNormalPerpendicular * castDirection;
            //Instantiate(testteastet, target, Quaternion.identity);
            RaycastHit2D raycast = Physics2D.Raycast(target, slopeNormal, -_collider.bounds.extents.y - 0.1f, _groundLayer);
            if (!raycast || Physics2D.OverlapPoint(target, _groundLayer))
            {
                i = 1000;
            }
            else
            {
                slopeNormal = raycast.normal.normalized;
                slopeNormalPerpendicular = Vector2.Perpendicular(slopeNormal);
            }

            if (i == 999)
            {
                Debug.LogError($"{gameObject.name} could not find the end of the platform or a ladder!");
                return direction ? Vector2.left : Vector2.right;
            }
        }

        target += slopeNormalPerpendicular * (0.8f + _collider.bounds.extents.x) * (direction ? 1f : -1f);
        //Instantiate(testteastet, target, Quaternion.identity);
        return target;
    }
}



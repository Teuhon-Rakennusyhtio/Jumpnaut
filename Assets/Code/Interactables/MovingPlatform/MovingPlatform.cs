using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovingPlatformPathTypes
{
    GoDirectlyToStartPoint,
    BacktrackToStartPoint
}
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] bool _movesOnItsOwn = false;
    [SerializeField] float _movingOnOwnDelay = 2f;
    [SerializeField] MovingPlatformPathTypes _whenTheLastPointIsReached = MovingPlatformPathTypes.GoDirectlyToStartPoint;
    [SerializeField] bool _waitAtEveryPathPoint = false;
    [SerializeField] Transform[] _pathPoints;
    public float Speed = 5f;
    float _progress, _target;
    float _direction = 0f, _oldDirection = 1f;
    int _currentStartPoint = 0;
    Transform _startPoint, _endPoint, _platfromTransform;
    Rigidbody2D _platfrom;
    Vector2 _pathVector;
    List<Rigidbody2D> _attachedRigidbodies;
    // Start is called before the first frame update
    void Start()
    {
        if (_movesOnItsOwn) _direction = 1f;
        _attachedRigidbodies = new List<Rigidbody2D>();
        _platfromTransform = transform.Find("Platform");
        if (_pathPoints.Length >= 2)
        {
            _startPoint = _pathPoints[0];
            _endPoint = _pathPoints[1];
        }
        else
        {
            Debug.LogError($"The moving platform \"{gameObject.name}\" does not have enough path points function!");
            _startPoint = _platfromTransform;
            _endPoint = _platfromTransform;
        }
        _platfrom = _platfromTransform.gameObject.GetComponent<Rigidbody2D>();
        _pathVector = _endPoint.position - _startPoint.position;
        _pathVector = _pathVector.normalized;
    }

    void GetNewTargetPoints(float direction)
    {
        if (_pathPoints.Length == 0) return;
        if (direction > 0f)
        {
            if (_currentStartPoint + 2 == _pathPoints.Length)
            {
                if (_whenTheLastPointIsReached == MovingPlatformPathTypes.GoDirectlyToStartPoint)
                {
                    _currentStartPoint = 0;
                }
                else
                {
                    _direction = 0f;
                    return;
                }
            }
            else
            {
                _currentStartPoint++;
            }
        }
        else
        {
            if (_currentStartPoint == 0)
            {
                if (_whenTheLastPointIsReached == MovingPlatformPathTypes.GoDirectlyToStartPoint)
                {
                    _currentStartPoint = _pathPoints.Length - 1;
                }
                else
                {
                    _direction = 0f;
                    return;
                }
            }
            else
            {
                _currentStartPoint--;
            }
        }
        int newEndPointIndex = _currentStartPoint + 1;
        if (newEndPointIndex >= _pathPoints.Length)
            newEndPointIndex = 0;

        _startPoint = _pathPoints[_currentStartPoint];
        _endPoint = _pathPoints[newEndPointIndex];
        _pathVector = _endPoint.position - _startPoint.position;
        _pathVector = _pathVector.normalized;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /*if (_progress == _target)
        {
            if (MovesOnItsOwn)
            {
                _target = _progress == 0f ? 1f : 0f;
            }
            else
            {
                return;
            }
        }*/
        //_progress = Mathf.MoveTowards(_progress, _target, Speed * Time.fixedDeltaTime);
        if ((_platfrom.position - (Vector2)_endPoint.position).sqrMagnitude < 0.01f && _direction == 1f)
        {
            bool backTrackNow = (_currentStartPoint == _pathPoints.Length - 2 && _whenTheLastPointIsReached == MovingPlatformPathTypes.BacktrackToStartPoint && _movesOnItsOwn);
            if (_waitAtEveryPathPoint || backTrackNow)
            {
                _direction = 0f;
                if (_movesOnItsOwn && backTrackNow)
                {
                    StartCoroutine(StartMovingOnOwnWithDelay(-1f, _movingOnOwnDelay));
                }
                else if (_movesOnItsOwn)
                {
                    GetNewTargetPoints(1f);
                    StartCoroutine(StartMovingOnOwnWithDelay(1f, _movingOnOwnDelay));
                }
            }
            else
            {
                GetNewTargetPoints(1f);
            }
        }
        else if ((_platfrom.position - (Vector2)_startPoint.position).sqrMagnitude < 0.01f && _direction == -1f)
        {
            /*_direction = 0f;
            if (_movesOnItsOwn)
            {
                StartCoroutine(StartMovingOnOwnWithDelay(1f, _movingOnOwnDelay));
            }*/

            bool backTrackNow = (_currentStartPoint == 0 && _whenTheLastPointIsReached == MovingPlatformPathTypes.BacktrackToStartPoint && _movesOnItsOwn);
            if (_waitAtEveryPathPoint || backTrackNow)
            {
                _direction = 0f;
                if (_movesOnItsOwn && backTrackNow)
                {
                    StartCoroutine(StartMovingOnOwnWithDelay(1f, _movingOnOwnDelay));
                }
                else if (_movesOnItsOwn)
                {
                    GetNewTargetPoints(-1f);
                    StartCoroutine(StartMovingOnOwnWithDelay(-1f, _movingOnOwnDelay));
                }
            }
            else
            {
                GetNewTargetPoints(-1f);
            }
        }
        Vector2 movementVector = _pathVector * Speed * _direction;
        _platfrom.velocity = movementVector;
        foreach (Rigidbody2D rigidbody in _attachedRigidbodies)
        {
            rigidbody.velocity += movementVector;
        }
        //_platfrom.MovePosition(Vector2.Lerp(_startPoint.position, _endPoint.position, _progress));

    }

    IEnumerator StartMovingOnOwnWithDelay(float direction, float delay)
    {
        yield return new WaitForSeconds(delay);
        _direction = direction;
    }

    public void OnCollisionChange(bool isAdding, Collision2D collision)
    {
        Rigidbody2D rigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rigidbody == null || rigidbody.bodyType != RigidbodyType2D.Dynamic) return;
        
        if (isAdding) 
        {
            _attachedRigidbodies.Add(rigidbody);
        }
        else
        {
            _attachedRigidbodies.Remove(rigidbody);
        }
    }

    public void SetTarget(float target)
    {
        if (target == 0f)
        {
            _direction = 0f;
        }
        else if (target > 0f && (_platfrom.position - (Vector2)_endPoint.position).sqrMagnitude > 0.01f)
        {
            _direction = 1f;
        }
        else if (target < 0f && (_platfrom.position - (Vector2)_startPoint.position).sqrMagnitude > 0.01f)
        {
            _direction = -1f;
        }
        //_direction = target > 0 ? 1f : -1f;
    }

    public void SetSpeed(float speed) => Speed = speed;

    public void ToggleIfMovesOnItsOwn(bool toggle)
    {
        _movesOnItsOwn = toggle;
        if (!toggle)
        {
            _oldDirection = _direction;
        }
        _direction = toggle ? _oldDirection : 0f;
    }
}

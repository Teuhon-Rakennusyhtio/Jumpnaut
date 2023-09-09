using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public bool MovesOnItsOwn = false;
    public float  Speed = 1f;
    float _progress, _target;
    Transform _startPoint, _endPoint, _platfromTransform;
    Rigidbody2D _platfrom;
    // Start is called before the first frame update
    void Start()
    {
        _startPoint = transform.Find("StartPoint");
        _endPoint = transform.Find("EndPoint");
        _platfromTransform = transform.Find("Platform");
        _platfrom = _platfromTransform.gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_progress == _target)
        {
            if (MovesOnItsOwn)
            {
                _target = _progress == 0f ? 1f : 0f;
            }
            else
            {
                return;
            }
        }
        _progress = Mathf.MoveTowards(_progress, _target, Speed * Time.fixedDeltaTime);
        _platfrom.MovePosition(Vector2.Lerp(_startPoint.position, _endPoint.position, _progress));
    }

    public void SetTarget(float target)
    {
        _target = Mathf.Clamp01(target);
    }

    public void ToggleIfMovesOnItsOwn(bool toggle) => MovesOnItsOwn = toggle;
}

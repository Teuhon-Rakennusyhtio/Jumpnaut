using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootLogic : MonoBehaviour
{
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] GameObject _mainTarget;
    [SerializeField] GameObject _secondaryTarget;
    Transform _mainTargetTransform, _secondaryTargetTransform;
    bool _wasUsingSecondary = false;
    // Start is called before the first frame update
    void Start()
    {
        _mainTargetTransform = _mainTarget.transform;
        _secondaryTargetTransform = _secondaryTarget.transform;
    }

    void Update()
    {
        RaycastHit2D ground = Physics2D.Raycast(_mainTargetTransform.position + Vector3.up * 0.1f, Vector2.down, 0.2f, _groundLayer);
        if (ground && ground.point.y > _mainTargetTransform.position.y)
        {

            transform.position = new Vector2(_mainTargetTransform.position.x, ground.point.y);
            _secondaryTargetTransform.position = transform.position;
            if (!_wasUsingSecondary)
            {
                FootHitGround();
                _mainTarget.SetActive(false);
                _secondaryTarget.SetActive(true);
            }
            _wasUsingSecondary = true;
        }
        else if (_wasUsingSecondary)
        {
            _wasUsingSecondary = false;
            _mainTarget.SetActive(true);
            _secondaryTarget.SetActive(false);
        }
    }

    public float FootHeight
    {
        get
        {
            return Mathf.Max(_mainTargetTransform.position.y, _secondaryTargetTransform.position.y);
        }
    }


    void FootHitGround()
    {

    }
}

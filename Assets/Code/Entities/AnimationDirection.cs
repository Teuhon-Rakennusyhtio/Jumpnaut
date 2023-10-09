using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDirection : MonoBehaviour
{
    [SerializeField] Transform _rig;
    bool _facingLeft;

    public bool FacingLeft { get { return _facingLeft; } }

    void Update()
    {
        _facingLeft = _rig.localScale.x < 0f;
        Debug.Log(FacingLeft);
    }
}

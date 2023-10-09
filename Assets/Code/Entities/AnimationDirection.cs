using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDirection : MonoBehaviour
{
    bool _facingLeft;

    public bool FacingLeft { get { return _facingLeft; } }
    
    public void TurnLeft()
    {
        _facingLeft = true;
    }

    public void TurnRight()
    {
        _facingLeft = false;
    }
}

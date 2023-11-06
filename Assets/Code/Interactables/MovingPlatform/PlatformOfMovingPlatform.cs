using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformOfMovingPlatform : MonoBehaviour
{
    MovingPlatform _movingPlatform;

    void Start()
    {
        _movingPlatform = transform.parent.GetComponent<MovingPlatform>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        _movingPlatform.OnCollisionChange(true, collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        _movingPlatform.OnCollisionChange(false, collision);
    }
}

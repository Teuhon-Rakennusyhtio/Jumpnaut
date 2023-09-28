using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundAlert : MonoBehaviour
{
    bool _onTheEdge;

    OnCollisionStay2D(Collision2D col)
    {
        if (col.CompareTag("Player") && _onTheEdge == true)
        {
            
        }
    }
}

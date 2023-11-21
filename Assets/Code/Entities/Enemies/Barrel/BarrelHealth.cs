using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BarrelHealth : GenericHealth
{
    [SerializeField] UnityEvent _die;
    int pointWorth = 100;

    protected override void Die()
    {
        GameManager.AddScore(pointWorth, transform.position);
        _die.Invoke();
    }
}

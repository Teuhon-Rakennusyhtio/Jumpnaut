using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BarrelHealth : GenericHealth
{
    [SerializeField] UnityEvent _die;

    protected override void Die()
    {
        _die.Invoke();
    }
}

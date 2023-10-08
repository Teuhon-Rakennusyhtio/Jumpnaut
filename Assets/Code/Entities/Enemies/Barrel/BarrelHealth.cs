using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BarrelHealth : GenericHealth
{
    [SerializeField] UnityEvent _die;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    protected override void DamagedLogic(Weapon weapon)
    {
        Debug.Log(_health);
    }

    protected override void Die()
    {
        _die.Invoke();
    }
}

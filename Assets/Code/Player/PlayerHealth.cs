using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : GenericHealth
{
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
        Debug.Log("I died :(");
    }
}

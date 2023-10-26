using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : GenericHealth
{
    PlayerDamagedEventArgs _args;
    // Start is called before the first frame update
    void Start()
    {
        _args = new PlayerDamagedEventArgs();
    }

    protected override void DamagedLogic(Weapon weapon)
    {
        Debug.Log(_health);
        _args.Health = _health;
        OnPlayerDamaged();
    }

    protected override void Die()
    {
        Debug.Log("I died :(");
    }

    public delegate void PlayerDamagedEventHandler(object source, EventArgs args);
    public event PlayerDamagedEventHandler PlayerDamaged;

    protected virtual void OnPlayerDamaged()
    {
        if (PlayerDamaged != null)
            PlayerDamaged(this, _args);
    }
}

public class PlayerDamagedEventArgs : EventArgs
{
    public int Health { get; set; }
}

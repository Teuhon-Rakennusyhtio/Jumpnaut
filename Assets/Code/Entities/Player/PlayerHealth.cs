using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : GenericHealth
{
    [SerializeField] PlayerMover _playerMover;
    PlayerHealthEventArgs _args;
    // Start is called before the first frame update
    void Start()
    {
        _args = new PlayerHealthEventArgs();
    }

    protected override void DamagedLogic(Weapon weapon)
    {
        _args.Health = _health;
        _playerMover.Damaged(_maxInvincibilityFrames);
        OnPlayerDamaged();
    }

    protected override void Die()
    {
        _playerMover.Die();
        OnPlayerDeath();
    }

    public delegate void PlayerDamagedEventHandler(object source, PlayerHealthEventArgs args);
    public delegate void PlayerDeathEventHandler(object source, PlayerHealthEventArgs args);
    public event PlayerDamagedEventHandler PlayerDamaged;
    public event PlayerDeathEventHandler PlayerDeath;

    protected virtual void OnPlayerDamaged()
    {
        PlayerDamaged?.Invoke(this, _args);
    }

    protected virtual void OnPlayerDeath()
    {
        PlayerDeath?.Invoke(this, _args);
    }
}

public class PlayerHealthEventArgs : EventArgs
{
    public int Health { get; set; }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : GenericHealth
{
    [SerializeField] PlayerMover _playerMover;
    PlayerHealthEventArgs _args;
    int _pointWorth = -500;
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
        GameManager.AddScore(_pointWorth, transform.position);
        _playerMover.Die();
        OnPlayerDeath();
    }

    public override void Heal(int amount)
    {
        base.Heal(amount);
        _args.Health = _health;
        OnPlayerHealed();
    }

    public delegate void PlayerDamagedEventHandler(object source, PlayerHealthEventArgs args);
    public delegate void PlayerDeathEventHandler(object source, PlayerHealthEventArgs args);
    public delegate void PlayerHealedEventHandler(object source, PlayerHealthEventArgs args);
    public event PlayerDamagedEventHandler PlayerDamaged;
    public event PlayerDeathEventHandler PlayerDeath;
    public event PlayerHealedEventHandler PlayerHealed;

    protected virtual void OnPlayerDamaged()
    {
        PlayerDamaged?.Invoke(this, _args);
    }

    protected virtual void OnPlayerDeath()
    {
        PlayerDeath?.Invoke(this, _args);
    }

    protected virtual void OnPlayerHealed()
    {
        PlayerHealed?.Invoke(this, _args);
    }
}

public class PlayerHealthEventArgs : EventArgs
{
    public int Health { get; set; }
}

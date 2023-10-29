using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum WeaponAlignment
{
    none,
    enemy,
    player
}

public class Weapon : MonoBehaviour
{
    [Tooltip("0 = Damages enemy, 1 = Damages player, 2 >= Damages both")]
    public WeaponAlignment Alignment = 0;
    [SerializeField] int _damage = 1;
    [SerializeField] float _knockbackForce = 1f;
    [SerializeField] UnityEvent _weaponHit;
    [SerializeField] bool _stayingDamage;
    public bool Thrown { get; set; }

    public Vector2 LatestHitPosition;

    public int Damage { get { return _damage; } }
    public float KnockbackForce { get { return _knockbackForce; } }
    public bool StayingDamage { get { return _stayingDamage; } }

    public void WeaponHit()
    {
        _weaponHit.Invoke();
    }
}

public class WeaponEventArgs : EventArgs
{
    public Sprite Sprite { get; set; }
    public int DurabilityType { get; set; }
    public int Durability { get; set; }
    public float AnalogDurability { get; set; }
}

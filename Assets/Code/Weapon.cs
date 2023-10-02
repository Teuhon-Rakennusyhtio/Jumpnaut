using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    [Tooltip("0 = Player, 1 = Enemy, 2 >= Player and enemy")]
    public int Alignment = 0;
    [SerializeField] int _damage = 1;
    [SerializeField] float _knockbackForce = 1f;
    [SerializeField] UnityEvent _weaponHit;
    public bool Thrown = false;

    public int Damage { get { return _damage; } }
    public float KnockbackForce { get{ return _knockbackForce; } }

    public void WeaponHit()
    {
        _weaponHit.Invoke();
    }
}

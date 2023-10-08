using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    [Tooltip("0 = Damages enemy, 1 = Damages player, 2 >= Damages both")]
    public int Alignment = 0;
    [SerializeField] int _damage = 1;
    [SerializeField] float _knockbackForce = 1f;
    [SerializeField] UnityEvent _weaponHit;
    [SerializeField] bool _subHitbox;
    [SerializeField] Weapon _mainWeapon;
    public bool Thrown = false;

    public Vector2 LatestHitDirection;

    public int Damage { get { return _damage; } }
    public float KnockbackForce { get { return _knockbackForce; } }

    public void WeaponHit()
    {
        if (_subHitbox)
        {
            _mainWeapon.LatestHitDirection = LatestHitDirection;
        }
        _weaponHit.Invoke();
    }
}

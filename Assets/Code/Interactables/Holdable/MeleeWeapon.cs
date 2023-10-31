using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Holdable
{
    float _swingDuration;
    bool _alreadyHit;
    //BreakageDebris[] _debris;
    void Start()
    {
        _weaponUseAnimation = "Melee Swing";
        //_debris = GetComponentsInChildren<BreakageDebris>();
    }

    protected override void OnAttack()
    {
        _alreadyHit = false;
        _weaponCollider.enabled = true;
        _swingDuration = 0.5f;
    }

    public void WeaponHit()
    {
        if (_alreadyHit) return;
        _alreadyHit = true;
        RemoveDurability(1);
    }

    protected override void OnThrow(Vector2 direction)
    {
        base.OnThrow(direction);
        _weaponCollider.enabled = true;
        _alreadyHit = false;
    }

    protected override void OnPickup(Transform hand, GenericHealth health)
    {
        base.OnPickup(hand, health);
        _weaponCollider.enabled = false;
    }

    public override void Break()
    {
        base.Break();
    }

    void Update()
    {
        if (_swingDuration > 0f)
        {
            _swingDuration -= Time.deltaTime;
            if (_swingDuration <= 0f)
            {
                _weaponCollider.enabled = false;
                _alreadyHit = false;
            }
        }
    }
}

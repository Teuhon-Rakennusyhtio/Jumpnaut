using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Holdable
{
    [SerializeField] float _swingWeaponHitboxDuration = 0.5f;
    float _currentSwingDuration;
    bool _alreadyHit;
    //BreakageDebris[] _debris;
    void Start()
    {
        //_weaponUseAnimation = "Melee Swing";
        //_debris = GetComponentsInChildren<BreakageDebris>();
    }

    protected override void OnAttack()
    {
        _alreadyHit = false;
        _weaponCollider.enabled = true;
        _currentSwingDuration = _swingWeaponHitboxDuration;
    }

    public void WeaponHit()
    {
        CameraMovement.SetCameraShake(2, 0.7f, 0.5f, 1f);
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
        if (_currentSwingDuration > 0f)
        {
            _currentSwingDuration -= Time.deltaTime;
            if (_currentSwingDuration <= 0f)
            {
                _weaponCollider.enabled = false;
                _alreadyHit = false;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : GenericHealth
{
    [SerializeField] EnemyMover _mover;
    int pointWorth = 500;
    float _damageDirection;

    public override void Damaged(Weapon weapon, Vector2 position)
    {
        _damageDirection = position.x;
        _mover.Damaged(_invincibilityFrames);
        base.Damaged(weapon, position);
    }
    protected override void Die()
    {
        GameManager.AddScore(pointWorth);
        base.Die();
        _mover.Die();
    }
}

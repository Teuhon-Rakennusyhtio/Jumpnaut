using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saw : Holdable
{
    [SerializeField] Collider2D _weaponCollider;
    [SerializeField] Weapon _weapon;
    [SerializeField] Animator _animator;
    [SerializeField] ParticleSystem _sparks;

    protected override void OnPickup(Transform hand)
    {
        GenericHealth health = hand.parent.GetComponentInChildren<GenericHealth>();
        _weapon.Thrown = false;
        if (health != null) _weapon.Alignment = health.Alignment; 
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Running"))
        {
            _animator.Play("Open");
        }
    }

    protected override void OnThrow(Vector2 direction)
    {
        _weapon.Thrown = true;
    }

    // Shoots spark particles towards the hurtbox that has been hit
    public void Sparks()
    {
        float angle = Vector2.SignedAngle(_weapon.LatestHitDirection, Vector2.up);
        var sparksShape = _sparks.shape;
        sparksShape.rotation = Vector3.up * angle;
        _sparks.Play();
    }

    void Update()
    {
        if (BeingHeld || _thrown)
        {
            _weaponCollider.enabled = !_weaponCollider.enabled;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saw : Holdable
{
    [SerializeField] Collider2D[] _weaponColliders;
    [SerializeField] Weapon[] _weapons;
    [SerializeField] Animator _animator;
    [SerializeField] ParticleSystem _sparks;

    protected override void OnPickup(Transform hand)
    {
        GenericHealth health = hand.parent.GetComponentInChildren<GenericHealth>();
        if (health != null)
        {
            foreach (Weapon weapon in _weapons)
            {
                weapon.Alignment = health.Alignment;
                weapon.Thrown = false;
            }
        } 
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Running"))
        {
            _animator.Play("Open");
        }
        foreach (Collider2D weaponCollider in _weaponColliders)
        {
            weaponCollider.enabled = true;
        }
    }

    protected override void OnThrow(Vector2 direction)
    {
        foreach (Weapon weapon in _weapons)
        {
            weapon.Thrown = true;
        }
    }

    // Shoots spark particles towards the hurtbox that has been hit
    public void Sparks()
    {
        float angle = Vector2.SignedAngle(_weapons[0].LatestHitDirection, Vector2.up);
        var sparksShape = _sparks.shape;
        sparksShape.rotation = Vector3.up * angle;
        _sparks.Play();
    }

    void Update()
    {
        if (BeingHeld || _thrown)
        {
            _weaponColliders[0].enabled = !_weaponColliders[0].enabled;
        }
    }
}

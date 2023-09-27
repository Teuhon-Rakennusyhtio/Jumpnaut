using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saw : Holdable
{
    [SerializeField] Collider2D _weaponCollider;
    [SerializeField] Weapon _weapon;
    [SerializeField] Animator _animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    protected override void OnPickup(Transform hand)
    {
        GenericHealth health = hand.parent.GetComponentInChildren<GenericHealth>();
        if (health != null) _weapon.Alignment = health.Alignment; 
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Running"))
        {
            _animator.Play("Open");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (BeingHeld || _thrown)
        {
            _weaponCollider.enabled = !_weaponCollider.enabled;
        }
    }
}

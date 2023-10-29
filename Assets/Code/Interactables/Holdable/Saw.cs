using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Saw : Holdable
{
    //[SerializeField] Collider2D[] _weaponColliders;
    //[SerializeField] Weapon[] _weapons;
    [SerializeField] Animator _animator;
    [SerializeField] ParticleSystem _sparks;
    [SerializeField] ParticleSystem _smoke;
    [SerializeField] Transform _sparkPosition;
    bool _exploded;
    bool _isTurnedOn;

    protected override void OnPickup(Transform hand, GenericHealth health)
    {
        base.OnPickup(hand, health);
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Running"))
        {
            _animator.Play("Open");
        }
        _weaponCollider.enabled = true;
        _isTurnedOn = true;
    }

    protected override void OnOutOfDurability()
    {
        _weaponCollider.enabled = false;
        _animator.Play("Out of fuel");
        _smoke.transform.parent = null;
        _smoke.Play();
    }

    // Shoots spark particles towards the hurtbox that has been hit
    public void Sparks()
    {
        _sparks.transform.parent = null;
        float angle = Vector2.SignedAngle(_weapon.LatestHitPosition - (Vector2)_weapon.transform.position, Vector2.up);
        var sparksShape = _sparks.shape;
        sparksShape.rotation = Vector3.up * angle;
        _sparks.Play();
    }

    public override void Break()
    {
        if (_exploded) return;
        _exploded = true;
        GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(Explode());
    }

    IEnumerator Explode()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(_sparks.gameObject);
        Destroy(_smoke.gameObject);
        Destroy(gameObject);
    }

    void Update()
    {
        if (_isTurnedOn)
        {
            RemoveDurability(Time.deltaTime);
        }
        if (BeingHeld || _thrown)
        {
            //_weaponColliders[0].enabled = !_weaponColliders[0].enabled;
            _sparks.transform.position = _sparkPosition.position;
            _smoke.transform.position = transform.position;
        }
    }
}

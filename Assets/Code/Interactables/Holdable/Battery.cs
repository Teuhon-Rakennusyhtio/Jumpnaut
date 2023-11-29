using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : Holdable
{
    [SerializeField] GameObject _particleEffect;
    [SerializeField] BatteryCollidedWithHurtbox _hurtboxCollider;
    [SerializeField] Collider2D _explosionCollider;
    public BatterySpawner BatterySpawner;
    bool _exploded = false, _firstTimePickup = true;
    AudioManager _audioManager;

    void Start()
    {
        /*_breaksOnImpact = true;
        _flipable = false;
        _isHeavy = true;*/
        GameObject audioManagerGameObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioManagerGameObject != null)
            _audioManager = audioManagerGameObject.GetComponent<AudioManager>();
    }

    public override void Break()
    {
        if (_exploded) return;
        _exploded = true;
        GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(Explode());
    }
    
    public void PlaceInSocket(Transform socket)
    {
        _thrown = false;
        BeingHeld = true;
        _holder.ClearHand();
        transform.parent = socket;
        transform.localPosition = Vector2.zero;
        transform.localRotation = Quaternion.identity;
    }

    protected override void OnPickup(Transform hand, GenericHealth health)
    {
        base.OnPickup(hand, health);
        _hurtboxCollider.Alignment = _alignment;
        _hurtboxCollider.enabled = false;
        if (BatterySpawner != null && _firstTimePickup)
        {
            _firstTimePickup = false;
            BatterySpawner.StartSpawning();
        }
    }

    protected override void OnThrow(Vector2 direction)
    {
        base.OnThrow(direction);
        _hurtboxCollider.enabled = true;
    }

    IEnumerator Explode()
    {
        RaycastHit2D[] potentialBatteryBreakables = Physics2D.CircleCastAll(transform.position, 2, Vector2.zero);
        foreach (RaycastHit2D potentialBreakable in potentialBatteryBreakables)
        {
            potentialBreakable.collider.GetComponent<IBatteryDamageable>()?.HitByBattery();
        }
        _audioManager?.PlaySFX(_audioManager.explosion);
        _explosionCollider.GetComponent<Weapon>().Alignment = _alignment;
        _explosionCollider.enabled = true;
        CameraMovement.SetCameraShake(3, 5, 1, 1f);
        GameObject explosionEffectObject = Instantiate(_particleEffect, transform.position, Quaternion.identity);
        ParticleSystem explosionEffect = explosionEffectObject.GetComponent<ParticleSystem>();
        explosionEffect.Play();
        yield return new WaitForSeconds(explosionEffect.main.duration);
        Destroy(explosionEffectObject);
        Destroy(gameObject);
    }
}

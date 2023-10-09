using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : Holdable
{
    [SerializeField] GameObject _particleEffect;
    public BatterySpawner BatterySpawner;
    bool _exploded = false;
    void Start()
    {
        _breaksOnImpact = true;
        _flipable = false;
        _isHeavy = true;
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
        GetComponentInParent<PlayerMover>().ClearHand();
        transform.parent = socket;
        transform.localPosition = Vector2.zero;
        transform.localRotation = Quaternion.identity;
    }

    IEnumerator Explode()
    {
        GameObject explosionEffectObject = Instantiate(_particleEffect, transform.position, Quaternion.identity);
        ParticleSystem explosionEffect = explosionEffectObject.GetComponent<ParticleSystem>();
        explosionEffect.Play();
        yield return new WaitForSeconds(explosionEffect.main.duration);
        if (BatterySpawner != null) BatterySpawner.SpawnBattery();
        Destroy(explosionEffectObject);
        Destroy(gameObject);
    }
}

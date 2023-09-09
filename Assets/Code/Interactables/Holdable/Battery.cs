using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : Holdable
{
    [SerializeField] Sprite _explosionGraphic;
    public BatterySpawner BatterySpawner;
    bool _exploded = false;
    void Awake()
    {
        _breaksOnImpact = true;
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
    }

    IEnumerator Explode()
    {
        Debug.Log("BOOM");
        GameObject explosionEffect = new GameObject("Battery explosion");
        explosionEffect.transform.position = transform.position;
        SpriteRenderer explosionRenderer = explosionEffect.AddComponent<SpriteRenderer>();
        explosionRenderer.sprite = _explosionGraphic;
        yield return new WaitForSeconds(0.5f);
        if (BatterySpawner != null) BatterySpawner.SpawnBattery();
        Destroy(explosionEffect);
        Destroy(gameObject);
    }
}

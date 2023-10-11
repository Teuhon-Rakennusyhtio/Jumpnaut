using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatterySpawner : MonoBehaviour
{
    [SerializeField] GameObject _battery;
    [SerializeField] bool _existsInSinglePlayer;
    [SerializeField] Animator _animator;
    [SerializeField] ParticleSystem _particleEffect;
    [SerializeField] SpriteRenderer _beltSprite;
    MaterialPropertyBlock _beltMaterialBlock;
    void Start()
    {
        _beltMaterialBlock = new MaterialPropertyBlock();
        _beltSprite.GetPropertyBlock(_beltMaterialBlock);
        if (!_existsInSinglePlayer && GameManager.PlayerDevices.Count == 1) return;
        StartSpawning();
    }

    IEnumerator SpawnBattery()
    {
        yield return new WaitForSeconds(4f);
        _beltMaterialBlock.SetFloat("_Speed", 0f);
        _beltSprite.SetPropertyBlock(_beltMaterialBlock);
        GameObject newBattery = Instantiate(_battery, transform.position, Quaternion.identity);
        newBattery.GetComponent<Battery>().BatterySpawner = this;

    }

    public void StartSpawning()
    {
        _beltMaterialBlock.SetFloat("_Speed", 1f);
        _beltSprite.SetPropertyBlock(_beltMaterialBlock);
        _animator.Play("FactoryFactoring");
        _particleEffect.Play();
        StartCoroutine(SpawnBattery());
    }
}

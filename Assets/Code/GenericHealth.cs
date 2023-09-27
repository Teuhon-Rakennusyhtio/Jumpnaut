using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericHealth : MonoBehaviour
{
    [SerializeField] int _maxHealth = 3;
    [SerializeField] float _maxInvincibilityFrames = 0.1f;
    [SerializeField] bool _isPlayerAligned = false;
    protected bool _invincibleToCatchable = false;
    protected int _health;
    protected float _invincibilityFrames = 0f;
    int _weaponLayer;
    
    void Awake()
    {
        _weaponLayer = LayerMask.NameToLayer("Weapon");
        _health = _maxHealth;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (_invincibilityFrames > 0f) return;
        if (collision.gameObject.layer != _weaponLayer) return;
        Damaged(collision.GetComponent<Weapon>());

    }

    public void Damaged(Weapon weapon)
    {
        if (
            (_isPlayerAligned && weapon.Alignment == 0) ||
            (!_isPlayerAligned && weapon.Alignment == 1) ||
            (weapon.Thrown && _invincibleToCatchable)
            ) return;

        weapon.WeaponHit();
        _health -= weapon.Damage;
        if (_health <= 0)
        {
            Die();
            return;
        }
        _invincibilityFrames = _maxInvincibilityFrames;
        
        DamagedLogic(weapon);
    }

    public int Alignment
    {
        get
        { 
            return _isPlayerAligned ? 0 : 1;
        }
    }

    protected virtual void DamagedLogic(Weapon weapon)
    {

    }

    protected virtual void Die()
    {
        
    }

    void Update()
    {
        if (_invincibilityFrames > 0f)
        {
            _invincibilityFrames -= Time.deltaTime;
        }
        UpdateLogic();
    }

    protected virtual void UpdateLogic()
    {
        
    }
}

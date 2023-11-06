using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericHealth : MonoBehaviour
{
    [SerializeField] int _maxHealth = 3;
    [SerializeField] protected float _maxInvincibilityFrames = 0.1f;
    [SerializeField] bool _isPlayerAligned = false;
    protected bool _invincibleToCatchable = false;
    protected int _health;
    protected float _invincibilityFrames = 0f;
    int _weaponLayer;

    List<Weapon> _weapons;
    
    public int MaxHealth
    {
        get
        {
            return _maxHealth;
        }
    }

    public int CurrentHealth
    {
        get
        {
            return _health;
        }
    }

    void Awake()
    {
        _weaponLayer = LayerMask.NameToLayer("Weapon");
        _health = _maxHealth;
        _weapons = new List<Weapon>();
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer != _weaponLayer || _health <= 0) return;
        Weapon weapon = collision.GetComponent<Weapon>();
        if (
            (weapon.Alignment == Alignment) ||
            (weapon.Thrown && _invincibleToCatchable)
            ) return;
        if (_weapons.Contains(weapon))
            if (!weapon.StayingDamage) return;
        else
            _weapons.Add(weapon);
        Vector2 hitPosition = collision.transform.position;
        
        Damaged(weapon, hitPosition);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Weapon weapon = collision.GetComponent<Weapon>();
        if (weapon == null) return;
        _weapons.Remove(weapon);
    }

    public virtual void Damaged(Weapon weapon, Vector2 position)
    {
        /*if (
            (_isPlayerAligned && weapon.Alignment == 0) ||
            (!_isPlayerAligned && weapon.Alignment == 1) ||
            (weapon.Thrown && _invincibleToCatchable)
            ) return;*/

        weapon.LatestHitPosition = position;
        weapon.WeaponHit();
        if (_invincibilityFrames > 0f) return;
        _health -= weapon.Damage;
        if (_health <= 0)
        {
            Die();
            return;
        }
        _invincibilityFrames = _maxInvincibilityFrames;
        
        DamagedLogic(weapon);
    }

    protected virtual void DamagedLogic(Weapon weapon)
    {
        
    }

    public virtual void Heal(int amount)
    {
        _health = Mathf.Min(_maxHealth, _health + amount);
    }

    public WeaponAlignment Alignment
    {
        get
        { 
            return _isPlayerAligned ? WeaponAlignment.player : WeaponAlignment.enemy;
        }
    }

    protected virtual void Die()
    {
        
    }

    protected virtual void Update()
    {
        if (_invincibilityFrames > 0f)
        {
            _invincibilityFrames -= Time.deltaTime;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum DurabilityType
{
    none,
    digital,
    analog
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Holdable : MonoBehaviour
{
    [SerializeField] Vector2 _positionInHand;
    [SerializeField] protected Weapon _weapon;
    [SerializeField] protected string _weaponUseAnimation;
    ToolBox _toolbox;
    protected Collider2D _weaponCollider;
    protected GenericMover _holder;
    SpriteRenderer _renderer;
    protected Rigidbody2D _rigidBody;
    CircleCollider2D _collider;
    int _groundLayerId;
    LayerMask _groundLayer;
    protected bool _thrown = false;
    protected bool _floating = false;
    float _xVelocity = 0f, _yVelocity = 0f, _outOfViewTime = 0f, _timeSinceThrown = 0f;
    public bool BeingHeld { get; set; }
    //public bool IsMeleeWeapon { get { return _isMeleeWeapon; } }
    Vector3 _realSize;
    Vector3 _floatingPosition;
    protected Vector2 _breakingCollisionPoint;
    
    [SerializeField] protected float _throwFallTime = 1f, _terminalVelocity = -15f, _fallAcceleration = 1f, _throwForce = 15f, _throwTorque = 1f;
    [SerializeField] protected bool _breaksOnImpact = false, _isHeavy = false, _flipable = true, _isWeapon = false, _startsFloatingWhenThrown = true, _startFloatingWhenSpawned = true;
    [SerializeField] DurabilityType _durabilityType = 0;
    [SerializeField] int _digitalDurability = 3;
    [SerializeField] float _analogDurability = 1f;
    [SerializeField] float _weaponCooldown;
    [SerializeField] float _weaponAnimationSpeed = 1f;
    [SerializeField] Sprite _itemIcon;
    protected BreakageDebris[] _debris;
    float _maxAnalogDurability;
    int _maxDigitalDurability;
    bool _broken;
    float _debrisAngle = 1.25f;
    protected bool _isHelmet = false;
    protected WeaponAlignment _alignment;
    GameObject _glowEffect;
    ParticleSystem _glowParticles;
    Light2D _glowLight;
    float _timeStoodStill = 0f;
    Vector3 _oldPosition;
    float _timeOffset;

    public Sprite ItemIcon { get { return _itemIcon; } }
    public int DigitalDurability { get { return _digitalDurability; } }
    public float AnalogDurability { get { return _analogDurability; } }
    public string WeaponUseAnimation { get { return _weaponUseAnimation; } }
    public bool IsHelmet { get { return _isHelmet; } }
    public ToolBox ToolBox
    {
        get
        {
            return _toolbox;
        }
        set
        {
            if (_toolbox == null)
            {
                _toolbox = value;
            }
        }
    }

    
    void Awake()
    {
        _maxAnalogDurability = _analogDurability;
        _maxDigitalDurability = _digitalDurability;
        _realSize = transform.localScale;
        _groundLayerId = LayerMask.NameToLayer("Ground");
        _groundLayer = LayerMask.GetMask("Ground");
        _renderer = GetComponent<SpriteRenderer>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _debris = GetComponentsInChildren<BreakageDebris>();
        _glowEffect = Instantiate(Resources.Load<GameObject>("Prefabs/HoldableGlow"), transform);
        _glowParticles = _glowEffect.GetComponent<ParticleSystem>();
        _glowLight = _glowEffect.GetComponent<Light2D>();
        _timeOffset = Random.Range(-1f, 1f);

        if (_analogDurability == 0f)
        {
            Debug.LogError($"On item {gameObject.name}, the analog durability can't be zero!");
            _analogDurability = 1f;
        }
        if (_weapon != null)
            _weaponCollider = _weapon.GetComponent<Collider2D>();
        
        if (_startFloatingWhenSpawned)
        {
            StartFloating();
            transform.position = _floatingPosition;
        }
    }

    void FixedUpdate()
    {
        if (_thrown)
        {
            _timeSinceThrown += Time.fixedDeltaTime;


            // Let the "physics" take hold after the wanted duration of time has passed since the item was thrown.
            if (_timeSinceThrown > _throwFallTime && _yVelocity > _terminalVelocity)
            {
                _yVelocity = Mathf.MoveTowards(_yVelocity, _terminalVelocity, _fallAcceleration * 50f * Time.fixedDeltaTime);
                _xVelocity = Mathf.MoveTowards(_xVelocity, 0f, _fallAcceleration * 25f * Time.fixedDeltaTime);
            }

            if ((transform.position - _oldPosition).sqrMagnitude < 0.00001f)
            {
                _timeStoodStill += Time.deltaTime;
            }
            else
            {
                _timeStoodStill = 0f;
            }
            _oldPosition = transform.position;

            if (_startsFloatingWhenThrown && _timeStoodStill > 1f)
            {
                _thrown = false;
                Invoke(nameof(StartFloating), 0.5f);
            }

            _rigidBody.velocity = new Vector2(_xVelocity, _yVelocity);
            // If the thrown holdable hasn't been seen in 30 seconds it probably doesn't need to exist anymore.
            if (!_renderer.isVisible) _outOfViewTime += Time.fixedDeltaTime;
            else _outOfViewTime = 0f;
        }
        if (_outOfViewTime > 30f) Break();
        if (_floating)
            Floating();
    }

    public void Throw(Vector2 direction)
    {
        bool flipped = transform.lossyScale.x < 0f;
        transform.parent = null;
        transform.localScale = flipped ? new Vector3(-_realSize.x, _realSize.y, _realSize.z) : _realSize;
        //transform.localScale = _realSize;
        _rigidBody.isKinematic = false;
        Vector2 throwVector = direction * _throwForce;
        _xVelocity = throwVector.x;
        _yVelocity = throwVector.y;
        _rigidBody.AddTorque(_throwTorque * (direction.x < 0f ? 1f : -1f), ForceMode2D.Impulse);
        _timeSinceThrown = 0f;
        _timeStoodStill = 0f;
        _thrown = true;
        BeingHeld = false;
        _holder = null;
        OnThrow(direction);
    }

    protected void RemoveDurability(int removedDurablity)
    {
        if (_weapon.Alignment == WeaponAlignment.enemy) return; // Enemies will not lose item durability
        removedDurablity = Mathf.Abs(removedDurablity);
        if (_durabilityType == DurabilityType.digital)
        {
            if (_digitalDurability <= 0) return;
            _digitalDurability -= removedDurablity;
            if (_digitalDurability <= 0)
            {
                _digitalDurability = 0;
                OnOutOfDurability();
            }
        }
        else if (_durabilityType == DurabilityType.analog)
        {
            if (_analogDurability <= 0f) return;
            _analogDurability -= removedDurablity;
            if (_analogDurability <= 0f)
            {
                _analogDurability = 0f;
                OnOutOfDurability();
            }
        }
        else
        {
            Debug.LogWarning($"Cannot remove durability from {gameObject.name} because it has no durability type!");
        }
        _holder?.DurabilityChanged();
    }

    protected void RemoveDurability(float removedDurablity)
    {
        if (_broken || _weapon.Alignment == WeaponAlignment.enemy) return; // Enemies will not lose item durability
        removedDurablity = Mathf.Abs(removedDurablity);
        if (_durabilityType == DurabilityType.digital)
        {
            Debug.LogWarning($"{gameObject.name} has digital durability and only integers are recommended to be removed from it! (tried to remove a float value {removedDurablity} from it)");
            if (_digitalDurability <= 0) return;
            _digitalDurability -= (int)removedDurablity;
            if (_digitalDurability <= 0)
            {
                _digitalDurability = 0;
                OnOutOfDurability();
            }
        }
        else if (_durabilityType == DurabilityType.analog)
        {
            if (_analogDurability <= 0f) return;
            _analogDurability -= removedDurablity;
            if (_analogDurability <= 0f)
            {
                _analogDurability = 0f;
                OnOutOfDurability();
            }
        }
        else
        {
            Debug.LogWarning($"Cannot remove durability from {gameObject.name} because it has no durability type!");
        }
        _holder?.DurabilityChanged();
    }

    protected virtual void OnThrow(Vector2 direction)
    {
        if (_weapon != null)
            _weapon.Thrown = true;
    }

    public void Attack()
    {
        OnAttack();
    }

    protected virtual void OnAttack()
    {
        
    }

    protected virtual void OnOutOfDurability()
    {
        Break();
    }

    public void Pickup(Transform hand, GenericMover holder, GenericHealth health, bool isFacingLeft, ref bool heavy, ref bool flipable, ref bool isWeapon, ref float weaponCooldown, ref string weaponUseAnimation, ref float weaponAnimationSpeed)
    {
        StopFloating();
        _thrown = false;
        _rigidBody.totalTorque = 0f;
        _rigidBody.freezeRotation = true;
        _rigidBody.freezeRotation = false;
        _rigidBody.isKinematic = true;
        _xVelocity = 0f;
        _yVelocity = 0f;
        _rigidBody.velocity = Vector2.zero;
        transform.parent = hand;
        transform.localScale = _realSize;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = _positionInHand;
        if (!_flipable)
        {
            transform.localPosition += (isFacingLeft ? Vector3.zero : Vector3.left * _positionInHand.x * 2);
        }
        _holder = holder;
        BeingHeld = true;
        OnPickup(hand, health);
        heavy = _isHeavy;
        flipable = _flipable;
        isWeapon = _isWeapon;
        weaponCooldown = _weaponCooldown;
        weaponAnimationSpeed = _weaponAnimationSpeed;
        weaponUseAnimation = _weaponUseAnimation;
    }

    protected virtual void OnPickup(Transform hand, GenericHealth health)
    {
        _toolbox?.SpawnNewHoldable();
        _toolbox = null;
        if (health != null)
        {
            _alignment = health.Alignment;
        }
        if (_weapon != null)
        {
            _weapon.Alignment = _alignment;
            _weapon.Thrown = false;
        } 
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (_thrown && collision.gameObject.layer == _groundLayerId && !_broken)
        {
            if (_breaksOnImpact)
            {
                _holder?.ClearHand();
                BeingHeld = true;
                _breakingCollisionPoint = collision.GetContact(0).point;
                Break();
            }
            else
            {
                if (collision.GetContact(0).point.y < transform.position.y)
                {
                    _xVelocity = 0f;
                }
                else
                {
                    _timeSinceThrown = _throwFallTime;
                }
            }
        }
    }

    protected void StartFloating()
    {
        if (_holder != null) return;
        _rigidBody.isKinematic = true;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.angularVelocity = 0f;
        _thrown = false;
        _floating = true;
        _glowParticles.Play();
        RaycastHit2D groundRaycast = Physics2D.Raycast(transform.position, Vector2.down, 2f, _groundLayer);
        if (groundRaycast)
        {
            _floatingPosition = groundRaycast.point + Vector2.up;
        }
        else
        {
            _floatingPosition = transform.position;
        }
    }

    protected void StopFloating()
    {
        _floating = false;
        _glowLight.intensity = 0f;
        _glowParticles.Stop();
    }

    void Floating()
    {
        transform.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(transform.localEulerAngles.z, Mathf.Sin((Time.time + _timeOffset) * 0.7f) * 5f, Time.deltaTime * 25f));
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, _floatingPosition + Vector3.up * Mathf.Sin(Time.time + _timeOffset) * 0.2f, Time.deltaTime * 0.4f);
        _glowLight.intensity = Mathf.MoveTowards(_glowLight.intensity, Mathf.Sin((Time.time + _timeOffset) * 0.85f) / 2 + 1f, Time.deltaTime);
    }

    public virtual void Break()
    {
        _broken = true;
        _holder?.ClearHand();
        if (_breakingCollisionPoint != Vector2.zero)
        {
            float angle = Vector2.Angle(_breakingCollisionPoint - (Vector2)transform.position, Vector2.right);
            _debrisAngle -= (angle / 90) - 1;
        }
        for (int i = 0; i < _debris.Length; i++)
        {
            _debris[i].Break(_debrisAngle);
            _debrisAngle += 1f / _debris.Length;
        }
        Destroy(gameObject);
    }

    public void GetHoldableEventArgs(HoldableEventArgs args)
    {
        args.ItemIcon = _itemIcon;
        args.MaxDigitalDurability = _maxDigitalDurability;
        args.DurabilityType = _durabilityType;
        args.DigitalDurability = _digitalDurability;
        args.AnalogDurability = _analogDurability / _maxAnalogDurability;
    }
}

public class HoldableEventArgs : System.EventArgs
{
    public Sprite ItemIcon { get; set; }
    public DurabilityType DurabilityType { get; set; }
    public int MaxDigitalDurability { get; set; }
    public int DigitalDurability { get; set; }
    public float AnalogDurability { get; set; }
}

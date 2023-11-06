using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : GenericMover
{
    [SerializeField] GameObject _playerHealthBar;
    //[SerializeField] PlayerHealth _health;

    //[SerializeField] Animator _animator;
    //[SerializeField] protected Transform _mainRig, _leftArm, _rightArm, _climbArm;
    MaterialPropertyBlock _materialPropertyBlock;
    public ChildDeviceManager Device;
    public int Id;
    Vector2 _cameraPosition;
    HoldableEventArgs _args;
    bool _pauseInput;//, _throwAnimationStarted;
    bool _isRegrouping;
    bool _regroup;
    bool _isDead;
    public float _respawnSpeed = 7f;
    public GameObject[] playerList;
    SpriteRenderer[] _sprites;
    public GameObject closestPlayer;
    float distance;
    float closest = 1000;
    private Transform targetPlayer;
    private GameObject spawnpoint;
    public float smoothTime = 0;
    private Vector2 velocity = Vector2.zero;


    public delegate void ItemPickupEventHandler(object source, HoldableEventArgs args);
    public delegate void ItemDurabilityChangeEventHandler(object source, HoldableEventArgs args);
    public delegate void ItemClearedChangeEventHandler(object source, HoldableEventArgs args);
    public event ItemPickupEventHandler ItemPickup;
    public event ItemDurabilityChangeEventHandler ItemDurabilityChange;
    public event ItemClearedChangeEventHandler ItemCleared;

    void Start()
    {
        _cameraPosition = Vector2.zero;
        Camera.main.GetComponent<CameraMovement>().AddPlayer(this);
        _args = new HoldableEventArgs();
        _materialPropertyBlock = new MaterialPropertyBlock();
        if (_sprites == null) _sprites = GetComponentsInChildren<SpriteRenderer>(true);
        spawnpoint = GameObject.Find("Spawnpoint");
    }

    public void AssignPlayer(ChildDeviceManager device, int id)
    {
        if (Device != null) return;
        Device = device;
        Id = id;
        Color colour = GameManager.GetPlayerColor(id);
        _sprites = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sprite in _sprites)
        {
            sprite.color = colour;
        }
        RectTransform playerHealthBars = GameObject.Find("PlayerHealthBars").GetComponent<RectTransform>();
        if (playerHealthBars != null) CreateHealthBar(playerHealthBars, colour);
    }

    void CreateHealthBar(RectTransform playerHealthBars, Color colour)
    {
        GameObject healthBar = Instantiate(_playerHealthBar, Vector3.zero, Quaternion.identity, playerHealthBars);
        //Color colour = Color.white;
        //if (GameManager.Instance != null) colour = GameManager.GetPlayerColor(Id);
        healthBar.GetComponent<PlayerHealthBar>().AssignPlayer(this, _health as PlayerHealth, colour);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        OpenPauseMenu();

        RaycastHit2D cameraPosition = Physics2D.Raycast(transform.position, Vector2.down, 5f, _groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * 5f, Color.green);
        if (cameraPosition) _cameraPosition = cameraPosition.point;
        else if (transform.position.y < _cameraPosition.y || _climbingLadder) _cameraPosition = transform.position;
    }

    protected override void GetInputs()
    {
        if (Device == null) return;
        if (Device.GetMoveAnalogInput.magnitude > 0.2f) _moveInput = Device.GetMoveAnalogInput;
        else _moveInput = Device.GetMoveInput;
        _jumpInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.jump];
        _useInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.use];
        _catchInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pickup];
        _pauseInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pause];
    }

    protected override void Jump()
    {
        base.Jump();
    }

    protected override void Catch(Holdable holdable)
    {
        base.Catch(holdable);
        holdable.GetHoldableEventArgs(_args);
        ItemPickup?.Invoke(this, _args);
    }

    protected override void Throw()
    {
        _heldItem.GetHoldableEventArgs(_args);
        ItemCleared?.Invoke(this, _args);
        base.Throw();
    }

    public override void ClearHand()
    {
        _heldItem.GetHoldableEventArgs(_args);
        ItemCleared?.Invoke(this, _args);
        base.ClearHand();
    }

    public override void DurabilityChanged()
    {
        base.DurabilityChanged();
        _heldItem?.GetHoldableEventArgs(_args);
        ItemDurabilityChange?.Invoke(this, _args);
    }

    void OpenPauseMenu()
    {
        if (_pauseInput) PauseMenu.Open(this);
    }

    public Vector2 GetPlayerCameraPosition() => _cameraPosition;

    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MainCamera") && _isDead == false)
        {
            _isRegrouping = true;
            SetControl(false);
            Camera.main.GetComponent<CameraMovement>().RemovePlayer(this);
            FindClosestPlayer();
        }

        if (collision.gameObject.tag == "SpawnPivot" && _isRegrouping == true)
        {
            _isRegrouping = false;
            SetControl(true);
            Camera.main.GetComponent<CameraMovement>().AddPlayer(this);
        }  

        if (collision.gameObject.tag == "Checkpoint")
        {
            spawnpoint.transform.position = transform.position;
            Debug.Log("Spawnpoint set");
        }

        if (collision.gameObject.tag == "Player" && _isDead == true)
        {
            _isDead = false;
            Camera.main.GetComponent<CameraMovement>().AddPlayer(this);
        }
    }

    void Respawning()
    {
        if (_isRegrouping == true)
        {
            float distance = Vector3.Distance(transform.position, closestPlayer.transform.position);
            transform.position = Vector3.MoveTowards(transform.position, closestPlayer.transform.position, _respawnSpeed * Time.deltaTime);
        }

        if (_isDead == true)
        {
            StartCoroutine(IEMourn());
            Camera.main.GetComponent<CameraMovement>().RemovePlayer(this);
        }
    }

    IEnumerator IEMourn()
    {
        yield return new WaitForSeconds(5);
        transform.position = spawnpoint.transform.position;
    }

    void FindClosestPlayer()
    {
        playerList = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < playerList.Length; i++)
        {
            distance = Vector2.Distance(this.transform.position, playerList[i].transform.position);

            if (distance < closest && distance > 4.45)
            {
                closestPlayer = playerList[i];
                closest = distance;
            }
        }
    }

    void Update()
    {
        Respawning();
    }

    public override void Damaged(float iFrames)
    {
        base.Damaged(iFrames);
        StartCoroutine(IEDamaged(iFrames));
    }

    IEnumerator IEDamaged(float iFrames)
    {
        float maxInvinsiblityFrames = iFrames;
        while(iFrames > 0f)
        {
            iFrames -= Time.deltaTime;
            if (iFrames < 0f) iFrames = 0f;
            float whiteness = Mathf.Sin(32 * iFrames) / 2 + 0.5f * Mathf.Min(iFrames, 1f) * 1.4f;

            // * (maxInvinsiblityFrames - iFrames)
            SetPlayerWhiteness(whiteness);
            yield return new WaitForEndOfFrame();
        }
        SetPlayerWhiteness(0f);
    }

    void SetPlayerWhiteness(float whiteness)
    {
        foreach (SpriteRenderer sprite in _sprites)
        {
            sprite.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat("_Whiteness", whiteness);
            sprite.SetPropertyBlock(_materialPropertyBlock);
        }
    }

    public override void Die()
    {
        base.Die();
        ExitLadder();
        //_coyoteTime = 0f;
        _animator?.Play((_facingLeft ? "Left " : "Right ") + "Fall");
        SetControl(false);
        StartCoroutine(IEDamaged(1f));
        StartCoroutine(IEDie());
    }

    IEnumerator IEDie()
    {
        while (!_grounded)
        {
            CheckIfInsideGround();
            CalculateGravity();
            yield return new WaitForEndOfFrame();
        }
        _movement = Vector2.zero;
        _gravity = Vector2.zero;
        if (_facingLeft)
        {
            PlayFullBodyAnimation("Die Left");
        }
        else
        {
            PlayFullBodyAnimation("Die Right");
        }
        _isDead = true;
    }
}

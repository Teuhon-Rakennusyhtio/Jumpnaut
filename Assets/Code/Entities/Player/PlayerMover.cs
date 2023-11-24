using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerMover : GenericMover
{
    [SerializeField] GameObject _playerHealthBar;
    [SerializeField] Light2D[] _playerLights;
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
    bool _isInCutscene;
    public float _respawnSpeed = 7f;
    public GameObject[] playerList;
    SpriteRenderer[] _sprites;
    public GameObject _ClosestPlayer;
    float _closest = 1000;
    float _distance;
    private DeathManager dm;
    private Transform _targetPlayer;
    private GameObject _spawnpoint;
    public SpriteRenderer _UfoRenderer;
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
        _spawnpoint = GameObject.Find("Spawnpoint");
        _UfoRenderer.enabled = false;
        dm = GameObject.FindObjectOfType<DeathManager>();
        _ladderSongExists = GameManager.LadderSongClip != null;
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
        if (!_isInCutscene)
        {
            if (Device == null) return;
            if (Device.GetMoveAnalogInput.magnitude > 0.2f) _moveInput = Device.GetMoveAnalogInput;
            else _moveInput = Device.GetMoveInput;
            _jumpInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.jump];
            _useInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.use];
            _catchInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pickup];
            _pauseInput = Device.GetInputs[(int) ChildDeviceManager.InputTypes.pause];
        }
        
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
            _spawnpoint.transform.position = transform.position;
            dm.HearseService();
            Debug.Log("Spawnpoint set");
        }

        if (collision.gameObject.tag == "Player" && _isDead == true)
        {
            EndFullBodyAnimation();
            SetControl(true);
            _health.Heal(1);
            dm.DeathReducer(this);
            Camera.main.GetComponent<CameraMovement>().AddPlayer(this);
            _isDead = false;
        }
    }

    void Regroup()
    {
        if (_isRegrouping == true)
        {
            _UfoRenderer.enabled = true;
            float distance = Vector3.Distance(transform.position, _ClosestPlayer.transform.position);
            transform.position = Vector3.MoveTowards(transform.position, _ClosestPlayer.transform.position, _respawnSpeed * Time.deltaTime);
        }
        else if (_isRegrouping == false)
        {
            _UfoRenderer.enabled = false;
        }
    }

    void Revive()
    {
        if (_isDead == true)
        {
            FindClosestPlayer();
            
            if (dm.DeathToll(playerList.Length) == true)
            {
                StartCoroutine(IEMourn());
            }
        }
    }

    IEnumerator IEMourn()
    {
        yield return new WaitForSeconds(3);
        dm.HearseService();
        EndFullBodyAnimation();
        SetControl(true);
        _health.Heal(10);
        _helmetMain.enabled = true;
        _helmetClimb.enabled = true;
        Camera.main.GetComponent<CameraMovement>().AddPlayer(this);
        dm.DeathReducer(this);
        yield return new WaitForSeconds(4);
        _isDead = false;
    }

    void FindClosestPlayer()
    {
        playerList = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < playerList.Length; i++)
        {
            _distance = Vector2.Distance(this.transform.position, playerList[i].transform.position);

            if (_distance < _closest && _distance > 4.45)
            {
                _ClosestPlayer = playerList[i];
                _closest = _distance;
            }
        }
    }

    bool _ladderSongExists;
    void Update()
    {
        if (_ladderSongExists)
        {
            _climbingSpeed = transform.position.y > 190f ? 3f : 5f;
        }
        Regroup();
        Revive();
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
        dm.DeathCount(this);
        Camera.main.GetComponent<CameraMovement>().RemovePlayer(this);
    }

    public void SnuffOutLightForSeconds(float seconds)
    {
        float oldIntensity = _playerLights[0].intensity;
        if (oldIntensity < 0f)
            oldIntensity = 1f;
        foreach (Light2D playerLight in _playerLights)
        {
            playerLight.intensity = 0f;
        }
        StartCoroutine(IESnuffOutLightForSeconds(seconds, oldIntensity));
    }
    
    IEnumerator IESnuffOutLightForSeconds(float seconds, float oldIntensity)
    {
        yield return new WaitForSeconds(seconds);
        while (_playerLights[0].intensity < oldIntensity)
        {
            foreach (Light2D playerLight in _playerLights)
            {
                playerLight.intensity = Mathf.MoveTowards(playerLight.intensity, oldIntensity, Time.deltaTime);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void PlayCutscene(CutsceneMovement[] cutscene)
    {
        if (_isInCutscene) return;
        _isInCutscene = true;
        StartCoroutine(IEPlayCutscene(cutscene));
    }

    IEnumerator IEPlayCutscene(CutsceneMovement[] cutscene)
    {
        foreach (CutsceneMovement movement in cutscene)
        {
            if (movement.Animation != "")
                PlayFullBodyAnimation(movement.Animation);
            
            _moveInput = movement.MoveInput;
            _jumpInput = movement.JumpInput;
            _useInput = movement.UseInput;
            _catchInput = movement.CatchInput;
            yield return new WaitForSeconds(movement.Duration);
        }
        EndFullBodyAnimation();
        _moveInput = Vector2.zero;
        _jumpInput = false;
        _useInput = false;
        _catchInput = false;
        _isInCutscene = false;
    }
}

[System.Serializable]
public struct CutsceneMovement
{
    public CutsceneMovement(float duration, Vector2 moveInput, bool jumpInput, bool useInput, bool catchInput, string animation)
    {
        Duration = duration;
        MoveInput = moveInput;
        JumpInput = jumpInput;
        UseInput = useInput;
        CatchInput = catchInput;
        Animation = animation;
    }

    public CutsceneMovement(float duration, string animation)
    {
        Duration = duration;
        MoveInput = Vector2.zero;
        JumpInput = false;
        UseInput = false;
        CatchInput = false;
        Animation = animation;
    }
    public float Duration;
    public Vector2 MoveInput;
    public bool JumpInput;
    public bool UseInput;
    public bool CatchInput;
    public string Animation;
}

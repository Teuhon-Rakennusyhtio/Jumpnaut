using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : GenericMover
{
    [SerializeField] GameObject _playerHealthBar;
    [SerializeField] PlayerHealth _health;

    [SerializeField] Animator _animator;
    [SerializeField] Transform _mainRig, _leftArm, _rightArm, _climbArm;
    public ChildDeviceManager Device;
    public int Id;
    Vector2 _cameraPosition;
    bool _pauseInput;//, _throwAnimationStarted;
    public float _respawnSpeed = 7f;
    GameObject spawnPoint;
    public GameObject[] playerList;
    public GameObject closestPlayer;
    float distance;
    float closest = 1000;
    private Transform targetPlayer;
    public float smoothTime = 0;
    private Vector2 velocity = Vector2.zero;

    void Start()
    {
        spawnPoint = GameObject.Find("Spawnpoint");
        _cameraPosition = Vector2.zero;
        Camera.main.GetComponent<CameraMovement>().AddPlayer(this);
    }

    public void AssignPlayer(ChildDeviceManager device, int id)
    {
        if (Device != null) return;
        Device = device;
        Id = id;
        Color colour = GameManager.GetPlayerColor(id);
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sprite in sprites)
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
        healthBar.GetComponent<PlayerHealthBar>().AssignPlayer(this, _health, colour);
    }

    protected override void FixedUpdateLogic()
    {

        OpenPauseMenu();

        RaycastHit2D cameraPosition = Physics2D.Raycast(transform.position, Vector2.down, 5f, _groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * 5f, Color.green);
        if (cameraPosition) _cameraPosition = cameraPosition.point;
        else if (transform.position.y < _cameraPosition.y || _climbingLadder) _cameraPosition = transform.position;
        if (_animator != null) Animate(_animator);
    }

    protected override void CheckFacingLogic()
    {
        bool oldFacing = _facingLeft;
        // Figure out which direction the player is facing
        _facingLeft = _mainRig.localScale.x < 0f;

        if (oldFacing != _facingLeft) FlipLogic();
    }

    protected override void FlipLogic()
    {
        if (!_heldItemIsFlipalbe)
        {
            _handTransform.localScale = new Vector3((_facingLeft ? -1f : 1f), 1f, 1f);
        }
        if (!_holdingSomething && !_catchInput)
        {
            _animator.Play("Empty Hand");
        }
        else if (!_heldItemIsFlipalbe && _handTransform.childCount > 0)
        {
            Transform heldItem = _handTransform.GetChild(0).transform;
            heldItem.localPosition = new Vector2(-heldItem.localPosition.x, heldItem.localPosition.y);
        }
        if (_holdingOut)
        {
            if (_facingLeft)
            {
                _animator.Play("Hold Out Left", -1, 1f);
            }
            else
            {
                _animator.Play("Hold Out Right", -1, 1f);
            }
        }
        
        if (_holdingHeavyObject || !_facingLeft)
        {
            _handTransform.parent = _rightArm;
            
        }
        else
        {
            _handTransform.parent = _leftArm;
        }
        _handTransform.localPosition = Vector3.right * 0.45f;
        _handTransform.localRotation = Quaternion.identity;
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

    protected override void OnEnterLadder()
    {
        _animator.Play("Climbing");
        _handTransform.parent = _climbArm;
        _handTransform.localScale = Vector3.one;
        _handTransform.localPosition = Vector3.right * 0.45f;
        _handTransform.localRotation = Quaternion.identity;
    }

    protected override void OnExitLadder()
    {
        if (_holdingHeavyObject || !_facingLeft)
        {
            _handTransform.parent = _rightArm;
            
        }
        else
        {
            _handTransform.parent = _leftArm;
        }
        _handTransform.localPosition = Vector3.right * 0.45f;
        _handTransform.localRotation = Quaternion.identity;
    }

    protected override void OnJump()
    {
        if (_facingLeft)
        {
            _animator.Play("Left Jump Start");
        }
        else
        {
            _animator.Play("Right Jump Start");
        }
    }

    protected override void CatchLogic(Holdable holdable)
    {
        base.CatchLogic(holdable);
        if (_holdingHeavyObject)
        {
            _animator.Play("Pickup Two Handed");
        }
        else if (!_facingLeft)
        {
            _animator.Play("Pickup Right");
        }
        else
        {
            _animator.Play("Pickup Left");
        }
    }

    protected override void ThrowLogic()
    {
        //_throwAnimationStarted = true;
        //_animator.SetTrigger("Throw");
        if (!_facingLeft)
        {
            _animator.Play("Throw Right");
        }
        else
        {
            _animator.Play("Throw Left");
        }
        Throw();
    }

    protected override void ClearHandLogic()
    {
        _animator.Play("Empty Hand");
    }

    void OpenPauseMenu()
    {
        if (_pauseInput) PauseMenu.Open(this);
    }

    public Vector2 GetPlayerCameraPosition() => _cameraPosition;

    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MainCamera"))
        {
            SetControl(false);
            FindClosestPlayer();
        }

        if (collision.gameObject.tag == "SpawnPivot" && IsInControl == false)
        {
            SetControl(true);
        }  
    }

    void Respawning()
    {

    if (IsInControl == false)
        {
            float distance = Vector3.Distance(transform.position, closestPlayer.transform.position);
            transform.position = Vector3.MoveTowards(transform.position, closestPlayer.transform.position, _respawnSpeed * Time.deltaTime);
        }
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
        MoveSpawnPoint();
    }

    void MoveSpawnPoint()
    {
        targetPlayer = closestPlayer.transform;
        // Define a target position above and behind the target transform
        Vector2 targetPosition = targetPlayer.TransformPoint(new Vector2(0, 0));

        // Smoothly move the camera towards that target position
        spawnPoint.transform.position = Vector2.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    void Update()
    {
        Respawning();
    }
}

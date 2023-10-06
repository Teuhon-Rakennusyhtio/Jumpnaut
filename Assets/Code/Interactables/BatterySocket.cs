using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BatterySocket : MonoBehaviour
{
    [SerializeField] UnityEvent _batteryInserted;
    [SerializeField] SpriteRenderer _useTipBubbleGraphic, _machineSprite;
    [SerializeField] Sprite _activatedSocketSprite;
    [SerializeField] bool _preInsertedInSinglePlayer;
    List<PlayerMover> _movers;
    Transform _useTipBubble;
    Battery _battery;
    float _useTipBubbleYCoord;
    bool _batteryInSocket = false;
    
    void Start()
    {
        _movers = new List<PlayerMover>();
        _useTipBubble = _useTipBubbleGraphic.gameObject.transform;
        _useTipBubbleYCoord = _useTipBubble.position.y;
        if (GameManager.PlayerDevices.Count == 1 && _preInsertedInSinglePlayer)
        {
            InsertBattery(false);
        }
    }

    void InsertBattery(bool insertedByPlayer)
    {
        _batteryInSocket = true;
        _useTipBubbleGraphic.enabled = false;
        _machineSprite.sprite = _activatedSocketSprite;
        _batteryInserted.Invoke();

        if (!insertedByPlayer)
        {
            transform.Find("BatterySprite").GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (_batteryInSocket) return;
        PlayerMover mover = collision.gameObject.GetComponent<PlayerMover>();
        if (mover != null) _movers.Add(mover);
        _useTipBubbleGraphic.enabled = CheckForBattery();
        UpdateBubblePosition();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        PlayerMover mover = collision.gameObject.GetComponent<PlayerMover>();
        if (mover != null) _movers.Remove(mover);
        _useTipBubbleGraphic.enabled = CheckForBattery();
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (_movers.Count == 0 || _batteryInSocket) return;
        

        PlayerMover mover = collision.GetComponentInChildren<PlayerMover>();
        if (mover != null && mover.MoveInput.y > 0.3f)
        {
            _battery = mover.GetComponentInChildren<Battery>();
        }
        if (_battery != null)
        {
            _battery.PlaceInSocket(transform);
            InsertBattery(true);
        }
        UpdateBubblePosition();
    }

    void UpdateBubblePosition()
    {
        _useTipBubble.position =
            new Vector2(_useTipBubble.position.x,
            _useTipBubbleYCoord +
            Mathf.Sin(Time.timeSinceLevelLoad * 3) * 0.1f);
    }

    bool CheckForBattery()
    {
        bool batteryIsNear = false;
        foreach (PlayerMover mover in _movers)
        {
            if (!batteryIsNear)
            {
                batteryIsNear = mover.GetComponentInChildren<Battery>() != null;
            }
        }
        return batteryIsNear;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

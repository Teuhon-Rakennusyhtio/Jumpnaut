using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BatterySocket : MonoBehaviour
{
    [SerializeField] UnityEvent _batteryInserted;
    List<PlayerMover> _movers;
    Battery _battery;
    bool _batteryInSocket = false;
    
    void Start()
    {
        _movers = new List<PlayerMover>();
        if (GameManager.PlayerDevices.Count == 1)
        {
            _batteryInserted.Invoke();
            transform.Find("BatterySprite").GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMover mover = collision.gameObject.GetComponent<PlayerMover>();
        if (mover != null) _movers.Add(mover);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        PlayerMover mover = collision.gameObject.GetComponent<PlayerMover>();
        if (mover != null) _movers.Remove(mover);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (_movers.Count == 0 || _batteryInSocket) return;
        
        foreach (PlayerMover mover in _movers)
        {
            if (mover.MoveInput.y > 0.3f || _battery != null)
            {
                _battery = mover.GetComponentInChildren<Battery>();
            }
        }
        if (_battery != null)
        {
            _battery.PlaceInSocket(transform);
            _batteryInSocket = true;
            _batteryInserted.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

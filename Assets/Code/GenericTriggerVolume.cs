using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericTriggerVolume : MonoBehaviour
{
    [SerializeField] UnityEvent _enterTrigger, _exitTrigger;
    [SerializeField] bool _oneTimeUse;
    bool _entered, _exited;

    void Start()
    {

    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && !(_oneTimeUse && _entered))
        {
            _enterTrigger.Invoke();
            _entered = true;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && !(_oneTimeUse && _exited))
        {
            _exitTrigger.Invoke();
            _exited = true;
        }
    }
}

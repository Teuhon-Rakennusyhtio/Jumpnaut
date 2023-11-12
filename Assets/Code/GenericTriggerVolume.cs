using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericTriggerVolume : MonoBehaviour
{
    [SerializeField] UnityEvent _enterTrigger, _exitTrigger;
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            _enterTrigger.Invoke();
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            _exitTrigger.Invoke();
        }
    }
}

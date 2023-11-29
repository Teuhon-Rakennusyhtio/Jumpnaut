using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericTriggerVolume : MonoBehaviour
{
    [SerializeField] UnityEvent _enterTrigger, _exitTrigger;
    [SerializeField] bool _oneTimeUse;
    bool _entered, _exited;
    AudioManager _audioManager;

    void Start()
    {
        GameObject audioManagerGameObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioManagerGameObject != null)
            _audioManager = audioManagerGameObject.GetComponent<AudioManager>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && !(_oneTimeUse && _entered))
        {
            _enterTrigger.Invoke();
            _entered = true;
            //_audioManager?.PlaySFX(_audioManager.checkpoint);
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

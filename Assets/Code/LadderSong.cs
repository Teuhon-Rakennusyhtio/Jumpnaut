using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderSong : MonoBehaviour
{
    bool _songStarted = false;
    AudioSource _audioSource;
    Transform _cameraTransform;
    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.LadderSong == null)
        {
            Destroy(gameObject);
        }
        else
        {
            _cameraTransform = Camera.main.transform;
            _audioSource = GetComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && !_songStarted)
        {
            _songStarted = true;
            _audioSource.PlayOneShot(GameManager.LadderSong);
        }
    }

    void Update()
    {
        transform.position = _cameraTransform.position;
    }
}

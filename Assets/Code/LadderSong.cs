using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class LadderSong : MonoBehaviour
{
    bool _songStarted = false;
    AudioSource _audioSource;
    Transform _cameraTransform;
    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.LadderSongClip == null)
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
            _audioSource.PlayOneShot(GameManager.LadderSongClip);
        }
    }

    void Update()
    {
        if (_songStarted)
            transform.position = _cameraTransform.position;
    }

    public static IEnumerator GetSong()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "LadderSong.ogg");
        if (File.Exists(path))
        {
            path = "file://" + path;
            using (UnityWebRequest songFile = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS))
            {
                yield return songFile.SendWebRequest();

                if (songFile.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(songFile.error);
                }
                else
                {
                    GameManager.LadderSongClip = DownloadHandlerAudioClip.GetContent(songFile);
                }
            }

        }
        //song = null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header(" - - - Audio Source - - - ")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header(" - - - Audio Clip - - - ")]
    public AudioClip background;
    public AudioClip title;
    public AudioClip jump;
    public AudioClip damage;
    public AudioClip playerDeath;
    public AudioClip enemyDeath;
    public AudioClip objectBreak;
    public AudioClip explosion;

    private void Start()
    {
        //musicSource.clip = background;
        //musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void PlayMenuMusic()
    {
        StopAllMusic();
        musicSource.clip = title;
        musicSource.Play();
    }

    public void PlayStageMusic()
    {
        StopAllMusic();
        musicSource.clip = background;
        musicSource.Play();
    }

    public void StopAllMusic()
    {
        musicSource.Stop();
    }
}

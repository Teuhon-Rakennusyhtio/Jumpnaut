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
    public AudioClip win;
    public AudioClip winsfx;
    public AudioClip jump;
    public AudioClip damage;
    public AudioClip playerDeath;
    public AudioClip enemyDeath;
    public AudioClip r4x4Death;
    public AudioClip objectBreak;
    public AudioClip explosion;
    public AudioClip batteryInsert;
    public AudioClip batteryGenerator;
    public AudioClip hammerHit;
    public AudioClip pilarBreak;
    public AudioClip checkpoint;
    public AudioClip throwsfx;
    public AudioClip buttonON;
    public AudioClip buttonOFF;
    public AudioClip sawOn;
    public AudioClip sawOff;
    public AudioClip sawHit;

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

    public void PlayWinMusic()
    {
        StopAllMusic();
        musicSource.clip = win;
        musicSource.Play();
    }

    public void ManualClapStop()
    {
        SFXSource.Stop();
    }

    public void StopAllMusic()
    {
        musicSource.Stop();
    }
}

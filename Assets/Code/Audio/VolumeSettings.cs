using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixeriino;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Slider masterSlider;

    void Start()
    {
        SetMusicVolume();
        SetSFXVolume();
        SetMasterVolume();
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        mixeriino.SetFloat("music", Mathf.Log10(volume)*20);
    }

    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        mixeriino.SetFloat("SFX", Mathf.Log10(volume)*20);
    }

    public void SetMasterVolume()
    {
        float volume = masterSlider.value;
        mixeriino.SetFloat("Master", Mathf.Log10(volume)*20);
    }
}

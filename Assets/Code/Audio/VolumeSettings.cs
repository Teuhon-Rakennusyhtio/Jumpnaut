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

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string MasterVolumeKey = "MasterVolume";

    void Start()
    {
        LoadVolumeSettings();
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        mixeriino.SetFloat("music", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        mixeriino.SetFloat("SFX", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat(SFXVolumeKey, volume);
    }

    public void SetMasterVolume()
    {
        float volume = masterSlider.value;
        mixeriino.SetFloat("Master", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
    }

    private void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey(MusicVolumeKey))
        {
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey);
            musicSlider.value = musicVolume;
            SetMusicVolume(); // Apply the loaded volume
        }

        if (PlayerPrefs.HasKey(SFXVolumeKey))
        {
            float sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey);
            SFXSlider.value = sfxVolume;
            SetSFXVolume(); // Apply the loaded volume
        }

        if (PlayerPrefs.HasKey(MasterVolumeKey))
        {
            float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey);
            masterSlider.value = masterVolume;
            SetMasterVolume(); // Apply the loaded volume
        }
    }
}

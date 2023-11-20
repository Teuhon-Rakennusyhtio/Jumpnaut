using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixeriino;
    [SerializeField] private Slider musicSlider;

    void Start()
    {
        SetMusicVolume();
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        mixeriino.SetFloat("music", Mathf.Log10(volume)*20);
    }
}

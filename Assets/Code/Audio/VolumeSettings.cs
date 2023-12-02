using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixeriino;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Slider masterSlider;
    private float scalingFactor = 50f;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string MasterVolumeKey = "MasterVolume";

    void Start()
    {
        LoadVolumeSettings();
    }

    void Update()
    {
        // Check for horizontal input and update the selected slider accordingly
        UpdateSliderInput();
    }

    private void UpdateSliderInput()
    {
        // Check if any UI element is currently selected
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            Slider selectedSlider = EventSystem.current.currentSelectedGameObject.GetComponent<Slider>();

            // Check if the selected UI element is a Slider
            if (selectedSlider != null)
            {
                float horizontalInput = Input.GetAxis("Horizontal");

                if (Mathf.Abs(horizontalInput) > 0.1f)
                {
                    selectedSlider.value += horizontalInput * Time.deltaTime;
                    selectedSlider.value = Mathf.Clamp01(selectedSlider.value); // Ensure the value stays between 0 and 1

                    // Call the corresponding volume setting method
                    if (selectedSlider == musicSlider)
                    {
                        SetMusicVolume();
                    }
                    else if (selectedSlider == SFXSlider)
                    {
                        SetSFXVolume();
                    }
                    else if (selectedSlider == masterSlider)
                    {
                        SetMasterVolume();
                    }
                }
            }
        }
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        mixeriino.SetFloat("music", Mathf.Log10(volume) * scalingFactor);
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        mixeriino.SetFloat("SFX", Mathf.Log10(volume) * scalingFactor);
        PlayerPrefs.SetFloat(SFXVolumeKey, volume);
    }

    public void SetMasterVolume()
    {
        float volume = masterSlider.value;
        mixeriino.SetFloat("Master", Mathf.Log10(volume) * scalingFactor);
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
            SetSFXVolume();
        }

        if (PlayerPrefs.HasKey(MasterVolumeKey))
        {
            float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey);
            masterSlider.value = masterVolume;
            SetMasterVolume();
        }
    }
}

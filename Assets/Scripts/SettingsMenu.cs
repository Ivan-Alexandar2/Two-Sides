using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;

    [Header("UI References")]
    public Slider masterSlider;
    public Slider ambienceSlider;
    public Slider sfxSlider;

    private void Start()
    {
        LoadVolume();

        // Load saved values or default to 1 (Max volume)
        
        //float masterVal = PlayerPrefs.GetFloat("MasterVol", 1f);
        //float ambienceVal = PlayerPrefs.GetFloat("AmbienceVol", 1f);
        //float sfxVal = PlayerPrefs.GetFloat("SFXVol", 0.5f);

        // Update the sliders visually
        //masterSlider.value = masterVal;
        //ambienceSlider.value = ambienceVal;
        //sfxSlider.value = sfxVal;

        // Apply the volume immediately
        // SetMasterVolume(masterVal);
        //SetAmbienceVolume(ambienceVal);
        //SetSFXVolume(sfxVal);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVol", volume);
    }

    public void SetAmbienceVolume(float volume)
    {
        audioMixer.SetFloat("AmbienceVol", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVol", volume);

    }

    public void SaveVolume() // saves the values when I press X
    {
        audioMixer.GetFloat("MasterVol", out float masterVolume);
        audioMixer.SetFloat("MasterVol", masterVolume);

        audioMixer.GetFloat("AmbienceVol", out float ambienceVolume);
        audioMixer.SetFloat("AmbienceVol", ambienceVolume);

        audioMixer.GetFloat("SFXVol", out float sfxVolume);
        audioMixer.SetFloat("SFXVol", sfxVolume);
    }

    public void LoadVolume()
    {
        // PlayerPrefs is a class that stores Player preferences between game sessions.
        masterSlider.value = PlayerPrefs.GetFloat("MasterVol");
        ambienceSlider.value = PlayerPrefs.GetFloat("AmbienceVol");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVol");
    }

    #region FULL SCREEN
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    #endregion

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}

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
        LoadFullscreen();
        LoadQuality();

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
        PlayerPrefs.SetFloat("MasterVol", volume);
        PlayerPrefs.Save();
    }

    public void SetAmbienceVolume(float volume)
    {
        audioMixer.SetFloat("AmbienceVol", volume);
        PlayerPrefs.SetFloat("AmbienceVol", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVol", volume);
        PlayerPrefs.SetFloat("SFXVol", volume);
        PlayerPrefs.Save();

    }

    public void LoadVolume()
    {
        // Load saved values or default to 0 (middle value for mixer, usually 0dB)
        float masterVol = PlayerPrefs.GetFloat("MasterVol", 0f);
        float ambienceVol = PlayerPrefs.GetFloat("AmbienceVol", 0f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 0f);

        // Update sliders
        masterSlider.value = masterVol;
        ambienceSlider.value = ambienceVol;
        sfxSlider.value = sfxVol;

        // Apply to mixer
        audioMixer.SetFloat("MasterVol", masterVol);
        audioMixer.SetFloat("AmbienceVol", ambienceVol);
        audioMixer.SetFloat("SFXVol", sfxVol);
    }

    #region FULL SCREEN
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadFullscreen()
    {
        int isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1);
        Screen.fullScreen = (isFullscreen == 1);
    }
    #endregion

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
        PlayerPrefs.Save();
    }

    private void LoadQuality()
    {
        int quality = PlayerPrefs.GetInt("QualityLevel", 2); // Default to medium quality
        QualitySettings.SetQualityLevel(quality);
    }
}

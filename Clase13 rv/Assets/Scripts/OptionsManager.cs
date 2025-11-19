using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

/// <summary>
/// Gestor de opciones del juego
/// Controla volumen, calidad gráfica y otras configuraciones
/// </summary>
public class OptionsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("VR Settings")]
    [SerializeField] private Slider smoothTurnSpeedSlider;
    [SerializeField] private Toggle snapTurnToggle;
    [SerializeField] private Slider comfortVignetteSlider;
    [SerializeField] private TextMeshProUGUI turnSpeedText;

    private Resolution[] resolutions;
    private bool isMuted = false;
    private float lastMasterVolume = 0.75f;

    private void Start()
    {
        LoadSettings();
        SetupResolutions();
        SetupQualityLevels();
        SetupAudioControls();
        SetupVRControls();
    }

    #region Audio Controls

    private void SetupAudioControls()
    {
        // Configurar sliders
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (muteToggle != null)
        {
            muteToggle.onValueChanged.AddListener(SetMute);
        }
    }

    public void SetMasterVolume(float volume)
    {
        if (audioMixer != null)
        {
            // Convertir de escala lineal (0-1) a escala logarítmica (dB)
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat("MasterVolume", dB);
        }

        if (masterVolumeText != null)
        {
            masterVolumeText.text = $"{Mathf.RoundToInt(volume * 100)}%";
        }

        lastMasterVolume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat("MusicVolume", dB);
        }

        if (musicVolumeText != null)
        {
            musicVolumeText.text = $"{Mathf.RoundToInt(volume * 100)}%";
        }

        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat("SFXVolume", dB);
        }

        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = $"{Mathf.RoundToInt(volume * 100)}%";
        }

        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetMute(bool muted)
    {
        isMuted = muted;

        if (muted)
        {
            // Silenciar todo
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MasterVolume", -80f);
            }
            AudioListener.volume = 0f;
        }
        else
        {
            // Restaurar volumen
            if (masterVolumeSlider != null)
            {
                SetMasterVolume(masterVolumeSlider.value);
            }
            AudioListener.volume = 1f;
        }

        PlayerPrefs.SetInt("Muted", muted ? 1 : 0);
    }

    /// <summary>
    /// Botón rápido para silenciar/activar sonido
    /// </summary>
    public void ToggleMute()
    {
        if (muteToggle != null)
        {
            muteToggle.isOn = !muteToggle.isOn;
        }
        else
        {
            SetMute(!isMuted);
        }
    }

    #endregion

    #region Graphics Settings

    private void SetupQualityLevels()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }

    private void SetupResolutions()
    {
        if (resolutionDropdown == null) return;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio.value.ToString("F0") + "Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    #endregion

    #region VR Settings

    private void SetupVRControls()
    {
        if (smoothTurnSpeedSlider != null)
        {
            smoothTurnSpeedSlider.onValueChanged.AddListener(SetTurnSpeed);
        }

        if (snapTurnToggle != null)
        {
            snapTurnToggle.onValueChanged.AddListener(SetSnapTurn);
        }

        if (comfortVignetteSlider != null)
        {
            comfortVignetteSlider.onValueChanged.AddListener(SetComfortVignette);
        }
    }

    public void SetTurnSpeed(float speed)
    {
        if (turnSpeedText != null)
        {
            turnSpeedText.text = $"{Mathf.RoundToInt(speed)}°/s";
        }

        PlayerPrefs.SetFloat("TurnSpeed", speed);
    }

    public void SetSnapTurn(bool enabled)
    {
        PlayerPrefs.SetInt("SnapTurn", enabled ? 1 : 0);
    }

    public void SetComfortVignette(float intensity)
    {
        PlayerPrefs.SetFloat("ComfortVignette", intensity);
    }

    #endregion

    #region Save/Load Settings

    private void LoadSettings()
    {
        // Audio
        if (masterVolumeSlider != null)
        {
            float volume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            masterVolumeSlider.value = volume;
            SetMasterVolume(volume);
        }

        if (musicVolumeSlider != null)
        {
            float volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            musicVolumeSlider.value = volume;
            SetMusicVolume(volume);
        }

        if (sfxVolumeSlider != null)
        {
            float volume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            sfxVolumeSlider.value = volume;
            SetSFXVolume(volume);
        }

        if (muteToggle != null)
        {
            bool muted = PlayerPrefs.GetInt("Muted", 0) == 1;
            muteToggle.isOn = muted;
            SetMute(muted);
        }

        // Graphics
        if (qualityDropdown != null)
        {
            int quality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
            qualityDropdown.value = quality;
        }

        if (fullscreenToggle != null)
        {
            bool fullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            fullscreenToggle.isOn = fullscreen;
        }

        // VR
        if (smoothTurnSpeedSlider != null)
        {
            float turnSpeed = PlayerPrefs.GetFloat("TurnSpeed", 90f);
            smoothTurnSpeedSlider.value = turnSpeed;
        }

        if (snapTurnToggle != null)
        {
            bool snapTurn = PlayerPrefs.GetInt("SnapTurn", 0) == 1;
            snapTurnToggle.isOn = snapTurn;
        }

        if (comfortVignetteSlider != null)
        {
            float vignette = PlayerPrefs.GetFloat("ComfortVignette", 0.5f);
            comfortVignetteSlider.value = vignette;
        }
    }

    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteAll();
        LoadSettings();
    }

    #endregion
}

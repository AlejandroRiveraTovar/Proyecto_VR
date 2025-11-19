using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Gestor del menú de pausa durante el juego
/// Permite pausar, reanudar, acceder a opciones y volver al menú
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject pauseOptionsPanel;
    [SerializeField] private GameObject inGameUI;

    [Header("Configuración")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Audio")]
    [SerializeField] private AudioSource pauseAudioSource;
    [SerializeField] private AudioClip pauseSound;
    [SerializeField] private AudioClip resumeSound;

    [Header("VR Controls")]
    [SerializeField] private bool enableVRPause = true;
    [SerializeField] private UnityEngine.InputSystem.InputActionReference pauseActionVR;

    private bool isPaused = false;
    private float originalTimeScale = 1f;

    private void Start()
    {
        // Asegurar que el menú está oculto al inicio
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (pauseOptionsPanel != null)
        {
            pauseOptionsPanel.SetActive(false);
        }

        // Configurar Input System si está disponible
        SetupVRInput();
    }

    private void Update()
    {
        // Detectar tecla de pausa (teclado)
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    private void SetupVRInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (enableVRPause && pauseActionVR != null)
        {
            pauseActionVR.action.performed += ctx => TogglePause();
        }
#endif
    }

    #region Pause Control

    /// <summary>
    /// Alternar entre pausado y no pausado
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Pausar el juego
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Mostrar menú de pausa
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Ocultar UI del juego
        if (inGameUI != null)
        {
            inGameUI.SetActive(false);
        }

        // Mostrar cursor si no es VR
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Pausar GameManager si existe
        if (AdvancedGameManager.Instance != null)
        {
            AdvancedGameManager.Instance.SetTimerState(false);
        }

        PlaySound(pauseSound);

        Debug.Log("[PauseMenu] Juego pausado");
    }

    /// <summary>
    /// Reanudar el juego
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = originalTimeScale;

        // Ocultar menú de pausa
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (pauseOptionsPanel != null)
        {
            pauseOptionsPanel.SetActive(false);
        }

        // Mostrar UI del juego
        if (inGameUI != null)
        {
            inGameUI.SetActive(true);
        }

        // Ocultar cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Reanudar GameManager si existe
        if (AdvancedGameManager.Instance != null)
        {
            AdvancedGameManager.Instance.SetTimerState(true);
        }

        PlaySound(resumeSound);

        Debug.Log("[PauseMenu] Juego reanudado");
    }

    #endregion

    #region Menu Navigation

    /// <summary>
    /// Botón: Reanudar
    /// </summary>
    public void OnResumeButton()
    {
        ResumeGame();
    }

    /// <summary>
    /// Botón: Opciones desde pausa
    /// </summary>
    public void OnPauseOptions()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (pauseOptionsPanel != null)
        {
            pauseOptionsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Botón: Volver al menú de pausa desde opciones
    /// </summary>
    public void OnBackToPauseMenu()
    {
        if (pauseOptionsPanel != null)
        {
            pauseOptionsPanel.SetActive(false);
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Botón: Reiniciar nivel actual
    /// </summary>
    public void OnRestartLevel()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    /// <summary>
    /// Botón: Volver al menú principal
    /// </summary>
    public void OnMainMenu()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(0); // Cargar primera escena
        }
    }

    /// <summary>
    /// Botón: Salir del juego
    /// </summary>
    public void OnQuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    #endregion

    #region Utilities

    private void PlaySound(AudioClip clip)
    {
        if (pauseAudioSource != null && clip != null)
        {
            // Usar unscaledTime para que suene incluso pausado
            pauseAudioSource.PlayOneShot(clip);
        }
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    private void OnDestroy()
    {
        // Asegurar que el tiempo vuelva a la normalidad
        Time.timeScale = 1f;

#if ENABLE_INPUT_SYSTEM
        if (pauseActionVR != null)
        {
            pauseActionVR.action.performed -= ctx => TogglePause();
        }
#endif
    }

    #endregion
}
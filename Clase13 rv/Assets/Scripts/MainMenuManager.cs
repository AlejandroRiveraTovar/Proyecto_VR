using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Gestor del menú principal del juego
/// Controla navegación entre paneles y carga de escenas
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles del Menú")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject experienceSelectionPanel;

    [Header("Audio")]
    [SerializeField] private AudioSource menuAudioSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;

    [Header("Configuración de Escenas")]
    [SerializeField] private string TallerAutos = "TallerAutos";
    

    [Header("Animaciones")]
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private float transitionDelay = 0.3f;

    private void Start()
    {
        // Mostrar solo el panel principal al inicio
        ShowPanel(mainMenuPanel);

        // Configurar cursor si no es VR
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    #region Navegación de Menú

    /// <summary>
    /// Botón: Comenzar Juego - Muestra selección de experiencias
    /// </summary>
    public void OnStartGame()
    {
        PlayButtonSound();
        ShowPanel(experienceSelectionPanel);
    }

    /// <summary>
    /// Botón: Opciones
    /// </summary>
    public void OnOptions()
    {
        PlayButtonSound();
        ShowPanel(optionsPanel);
    }

    /// <summary>
    /// Botón: Cómo se Juega
    /// </summary>
    public void OnHowToPlay()
    {
        PlayButtonSound();
        ShowPanel(howToPlayPanel);
    }

    /// <summary>
    /// Botón: Volver al Menú Principal
    /// </summary>
    public void OnBackToMainMenu()
    {
        PlayButtonSound();
        ShowPanel(mainMenuPanel);
    }

    /// <summary>
    /// Botón: Salir del Juego
    /// </summary>
    public void OnQuitGame()
    {
        PlayButtonSound();
        Debug.Log("Saliendo del juego...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    #endregion

    #region Carga de Experiencias

    /// <summary>
    /// Cargar experiencia de Cambio de Aceite
    /// </summary>
    public void TallerAutosVR()
    {
        PlayButtonSound();
        LoadScene(TallerAutos);
    }

    /// <summary>
    /// Cargar experiencia de Cambio de Frenos
    /// </summary>
   

    private void LoadScene(string sceneName)
    {
        // Opcional: Mostrar pantalla de carga
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        yield return new WaitForSeconds(transitionDelay);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Puedes agregar una barra de progreso aquí
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            // Actualizar UI de carga si existe
            yield return null;
        }
    }

    #endregion

    #region Gestión de Paneles

    private void ShowPanel(GameObject panelToShow)
    {
        // Ocultar todos los paneles
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (experienceSelectionPanel != null) experienceSelectionPanel.SetActive(false);

        // Mostrar el panel seleccionado
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }
    }

    #endregion

    #region Audio

    private void PlayButtonSound()
    {
        if (menuAudioSource != null && buttonClickSound != null)
        {
            menuAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    public void PlayHoverSound()
    {
        if (menuAudioSource != null && buttonHoverSound != null)
        {
            menuAudioSource.PlayOneShot(buttonHoverSound);
        }
    }

    #endregion
}

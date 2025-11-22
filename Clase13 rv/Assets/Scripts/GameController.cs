using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controlador principal del juego con timer de 10 minutos
/// Maneja Victory y Game Over
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("Timer Configuration")]
    [SerializeField] private float totalTime = 600f; // 10 minutos en segundos
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerFillBar;
    [SerializeField] private Color normalTimeColor = Color.white;
    [SerializeField] private Color warningTimeColor = Color.yellow;
    [SerializeField] private Color criticalTimeColor = Color.red;
    [SerializeField] private float warningTimeThreshold = 180f; // 3 minutos
    [SerializeField] private float criticalTimeThreshold = 60f; // 1 minuto

    [Header("Canvas Screens")]
    [SerializeField] private GameObject victoryCanvas;
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject inGameUI;

    [Header("Victory Screen")]
    [SerializeField] private TextMeshProUGUI victoryTimeText;
    [SerializeField] private TextMeshProUGUI victoryScoreText;
    [SerializeField] private TextMeshProUGUI victoryRankText;

    [Header("Game Over Screen")]
    [SerializeField] private TextMeshProUGUI gameOverReasonText;
    [SerializeField] private TextMeshProUGUI gameOverStatsText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip warningBeep;
    [SerializeField] private AudioClip criticalBeep;

    [Header("References")]
    [SerializeField] private InteractiveZoneSystem zoneSystem;

    // Estado del juego
    private float currentTime;
    private bool gameActive = true;
    private bool allTasksCompleted = false;
    private float completionTime;
    private bool warningPlayed = false;
    private bool criticalPlayed = false;

    private void Start()
    {
        InitializeGame();
    }

    private void Update()
    {
        if (gameActive)
        {
            UpdateTimer();
        }
    }

    private void InitializeGame()
    {
        // Configurar tiempo inicial
        currentTime = totalTime;
        gameActive = true;
        allTasksCompleted = false;
        warningPlayed = false;
        criticalPlayed = false;

        // Ocultar pantallas de resultado
        if (victoryCanvas != null) victoryCanvas.SetActive(false);
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        if (inGameUI != null) inGameUI.SetActive(true);

        // Actualizar timer inicial
        UpdateTimerDisplay();

        // Buscar zona system si no está asignado
        if (zoneSystem == null)
        {
            zoneSystem = FindObjectOfType<InteractiveZoneSystem>();
        }

        Debug.Log("[GameController] Juego iniciado - Tiempo: 10 minutos");
    }

    private void UpdateTimer()
    {
        // Decrementar tiempo
        currentTime -= Time.deltaTime;

        // Verificar si se acabó el tiempo
        if (currentTime <= 0)
        {
            currentTime = 0;
            OnTimeUp();
            return;
        }

        // Actualizar display
        UpdateTimerDisplay();

        // Alertas de tiempo
        CheckTimeWarnings();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";

            // Color según tiempo restante
            if (currentTime <= criticalTimeThreshold)
            {
                timerText.color = criticalTimeColor;
            }
            else if (currentTime <= warningTimeThreshold)
            {
                timerText.color = warningTimeColor;
            }
            else
            {
                timerText.color = normalTimeColor;
            }
        }

        // Barra de progreso del tiempo
        if (timerFillBar != null)
        {
            float fillAmount = currentTime / totalTime;
            timerFillBar.fillAmount = fillAmount;

            // Color de la barra
            if (currentTime <= criticalTimeThreshold)
            {
                timerFillBar.color = criticalTimeColor;
            }
            else if (currentTime <= warningTimeThreshold)
            {
                timerFillBar.color = warningTimeColor;
            }
            else
            {
                timerFillBar.color = normalTimeColor;
            }
        }
    }

    private void CheckTimeWarnings()
    {
        // Advertencia a 3 minutos
        if (currentTime <= warningTimeThreshold && !warningPlayed)
        {
            warningPlayed = true;
            PlaySound(warningBeep);
            ShowWarningMessage("¡3 MINUTOS RESTANTES!");
            Debug.LogWarning("[GameController] ¡3 minutos restantes!");
        }

        // Advertencia crítica a 1 minuto
        if (currentTime <= criticalTimeThreshold && !criticalPlayed)
        {
            criticalPlayed = true;
            PlaySound(criticalBeep);
            ShowWarningMessage("¡1 MINUTO RESTANTE!");
            Debug.LogWarning("[GameController] ¡1 minuto restante!");
        }

        // Beep cada segundo en los últimos 10 segundos
        if (currentTime <= 10f && currentTime > 0)
        {
            int currentSecond = Mathf.FloorToInt(currentTime);
            int previousSecond = Mathf.FloorToInt(currentTime + Time.deltaTime);

            if (currentSecond != previousSecond)
            {
                PlaySound(criticalBeep);
            }
        }
    }

    private void ShowWarningMessage(string message)
    {
        // Mostrar mensaje temporal de advertencia
        InGameUIManager uiManager = FindObjectOfType<InGameUIManager>();
        if (uiManager != null)
        {
            uiManager.ShowFeedback(message, InGameUIManager.FeedbackType.Error);
        }
    }

    /// <summary>
    /// Llamado cuando se completan todas las tareas
    /// </summary>
    public void OnAllTasksCompleted()
    {
        if (!gameActive || allTasksCompleted) return;

        allTasksCompleted = true;
        completionTime = totalTime - currentTime;
        gameActive = false;

        ShowVictoryScreen();
    }

    private void OnTimeUp()
    {
        if (!gameActive) return;

        gameActive = false;

        // Si completó todas las tareas → victoria aunque llegue justo al final
        if (zoneSystem != null && zoneSystem.AllZonesCompleted())
        {
            completionTime = totalTime; // completó al límite exacto
            ShowVictoryScreen();
            Debug.Log("[GameController] ¡Victoria justo a tiempo!");
            return;
        }

        // Si NO completó → Game Over
        ShowGameOverScreen("¡TIEMPO AGOTADO!");
        Debug.Log("[GameController] Game Over - Tiempo agotado");
    }

    private void ShowVictoryScreen()
    {
        // Ocultar UI del juego
        if (inGameUI != null) inGameUI.SetActive(false);

        // Mostrar pantalla de victoria
        if (victoryCanvas != null) victoryCanvas.SetActive(true);

        // Pausar juego
        Time.timeScale = 0f;

        // Actualizar textos de victoria
        if (victoryTimeText != null)
        {
            int minutes = Mathf.FloorToInt(completionTime / 60f);
            int seconds = Mathf.FloorToInt(completionTime % 60f);
            victoryTimeText.text = $"Tiempo: {minutes:00}:{seconds:00}";
        }

        // Calcular puntuación
        int finalScore = CalculateFinalScore();
        if (victoryScoreText != null)
        {
            victoryScoreText.text = $"Puntuación: {finalScore}";
        }

        // Calcular rango
        string rank = CalculateRank(completionTime);
        if (victoryRankText != null)
        {
            victoryRankText.text = $"Rango: {rank}";
        }

        // Sonido de victoria
        PlaySound(victorySound);

        // Registrar en AdvancedGameManager si existe
        if (AdvancedGameManager.Instance != null)
        {
            AdvancedGameManager.Instance.RegisterCompletion("TallerAutos", finalScore);
        }

        Debug.Log($"[GameController] ¡VICTORIA! Tiempo: {completionTime:F2}s - Puntuación: {finalScore}");
    }

    private void ShowGameOverScreen(string reason)
    {
        // Ocultar UI del juego
        if (inGameUI != null) inGameUI.SetActive(false);

        // Mostrar pantalla de game over
        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);

        // Pausar juego
        Time.timeScale = 0f;

        // Actualizar razón de game over
        if (gameOverReasonText != null)
        {
            gameOverReasonText.text = reason;
        }

        // Estadísticas
        if (gameOverStatsText != null)
        {
            int completedTasks = zoneSystem != null ? zoneSystem.GetCompletedZonesCount() : 0;
            int totalTasks = zoneSystem != null ? zoneSystem.GetTotalZonesCount() : 5;

            gameOverStatsText.text = $"Tareas Completadas: {completedTasks}/{totalTasks}\n";

            if (AdvancedGameManager.Instance != null)
            {
                int errors = AdvancedGameManager.Instance.GetErrorCount();
                gameOverStatsText.text += $"Errores: {errors}";
            }
        }

        // Sonido de game over
        PlaySound(gameOverSound);

        Debug.Log($"[GameController] Game Over - Razón: {reason}");
    }

    private int CalculateFinalScore()
    {
        int score = 1000; // Puntuación base

        // Bonus por tiempo sobrante (1 punto por segundo)
        score += Mathf.RoundToInt(currentTime);

        // Bonus por velocidad de completado
        if (completionTime < 300f) // Menos de 5 minutos
        {
            score += 500;
        }
        else if (completionTime < 420f) // Menos de 7 minutos
        {
            score += 300;
        }

        // Incluir puntuación del AdvancedGameManager
        if (AdvancedGameManager.Instance != null)
        {
            score += AdvancedGameManager.Instance.GetCurrentScore();
        }

        return score;
    }

    private string CalculateRank(float time)
    {
        if (time < 300f) return "S - ¡EXCELENTE!";
        if (time < 420f) return "A - ¡MUY BIEN!";
        if (time < 540f) return "B - BIEN";
        return "C - COMPLETADO";
    }

    #region Botones de UI

    /// <summary>
    /// Botón: Reiniciar nivel
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Botón: Volver al menú principal
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Botón: Siguiente nivel
    /// </summary>
    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        // Implementar lógica de siguiente nivel
        Debug.Log("[GameController] Cargar siguiente nivel");
    }

    /// <summary>
    /// Botón: Salir del juego
    /// </summary>
    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    #endregion

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }

    public bool IsGameActive()
    {
        return gameActive;
    }

    private void OnDestroy()
    {
        // Asegurar que el tiempo vuelva a normal
        Time.timeScale = 1f;
    }
}

using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Gestor avanzado de gamificación para experiencias VR
/// Maneja scoring, tiempo, precisión y retroalimentación
/// NOTA: Este es un GameManager alternativo/adicional al existente
/// </summary>
public class AdvancedGameManager : MonoBehaviour
{
    public static AdvancedGameManager Instance { get; private set; }

    [Header("Sistema de Puntuación")]
    [SerializeField] private int baseScore = 100;
    [SerializeField] private int timeBonus = 50;
    [SerializeField] private int precisionBonus = 30;
    [SerializeField] private int errorPenalty = -10;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI errorCountText;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI feedbackMessageText;

    [Header("Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip pointsGainedSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip completionSound;

    // Estado actual
    private int currentScore = 0;
    private int errorCount = 0;
    private float elapsedTime = 0f;
    private bool isTimerRunning = false;
    private string currentExperience = "";

    private Dictionary<string, ExperienceData> experienceHistory;

    [System.Serializable]
    public class ExperienceData
    {
        public string name;
        public float completionTime;
        public int score;
        public int errors;
        public System.DateTime timestamp;
    }

    private void Awake()
    {
        // Patrón Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            experienceHistory = new Dictionary<string, ExperienceData>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    /// <summary>
    /// Inicia una nueva experiencia de aprendizaje
    /// </summary>
    public void StartExperience(string experienceName)
    {
        currentExperience = experienceName;
        currentScore = baseScore;
        errorCount = 0;
        elapsedTime = 0f;
        isTimerRunning = true;

        UpdateUI();

        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }

        Debug.Log($"[AdvancedGameManager] Iniciada experiencia: {experienceName}");
    }

    /// <summary>
    /// Agrega puntos al jugador
    /// </summary>
    public void AddPoints(int points, string reason = "")
    {
        currentScore += points;
        currentScore = Mathf.Max(0, currentScore); // No permitir puntuación negativa

        UpdateScoreUI();
        PlaySound(pointsGainedSound);

        if (!string.IsNullOrEmpty(reason))
        {
            ShowFloatingText($"+{points}: {reason}");
        }

        Debug.Log($"[AdvancedGameManager] +{points} puntos. Razón: {reason}");
    }

    /// <summary>
    /// Registra un error del usuario
    /// </summary>
    public void RegisterError(string errorType)
    {
        errorCount++;
        currentScore += errorPenalty;
        currentScore = Mathf.Max(0, currentScore);

        UpdateUI();
        PlaySound(errorSound);

        ShowFloatingText($"{errorPenalty}: {errorType}");

        Debug.Log($"[AdvancedGameManager] Error registrado: {errorType}. Errores totales: {errorCount}");
    }

    /// <summary>
    /// Completa la experiencia actual y calcula la puntuación final
    /// </summary>
    public void RegisterCompletion(string experienceName, float customScore = -1)
    {
        isTimerRunning = false;

        // Calcular bonificaciones
        int finalScore = customScore >= 0 ? (int)customScore : currentScore;

        // Bonus por tiempo (completar rápido)
        if (elapsedTime < 180f) // Menos de 3 minutos
        {
            int timeBonusPoints = Mathf.RoundToInt(timeBonus * (1f - elapsedTime / 180f));
            finalScore += timeBonusPoints;
            Debug.Log($"[AdvancedGameManager] Bonus de tiempo: +{timeBonusPoints}");
        }

        // Bonus por precisión (pocos errores)
        if (errorCount == 0)
        {
            finalScore += precisionBonus;
            Debug.Log($"[AdvancedGameManager] Bonus de precisión: +{precisionBonus}");
        }

        // Guardar en historial
        ExperienceData data = new ExperienceData
        {
            name = experienceName,
            completionTime = elapsedTime,
            score = finalScore,
            errors = errorCount,
            timestamp = System.DateTime.Now
        };

        if (experienceHistory.ContainsKey(experienceName))
        {
            // Actualizar si es mejor puntuación
            if (finalScore > experienceHistory[experienceName].score)
            {
                experienceHistory[experienceName] = data;
                Debug.Log($"[AdvancedGameManager] ¡Nuevo récord! Puntuación anterior superada");
            }
        }
        else
        {
            experienceHistory.Add(experienceName, data);
        }

        ShowCompletionScreen(finalScore);
        PlaySound(completionSound);

        Debug.Log($"[AdvancedGameManager] Experiencia completada: {experienceName}. Puntuación final: {finalScore}");
    }

    private void ShowCompletionScreen(int finalScore)
    {
        if (completionPanel == null) return;

        completionPanel.SetActive(true);

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Puntuación Final: {finalScore}";
        }

        if (feedbackMessageText != null)
        {
            string feedback = GetFeedbackMessage(finalScore, errorCount);
            feedbackMessageText.text = feedback;
        }
    }

    private string GetFeedbackMessage(int score, int errors)
    {
        string message = "";

        // Evaluación general
        if (score >= 100)
        {
            message = "¡EXCELENTE TRABAJO!\n";
        }
        else if (score >= 80)
        {
            message = "¡MUY BIEN!\n";
        }
        else if (score >= 60)
        {
            message = "BIEN HECHO\n";
        }
        else
        {
            message = "NECESITAS PRÁCTICA\n";
        }

        message += $"\nTiempo: {FormatTime(elapsedTime)}\n";
        message += $"Errores: {errors}\n";

        // Mensajes especiales
        if (errors == 0)
        {
            message += "\n¡Trabajo sin errores! +30 puntos";
        }

        if (elapsedTime < 120f)
        {
            message += "\n¡Completado rápidamente! Bonus de tiempo";
        }

        // Sugerencias
        if (errors > 3)
        {
            message += "\n\nConsejo: Revisa los pasos antes de actuar";
        }

        if (elapsedTime > 300f)
        {
            message += "\n\nConsejo: Intenta trabajar más rápido";
        }

        return message;
    }

    private void UpdateUI()
    {
        UpdateScoreUI();
        UpdateTimerUI();
        UpdateErrorCountUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Puntuación: {currentScore}";
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = $"Tiempo: {FormatTime(elapsedTime)}";
        }
    }

    private void UpdateErrorCountUI()
    {
        if (errorCountText != null)
        {
            errorCountText.text = $"Errores: {errorCount}";
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    private void ShowFloatingText(string text)
    {
        // Implementar sistema de texto flotante
        // Por ahora solo debug
        Debug.Log($"[AdvancedGameManager] Floating Text: {text}");
    }

    private void PlaySound(AudioClip clip)
    {
        if (uiAudioSource != null && clip != null)
        {
            uiAudioSource.PlayOneShot(clip);
        }
    }

    // Métodos públicos de consulta

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public int GetErrorCount()
    {
        return errorCount;
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    public ExperienceData GetBestScore(string experienceName)
    {
        if (experienceHistory.ContainsKey(experienceName))
        {
            return experienceHistory[experienceName];
        }
        return null;
    }

    /// <summary>
    /// Reinicia la experiencia actual
    /// </summary>
    public void RestartCurrentExperience()
    {
        if (!string.IsNullOrEmpty(currentExperience))
        {
            StartExperience(currentExperience);
            Debug.Log($"[AdvancedGameManager] Reiniciando experiencia: {currentExperience}");
        }
    }

    /// <summary>
    /// Pausa o reanuda el temporizador
    /// </summary>
    public void SetTimerState(bool running)
    {
        isTimerRunning = running;
        Debug.Log($"[AdvancedGameManager] Temporizador: {(running ? "Activo" : "Pausado")}");
    }
}
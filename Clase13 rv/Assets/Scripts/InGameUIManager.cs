using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestor de UI durante el juego
/// Muestra puntuación, tiempo, iconos y feedback en tiempo real
/// </summary>
public class InGameUIManager : MonoBehaviour
{
    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image scoreIcon;
    [SerializeField] private Animator scoreAnimator;

    [Header("Timer Display")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerIcon;

    [Header("Error Display")]
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Image errorIcon;

    [Header("Current Step Display")]
    [SerializeField] private TextMeshProUGUI currentStepText;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI progressPercentageText;

    [Header("Icons")]
    [SerializeField] private Sprite scoreIconSprite;
    [SerializeField] private Sprite timerIconSprite;
    [SerializeField] private Sprite errorIconSprite;
    [SerializeField] private Sprite checkmarkIcon;
    [SerializeField] private Sprite warningIcon;

    [Header("Feedback Popup")]
    [SerializeField] private GameObject feedbackPopup;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Image feedbackIcon;
    [SerializeField] private float feedbackDuration = 2f;

    [Header("Points Animation")]
    [SerializeField] private GameObject pointsTextPrefab;
    [SerializeField] private Transform pointsSpawnLocation;
    [SerializeField] private float pointsAnimDuration = 1.5f;

    [Header("VR Canvas Settings")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private float canvasDistance = 2f;
    [SerializeField] private bool followPlayer = false;
    [SerializeField] private Transform playerCamera;

    private int currentScore = 0;
    private int currentErrors = 0;
    private float currentTime = 0f;
    private int totalSteps = 5;
    private int completedSteps = 0;

    private void Start()
    {
        SetupIcons();
        UpdateUI();

        if (feedbackPopup != null)
        {
            feedbackPopup.SetActive(false);
        }

        // Configurar canvas para VR
        SetupVRCanvas();
    }

    private void Update()
    {
        // Actualizar desde AdvancedGameManager si está disponible
        if (AdvancedGameManager.Instance != null)
        {
            currentScore = AdvancedGameManager.Instance.GetCurrentScore();
            currentErrors = AdvancedGameManager.Instance.GetErrorCount();
            currentTime = AdvancedGameManager.Instance.GetElapsedTime();

            UpdateScoreDisplay();
            UpdateTimerDisplay();
            UpdateErrorDisplay();
        }

        // Hacer que el canvas siga al jugador si está configurado
        if (followPlayer && playerCamera != null && mainCanvas != null)
        {
            FollowPlayerCamera();
        }
    }

    #region UI Setup

    private void SetupIcons()
    {
        if (scoreIcon != null && scoreIconSprite != null)
        {
            scoreIcon.sprite = scoreIconSprite;
        }

        if (timerIcon != null && timerIconSprite != null)
        {
            timerIcon.sprite = timerIconSprite;
        }

        if (errorIcon != null && errorIconSprite != null)
        {
            errorIcon.sprite = errorIconSprite;
        }
    }

    private void SetupVRCanvas()
    {
        if (mainCanvas == null) return;

        // Configurar para VR
        mainCanvas.renderMode = RenderMode.WorldSpace;

        // IMPORTANTE: Asignar cámara de evento
        if (playerCamera != null)
        {
            mainCanvas.worldCamera = playerCamera.GetComponent<Camera>();

            // Posicionar canvas frente al jugador
            Vector3 canvasPosition = playerCamera.position + playerCamera.forward * canvasDistance + Vector3.up * 0.5f;
            mainCanvas.transform.position = canvasPosition;
            mainCanvas.transform.LookAt(playerCamera);
            mainCanvas.transform.Rotate(0, 180, 0);
        }

        // Escalar apropiadamente
        mainCanvas.transform.localScale = Vector3.one * 0.001f;
    }

    private void FollowPlayerCamera()
    {
        Vector3 targetPosition = playerCamera.position + playerCamera.forward * canvasDistance;
        mainCanvas.transform.position = Vector3.Lerp(
            mainCanvas.transform.position,
            targetPosition,
            Time.deltaTime * 2f
        );

        mainCanvas.transform.LookAt(playerCamera);
        mainCanvas.transform.Rotate(0, 180, 0);
    }

    #endregion

    #region Display Updates

    private void UpdateUI()
    {
        UpdateScoreDisplay();
        UpdateTimerDisplay();
        UpdateErrorDisplay();
        UpdateProgressBar();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }

        // Animar cuando cambia la puntuación
        if (scoreAnimator != null)
        {
            scoreAnimator.SetTrigger("Update");
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void UpdateErrorDisplay()
    {
        if (errorText != null)
        {
            errorText.text = currentErrors.ToString();
        }
    }

    public void UpdateCurrentStep(string stepDescription)
    {
        if (currentStepText != null)
        {
            currentStepText.text = stepDescription;
        }
    }

    public void UpdateProgressBar()
    {
        if (progressBar != null)
        {
            float progress = (float)completedSteps / totalSteps;
            progressBar.fillAmount = progress;
        }

        if (progressPercentageText != null)
        {
            float percentage = ((float)completedSteps / totalSteps) * 100f;
            progressPercentageText.text = $"{Mathf.RoundToInt(percentage)}%";
        }
    }

    #endregion

    #region Feedback System

    /// <summary>
    /// Mostrar mensaje de feedback temporal
    /// </summary>
    public void ShowFeedback(string message, FeedbackType type)
    {
        if (feedbackPopup == null || feedbackText == null) return;

        feedbackText.text = message;

        // Cambiar icono según el tipo
        if (feedbackIcon != null)
        {
            switch (type)
            {
                case FeedbackType.Success:
                    feedbackIcon.sprite = checkmarkIcon;
                    feedbackIcon.color = Color.green;
                    break;
                case FeedbackType.Error:
                    feedbackIcon.sprite = warningIcon;
                    feedbackIcon.color = Color.red;
                    break;
                case FeedbackType.Info:
                    feedbackIcon.color = Color.yellow;
                    break;
            }
        }

        feedbackPopup.SetActive(true);

        // Ocultar después del tiempo configurado
        CancelInvoke(nameof(HideFeedback));
        Invoke(nameof(HideFeedback), feedbackDuration);
    }

    private void HideFeedback()
    {
        if (feedbackPopup != null)
        {
            feedbackPopup.SetActive(false);
        }
    }

    #endregion

    #region Points Animation

    /// <summary>
    /// Mostrar animación de puntos ganados
    /// </summary>
    public void ShowPointsGained(int points)
    {
        if (pointsTextPrefab == null || pointsSpawnLocation == null) return;

        GameObject pointsObj = Instantiate(pointsTextPrefab, pointsSpawnLocation.position, Quaternion.identity);
        pointsObj.transform.SetParent(mainCanvas.transform);

        TextMeshProUGUI pointsText = pointsObj.GetComponent<TextMeshProUGUI>();
        if (pointsText != null)
        {
            pointsText.text = $"+{points}";
            pointsText.color = Color.yellow;
        }

        // Animar hacia arriba y desvanecer
        StartCoroutine(AnimatePoints(pointsObj));
    }

    private System.Collections.IEnumerator AnimatePoints(GameObject pointsObj)
    {
        Vector3 startPos = pointsObj.transform.localPosition;
        Vector3 endPos = startPos + Vector3.up * 100f;

        TextMeshProUGUI text = pointsObj.GetComponent<TextMeshProUGUI>();
        Color startColor = text != null ? text.color : Color.white;

        float elapsed = 0f;

        while (elapsed < pointsAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pointsAnimDuration;

            // Mover hacia arriba
            pointsObj.transform.localPosition = Vector3.Lerp(startPos, endPos, t);

            // Desvanecer
            if (text != null)
            {
                Color newColor = startColor;
                newColor.a = 1f - t;
                text.color = newColor;
            }

            yield return null;
        }

        Destroy(pointsObj);
    }

    #endregion

    #region Public Methods

    public void SetTotalSteps(int steps)
    {
        totalSteps = steps;
        UpdateProgressBar();
    }

    public void IncrementCompletedSteps()
    {
        completedSteps++;
        UpdateProgressBar();
    }

    public void ResetProgress()
    {
        completedSteps = 0;
        UpdateProgressBar();
    }

    #endregion

    public enum FeedbackType
    {
        Success,
        Error,
        Info
    }
}
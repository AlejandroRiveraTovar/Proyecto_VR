using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Controlador principal para la experiencia de cambio de aceite en VR
/// Maneja las etapas del proceso y la retroalimentaci�n al usuario
/// </summary>
public class OilChangeInteraction : MonoBehaviour
{
    [Header("Referencias del Vehículo")]
    [SerializeField] private Transform oilCapLocation; // Ubicación de la tapa de aceite
    [SerializeField] private Transform oilDipstickLocation; // Varilla medidora
    [SerializeField] private GameObject oilCapObject; // Modelo 3D de la tapa
    [SerializeField] private GameObject dipstickObject; // Modelo 3D de la varilla

    [Header("Zona de Interacción")]
    [SerializeField] private GameObject highlightZone; // Área que se ilumina
    [SerializeField] private Material highlightMaterial; // Material con emisión
    [SerializeField] private float highlightIntensity = 2f;
    [SerializeField] private Color correctZoneColor = Color.green;
    [SerializeField] private Color incorrectZoneColor = Color.red;

    [Header("Objetos Interactuables")]
    [SerializeField] private XRGrabInteractable oilBottle; // Botella de aceite agarrable
    [SerializeField] private XRGrabInteractable wrench; // Llave para aflojar
    [SerializeField] private Transform oilBottleSpout; // Pico/boquilla de la botella

    [Header("Sistema de Vertido")]
    [SerializeField] private ParticleSystem oilParticles; // Partículas de aceite
    [SerializeField] private AudioSource oilPouringAudioSource; // Audio del aceite
    [SerializeField] private AudioClip oilPouringSound; // Sonido de líquido
    [SerializeField] private float particleEmissionRate = 50f;
    [SerializeField] private GameObject pouringZoneHighlight; // Iluminación zona de vertido

    [Header("Configuración de Distancia")]
    [SerializeField] private float detectionRadius = 0.3f; // Radio de detección
    [SerializeField] private float pourDistance = 0.15f; // Distancia para verter

    [Header("Audio y Retroalimentación")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;
    [SerializeField] private AudioClip completionSound;

    [Header("UI de Retroalimentación")]
    [SerializeField] private Canvas feedbackCanvas;
    [SerializeField] private TMPro.TextMeshProUGUI feedbackText;
    [SerializeField] private float feedbackDuration = 3f;

    // Estado del proceso
    private enum OilChangeState
    {
        RemoveDipstick,
        RemoveOilCap,
        PourOil,
        ReplaceOilCap,
        CheckLevel,
        Complete
    }

    private OilChangeState currentState = OilChangeState.RemoveDipstick;
    private bool isHighlighting = false;
    private Material originalMaterial;
    private Renderer highlightRenderer;
    private float oilLevel = 0f; // 0 a 1
    private const float targetOilLevel = 0.8f;

    // Integración con zona system
    private InteractiveZoneSystem zoneSystem;
    private bool hasRegisteredDistanceError = false;

    private void Start()
    {
        InitializeComponents();
        SetupInteractables();
        UpdateInstructions();

        // Iniciar experiencia con AdvancedGameManager
        if (AdvancedGameManager.Instance != null)
        {
            AdvancedGameManager.Instance.StartExperience("OilChange");
        }

        // Buscar zona system
        zoneSystem = FindObjectOfType<InteractiveZoneSystem>();
        if (zoneSystem == null)
        {
            Debug.LogWarning("[OilChangeInteraction] No se encontró InteractiveZoneSystem");
        }
    }

    private void InitializeComponents()
    {
        if (highlightZone != null)
        {
            highlightRenderer = highlightZone.GetComponent<Renderer>();
            if (highlightRenderer != null)
            {
                originalMaterial = highlightRenderer.material;
            }
            highlightZone.SetActive(false);
        }

        if (feedbackCanvas != null)
        {
            feedbackCanvas.gameObject.SetActive(false);
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void SetupInteractables()
    {
        if (oilBottle != null)
        {
            oilBottle.selectEntered.AddListener(OnOilBottleGrabbed);
            oilBottle.selectExited.AddListener(OnOilBottleReleased);
        }

        if (wrench != null)
        {
            wrench.selectEntered.AddListener(OnWrenchGrabbed);
        }
    }

    private void Update()
    {
        CheckProximityAndHighlight();

        // Verificar si el usuario está vertiendo aceite
        if (currentState == OilChangeState.PourOil && oilBottle != null && oilBottle.isSelected)
        {
            CheckOilPouringPosition();
        }
    }

    private void CheckProximityAndHighlight()
    {
        Transform targetLocation = GetCurrentTargetLocation();
        if (targetLocation == null) return;

        // Obtener la posición del controlador VR más cercano
        Transform nearestController = GetNearestController();
        if (nearestController == null) return;

        float distance = Vector3.Distance(nearestController.position, targetLocation.position);

        if (distance <= detectionRadius)
        {
            if (!isHighlighting)
            {
                ActivateHighlight(true);
            }
        }
        else
        {
            if (isHighlighting)
            {
                DeactivateHighlight();
            }
        }
    }

    private Transform GetCurrentTargetLocation()
    {
        switch (currentState)
        {
            case OilChangeState.RemoveDipstick:
                return oilDipstickLocation;
            case OilChangeState.RemoveOilCap:
            case OilChangeState.PourOil:
            case OilChangeState.ReplaceOilCap:
                return oilCapLocation;
            case OilChangeState.CheckLevel:
                return oilDipstickLocation;
            default:
                return null;
        }
    }

    private Transform GetNearestController()
    {
        // Buscar controladores XR en la escena
        var controllers = FindObjectsOfType<XRBaseController>();
        if (controllers.Length == 0) return null;

        Transform target = GetCurrentTargetLocation();
        if (target == null) return null;

        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (var controller in controllers)
        {
            float dist = Vector3.Distance(controller.transform.position, target.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = controller.transform;
            }
        }

        return nearest;
    }

    private void ActivateHighlight(bool isCorrectZone)
    {
        if (highlightZone == null || highlightRenderer == null) return;

        highlightZone.SetActive(true);
        isHighlighting = true;

        // Crear material con emisión
        Material glowMat = new Material(highlightMaterial != null ? highlightMaterial : originalMaterial);
        glowMat.EnableKeyword("_EMISSION");

        Color emissionColor = isCorrectZone ? correctZoneColor : incorrectZoneColor;
        glowMat.SetColor("_EmissionColor", emissionColor * highlightIntensity);

        highlightRenderer.material = glowMat;
    }

    private void DeactivateHighlight()
    {
        if (highlightZone == null) return;

        isHighlighting = false;
        highlightZone.SetActive(false);
    }

    private void OnOilBottleGrabbed(SelectEnterEventArgs args)
    {
        if (true)
        {
            ShowFeedback("Acércate a la entrada de aceite del motor", Color.yellow);

            // Puntos por agarrar la botella en el momento correcto
            if (AdvancedGameManager.Instance != null)
            {
                AdvancedGameManager.Instance.AddPoints(5, "Botella de aceite tomada");
            }

            // Completar zona de botella
            if (zoneSystem != null)
            {
                zoneSystem.CompleteZone("OilBottle");
            }
        }
        else
        {
            ShowFeedback("Primero debes remover la tapa de aceite", Color.red);
            PlaySound(incorrectSound);

            // Registrar error
            if (AdvancedGameManager.Instance != null)
            {
                AdvancedGameManager.Instance.RegisterError("Botella tomada antes de tiempo");
            }
        }
    }

    private void OnOilBottleReleased(SelectExitEventArgs args)
    {
        // Lógica cuando se suelta la botella
    }

    private void OnWrenchGrabbed(SelectEnterEventArgs args)
    {
        // Lógica para usar la llave
    }

    private void CheckOilPouringPosition()
    {
        if (oilCapLocation == null) return;

        Transform spoutTransform = oilBottleSpout != null ? oilBottleSpout : oilBottle.transform;

        float distance = Vector3.Distance(spoutTransform.position, oilCapLocation.position);

        // Activar highlight de zona
        if (pouringZoneHighlight != null)
            pouringZoneHighlight.SetActive(distance <= pourDistance * 1.5f);

        // Si estamos en la zona → verter SIN inclinación
        if (distance <= pourDistance)
        {
            if (oilParticles != null && !oilParticles.isPlaying)
                oilParticles.Play();

            if (oilPouringAudioSource != null && !oilPouringAudioSource.isPlaying && oilPouringSound != null)
            {
                oilPouringAudioSource.clip = oilPouringSound;
                oilPouringAudioSource.loop = true;
                oilPouringAudioSource.Play();
            }

            PourOil();
        }
        else
        {
            StopPouring();
        }
    }

    private void StopPouring()
    {
        // Detener partículas
        if (oilParticles != null && oilParticles.isPlaying)
        {
            oilParticles.Stop();
        }

        // Detener sonido
        if (oilPouringAudioSource != null && oilPouringAudioSource.isPlaying)
        {
            oilPouringAudioSource.Stop();
        }
    }

    

    private void PourOil()
    {
        oilLevel += Time.deltaTime * 0.2f; // Incremento gradual

        if (oilLevel >= targetOilLevel && oilLevel < 1f)
        {
            // Nivel correcto
            ShowFeedback($"Nivel de aceite: {(oilLevel * 100):F0}% - ¡Perfecto!", Color.green);
        }
        else if (oilLevel >= 1f)
        {
            // Exceso de aceite
            oilLevel = 1f;
            ShowFeedback("¡CUIDADO! Exceso de aceite. Has llenado demasiado", Color.red);
            PlaySound(incorrectSound);

            // Registrar error grave
            if (AdvancedGameManager.Instance != null)
            {
                AdvancedGameManager.Instance.RegisterError("Exceso de aceite - Sobrellenado");
            }

            AdvanceToNextState();
        }
        else
        {
            ShowFeedback($"Vertiendo aceite... {(oilLevel * 100):F0}%", Color.cyan);
        }

        if (oilLevel >= targetOilLevel && oilLevel < 1f)
        {
            Invoke(nameof(CompleteOilPouring), 1f);
        }
    }

    private void CompleteOilPouring()
    {
        ShowFeedback("¡Excelente! Nivel de aceite correcto", Color.green);
        PlaySound(correctSound);

        // Detener vertido
        StopPouring();

        // Desactivar highlight de zona
        if (pouringZoneHighlight != null)
        {
            pouringZoneHighlight.SetActive(false);
        }

        // Puntos importantes por verter correctamente
        if (AdvancedGameManager.Instance != null)
        {
            AdvancedGameManager.Instance.AddPoints(25, "Aceite vertido correctamente");
        }

        AdvanceToNextState();
    }

    public void OnDipstickRemoved()
    {
        if (currentState == OilChangeState.RemoveDipstick)
        {
            ShowFeedback("¡Correcto! Varilla removida. Ahora remueve la tapa de aceite", Color.green);
            PlaySound(correctSound);

            // Registrar puntos en AdvancedGameManager
            if (AdvancedGameManager.Instance != null)
            {
                AdvancedGameManager.Instance.AddPoints(10, "Varilla removida correctamente");
            }

            // Completar zona interactiva
            if (zoneSystem != null)
            {
                zoneSystem.CompleteZone("Dipstick");
            }

            AdvanceToNextState();
        }
        else
        {
            ShowFeedback("Ese no es el paso correcto en este momento", Color.red);
            PlaySound(incorrectSound);

            // Registrar error
            if (AdvancedGameManager.Instance != null)
            {
                AdvancedGameManager.Instance.RegisterError("Varilla removida en momento incorrecto");
            }
        }
    }

    public void OnOilCapRemoved()
    {
        if (currentState == OilChangeState.RemoveOilCap)
        {
            if (oilCapObject != null) oilCapObject.SetActive(false);
            ShowFeedback("¡Perfecto! Tapa removida. Ahora toma la botella de aceite", Color.green);
            PlaySound(correctSound);

            // Registrar puntos
            if (AdvancedGameManager.Instance != null)
            {
                AdvancedGameManager.Instance.AddPoints(15, "Tapa de aceite removida correctamente");
            }

            // Completar zona interactiva
            if (zoneSystem != null)
            {
                zoneSystem.CompleteZone("OilCap");
            }

            AdvanceToNextState();
        }
        else
        {
            ShowFeedback("Primero debes remover la varilla medidora", Color.red);
            PlaySound(incorrectSound);

            // Registrar error
            if (AdvancedGameManager.Instance != null)
            {
                AdvancedGameManager.Instance.RegisterError("Intento de remover tapa sin haber quitado la varilla");
            }
        }
    }

    public void OnOilCapReplaced()
    {
        if (currentState == OilChangeState.ReplaceOilCap)
        {
            if (oilCapObject != null) oilCapObject.SetActive(true);
            ShowFeedback("¡Bien hecho! Tapa colocada. Verifica el nivel con la varilla", Color.green);
            PlaySound(correctSound);

            // Registrar puntos
            if (AdvancedGameManager.Instance != null)
            {
                AdvancedGameManager.Instance.AddPoints(10, "Tapa de aceite reemplazada");
            }

            // Completar zona
            if (zoneSystem != null)
            {
                zoneSystem.CompleteZone("ReplaceOilCap");
            }

            AdvanceToNextState();
        }
    }

    public void OnLevelChecked()
    {
        if (currentState == OilChangeState.CheckLevel)
        {
            ShowFeedback("¡Excelente! Nivel verificado correctamente", Color.green);
            PlaySound(correctSound);

            // Registrar puntos
            if (AdvancedGameManager.Instance != null)
            {
                AdvancedGameManager.Instance.AddPoints(10, "Nivel verificado");
            }

            // Completar última zona
            if (zoneSystem != null)
            {
                zoneSystem.CompleteZone("CheckLevel");
            }

            AdvanceToNextState();
        }
    }

    private void AdvanceToNextState()
    {
        currentState++;
        hasRegisteredDistanceError = false; // Resetear bandera para el siguiente estado

        if (currentState == OilChangeState.Complete)
        {
            CompleteExperience();
        }
        else
        {
            UpdateInstructions();
        }
    }

    private void UpdateInstructions()
    {
        string instruction = "";

        switch (currentState)
        {
            case OilChangeState.RemoveDipstick:
                instruction = "Paso 1: Remueve la varilla medidora de aceite";
                break;
            case OilChangeState.RemoveOilCap:
                instruction = "Paso 2: Remueve la tapa de aceite del motor";
                break;
            case OilChangeState.PourOil:
                instruction = "Paso 3: Vierte el aceite nuevo en el motor";
                break;
            case OilChangeState.ReplaceOilCap:
                instruction = "Paso 4: Coloca de nuevo la tapa de aceite";
                break;
            case OilChangeState.CheckLevel:
                instruction = "Paso 5: Verifica el nivel con la varilla";
                break;
        }

        ShowFeedback(instruction, Color.white, 5f);
    }

    private void CompleteExperience()
    {
        float score = CalculateScore();
        string message = $"¡Cambio de aceite completado!\n\nPuntuación: {score:F0}/100";

        if (oilLevel > 1f)
        {
            message += "\n\nAdvertencia: Llenaste en exceso (-10 puntos)";
        }

        ShowFeedback(message, Color.green, 10f);
        PlaySound(completionSound);

        // Registrar completado en AdvancedGameManager
        if (AdvancedGameManager.Instance != null)
        {
            AdvancedGameManager.Instance.RegisterCompletion("OilChange", score);
        }
    }

    private float CalculateScore()
    {
        float baseScore = 100f;

        // Penalización por exceso de aceite
        if (oilLevel > 1f)
        {
            baseScore -= 10f;
        }

        // Bonificación por nivel óptimo
        if (Mathf.Abs(oilLevel - targetOilLevel) < 0.05f)
        {
            baseScore += 10f;
        }

        return Mathf.Clamp(baseScore, 0f, 110f);
    }

    private void ShowFeedback(string message, Color color, float duration = -1f)
    {
        if (feedbackCanvas == null || feedbackText == null) return;

        feedbackText.text = message;
        feedbackText.color = color;
        feedbackCanvas.gameObject.SetActive(true);

        CancelInvoke(nameof(HideFeedback));

        float displayTime = duration > 0 ? duration : feedbackDuration;
        Invoke(nameof(HideFeedback), displayTime);
    }

    private void HideFeedback()
    {
        if (feedbackCanvas != null)
        {
            feedbackCanvas.gameObject.SetActive(false);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar zonas de detección en el editor
        if (oilCapLocation != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(oilCapLocation.position, detectionRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(oilCapLocation.position, pourDistance);
        }

        if (oilDipstickLocation != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(oilDipstickLocation.position, detectionRadius);
        }
    }
}
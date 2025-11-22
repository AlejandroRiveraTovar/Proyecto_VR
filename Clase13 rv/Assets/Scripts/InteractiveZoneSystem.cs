using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Sistema que detecta cuando el jugador se acerca a zonas interactivas
/// y muestra mensajes de instrucción
/// </summary>
public class InteractiveZoneSystem : MonoBehaviour
{
    [System.Serializable]
    public class InteractiveZone
    {
        [Header("Configuración")]
        public string zoneName;
        public Transform zonePosition; // GameObject vacío que marca la posición
        public float detectionRadius = 1f;

        [Header("Mensaje")]
        [TextArea(2, 4)]
        public string instructionMessage;
        public Color messageColor = Color.white;

        [Header("Visual")]
        public GameObject highlightEffect; // Efecto visual (opcional)
        public bool showGizmo = true;

        [Header("Estado")]
        public bool isCompleted = false;
        public bool isActive = true;

        [Header("Audio por Zona")]
        public AudioClip zoneMusic;        // Música o sonido ambiental de la zona
        public bool loopMusic = true;      // Si debe hacer loop

        [HideInInspector]
        public bool playerIsNear = false;
    }

    [Header("Zonas Interactivas")]
    [SerializeField] private InteractiveZone[] interactiveZones = new InteractiveZone[5];

    [Header("Referencias del Jugador")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool autoFindPlayer = true;

    [Header("UI de Mensajes")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image messageBackground;

    [Header("Configuración del Sistema")]
    [SerializeField] private bool showNearestOnly = true; // Cambiar a true por defecto
    [SerializeField] private float messageFadeSpeed = 5f;

    [Header("Posicionamiento de Canvas")]
    [SerializeField] private bool moveCanvasToZone = true; // Mover canvas a la zona
    [SerializeField] private float canvasMoveSpeed = 8f; // Velocidad de movimiento
    [SerializeField] private Vector3 canvasOffset = new Vector3(0, 0.5f, 0); // Offset desde la zona
    [SerializeField] private bool rotateToCamera = true; // Rotar hacia cámara

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip enterZoneSound;
    [SerializeField] private AudioClip exitZoneSound;
    [SerializeField] private AudioClip completionSound;

    private InteractiveZone currentZone = null;
    private CanvasGroup messageCanvasGroup;
    private int completedZonesCount = 0;

    private void Start()
    {
        if (autoFindPlayer && playerTransform == null)
        {
            FindPlayer();
        }

        SetupMessagePanel();
        InitializeZones();
    }

    private void Update()
    {
        CheckProximityToZones();
        UpdateMessageVisibility();
    }

    private void FindPlayer()
    {
        // Buscar XR Origin
        GameObject xrOrigin = GameObject.Find("XR Origin");
        if (xrOrigin != null)
        {
            Transform cameraOffset = xrOrigin.transform.Find("Camera Offset");
            if (cameraOffset != null)
            {
                Transform mainCamera = cameraOffset.Find("Main Camera");
                if (mainCamera != null)
                {
                    playerTransform = mainCamera;
                    Debug.Log("[InteractiveZone] Jugador encontrado en XR Origin");
                    return;
                }
            }
        }

        // Buscar Main Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            playerTransform = mainCam.transform;
            Debug.Log("[InteractiveZone] Jugador encontrado: Main Camera");
        }
    }

    private void SetupMessagePanel()
    {
        if (messagePanel != null)
        {
            messageCanvasGroup = messagePanel.GetComponent<CanvasGroup>();
            if (messageCanvasGroup == null)
            {
                messageCanvasGroup = messagePanel.AddComponent<CanvasGroup>();
            }
            messageCanvasGroup.alpha = 0;
            messagePanel.SetActive(true);
        }
    }

    private void InitializeZones()
    {
        for (int i = 0; i < interactiveZones.Length; i++)
        {
            if (interactiveZones[i].highlightEffect != null)
            {
                interactiveZones[i].highlightEffect.SetActive(false);
            }
        }
    }

    private void CheckProximityToZones()
    {
        if (playerTransform == null) return;

        if (showNearestOnly)
        {
            // MODO ORIGINAL: Solo mostrar la zona más cercana
            InteractiveZone nearestZone = null;
            float nearestDistance = float.MaxValue;

            foreach (var zone in interactiveZones)
            {
                if (!zone.isActive || zone.isCompleted || zone.zonePosition == null)
                    continue;

                float distance = Vector3.Distance(playerTransform.position, zone.zonePosition.position);

                if (distance <= zone.detectionRadius && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestZone = zone;
                }
            }

            if (nearestZone != currentZone)
            {
                if (currentZone != null)
                {
                    OnExitZone(currentZone);
                }

                if (nearestZone != null)
                {
                    OnEnterZone(nearestZone);
                }

                currentZone = nearestZone;
            }
        }
        else
        {
            // NUEVO MODO: Verificar TODAS las zonas independientemente
            foreach (var zone in interactiveZones)
            {
                if (!zone.isActive || zone.isCompleted || zone.zonePosition == null)
                    continue;

                float distance = Vector3.Distance(playerTransform.position, zone.zonePosition.position);
                bool isInRange = distance <= zone.detectionRadius;

                // Si entró a la zona y no estaba marcada como "cerca"
                if (isInRange && !zone.playerIsNear)
                {
                    OnEnterZone(zone);
                    currentZone = zone; // Actualizar zona actual
                }
                // Si salió de la zona y estaba marcada como "cerca"
                else if (!isInRange && zone.playerIsNear)
                {
                    OnExitZone(zone);
                    if (currentZone == zone)
                    {
                        currentZone = null;
                    }
                }
            }
        }
    }

    public bool AllZonesCompleted()
    {
        return GetCompletedZonesCount() >= GetTotalZonesCount();
    }

    private void OnEnterZone(InteractiveZone zone)
    {
        zone.playerIsNear = true;

        // Mostrar mensaje
        if (messageText != null)
        {
            messageText.text = zone.instructionMessage;
            messageText.color = zone.messageColor;
        }

        if (messageBackground != null)
        {
            messageBackground.color = new Color(
                zone.messageColor.r * 0.2f,
                zone.messageColor.g * 0.2f,
                zone.messageColor.b * 0.2f,
                0.8f
            );
        }

        // Activar efecto visual
        if (zone.highlightEffect != null)
        {
            zone.highlightEffect.SetActive(true);
        }

        // Sonido general de entrada
        PlaySound(enterZoneSound);

        // Reproducir música de la zona
        if (audioSource != null && zone.zoneMusic != null)
        {
            audioSource.clip = zone.zoneMusic;
            audioSource.loop = zone.loopMusic;
            audioSource.Play();
        }

        Debug.Log($"[InteractiveZone] Entraste a zona: {zone.zoneName}");
    }

    private void OnExitZone(InteractiveZone zone)
    {
        zone.playerIsNear = false;

        // Detener música de zona
        if (audioSource != null && zone.zoneMusic != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }

        // Desactivar efecto visual
        if (zone.highlightEffect != null)
        {
            zone.highlightEffect.SetActive(false);
        }

        // Sonido general de salida
        PlaySound(exitZoneSound);

        Debug.Log($"[InteractiveZone] Saliste de zona: {zone.zoneName}");
    }

    private void UpdateMessageVisibility()
    {
        if (messageCanvasGroup == null) return;

        float targetAlpha = (currentZone != null) ? 1f : 0f;
        messageCanvasGroup.alpha = Mathf.Lerp(
            messageCanvasGroup.alpha,
            targetAlpha,
            Time.deltaTime * messageFadeSpeed
        );
    }

    /// <summary>
    /// Llamar cuando se completa una zona
    /// </summary>
    public void CompleteZone(string zoneName)
    {
        foreach (var zone in interactiveZones)
        {
            if (zone.zoneName == zoneName && !zone.isCompleted)
            {
                zone.isCompleted = true;
                zone.isActive = false;
                completedZonesCount++;

                // Desactivar efecto
                if (zone.highlightEffect != null)
                {
                    zone.highlightEffect.SetActive(false);
                }

                // Sonido de completado
                PlaySound(completionSound);

                // Si es la zona actual, limpiar mensaje
                if (currentZone == zone)
                {
                    currentZone = null;
                }

                Debug.Log($"[InteractiveZone] Zona completada: {zoneName} ({completedZonesCount}/{interactiveZones.Length})");

                // Verificar si todas están completadas
                if (completedZonesCount >= interactiveZones.Length)
                {
                    OnAllZonesCompleted();
                }

                break;
            }
        }
    }

    /// <summary>
    /// Llamar cuando se completa una zona por índice
    /// </summary>
    public void CompleteZoneByIndex(int index)
    {
        if (index >= 0 && index < interactiveZones.Length)
        {
            CompleteZone(interactiveZones[index].zoneName);
        }
    }

    private void OnAllZonesCompleted()
    {
        Debug.Log("[InteractiveZone] ¡Todas las zonas completadas!");

        // Notificar al GameController
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            gameController.OnAllTasksCompleted();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public int GetCompletedZonesCount()
    {
        return completedZonesCount;
    }

    public int GetTotalZonesCount()
    {
        return interactiveZones.Length;
    }

    public bool IsZoneCompleted(string zoneName)
    {
        foreach (var zone in interactiveZones)
        {
            if (zone.zoneName == zoneName)
            {
                return zone.isCompleted;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (interactiveZones == null) return;

        foreach (var zone in interactiveZones)
        {
            if (!zone.showGizmo || zone.zonePosition == null) continue;

            // Color según estado
            if (zone.isCompleted)
            {
                Gizmos.color = Color.green;
            }
            else if (zone.playerIsNear)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = Color.cyan;
            }

            // Dibujar esfera de detección
            Gizmos.DrawWireSphere(zone.zonePosition.position, zone.detectionRadius);

            // Etiqueta
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                zone.zonePosition.position + Vector3.up * 0.5f,
                zone.zoneName
            );
#endif
        }
    }
}
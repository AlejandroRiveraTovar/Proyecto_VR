using UnityEngine;

/// <summary>
/// Script para posicionar correctamente un Canvas World Space en VR
/// Asegura que el canvas sea visible en XR Device Simulator
/// </summary>
[RequireComponent(typeof(Canvas))]
public class VRCanvasPositioner : MonoBehaviour
{
    [Header("Configuración de Posición")]
    [SerializeField] private float distanceFromCamera = 2f;
    [SerializeField] private float heightOffset = 0f; // Ajuste vertical desde la cámara
    [SerializeField] private bool followPlayer = false;
    [SerializeField] private float followSpeed = 2f;

    [Header("Referencias")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private bool autoFindCamera = true;

    [Header("Configuración de Canvas")]
    [SerializeField] private Vector2 canvasSize = new Vector2(160, 50);
    [SerializeField] private float canvasScale = 0.01f;

    private Canvas canvas;
    private RectTransform rectTransform;
    private bool isSetup = false;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();

        // Buscar cámara automáticamente si está habilitado
        if (autoFindCamera && playerCamera == null)
        {
            FindPlayerCamera();
        }
    }

    private void Start()
    {
        SetupCanvas();
        PositionCanvas();
    }

    private void Update()
    {
        if (followPlayer && playerCamera != null)
        {
            FollowPlayerCamera();
        }
    }

    private void FindPlayerCamera()
    {
        // Buscar la cámara principal
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            playerCamera = mainCam.transform;
            Debug.Log($"[VRCanvasPositioner] Cámara encontrada: {mainCam.name}");
            return;
        }

        // Buscar XR Origin
        GameObject xrOrigin = GameObject.Find("XR Origin");
        if (xrOrigin != null)
        {
            // Buscar Camera Offset > Main Camera
            Transform cameraOffset = xrOrigin.transform.Find("Camera Offset");
            if (cameraOffset != null)
            {
                Transform mainCamera = cameraOffset.Find("Main Camera");
                if (mainCamera != null)
                {
                    playerCamera = mainCamera;
                    Debug.Log($"[VRCanvasPositioner] Cámara XR encontrada: {mainCamera.name}");
                    return;
                }
            }
        }

        // Última opción: buscar cualquier cámara activa
        Camera[] cameras = FindObjectsOfType<Camera>();
        if (cameras.Length > 0)
        {
            playerCamera = cameras[0].transform;
            Debug.Log($"[VRCanvasPositioner] Usando cámara: {cameras[0].name}");
        }
        else
        {
            Debug.LogError("[VRCanvasPositioner] No se encontró ninguna cámara en la escena!");
        }
    }

    private void SetupCanvas()
    {
        if (isSetup) return;

        // Configurar Canvas para World Space
        canvas.renderMode = RenderMode.WorldSpace;

        // Asignar cámara de evento
        if (playerCamera != null)
        {
            Camera cam = playerCamera.GetComponent<Camera>();
            if (cam != null)
            {
                canvas.worldCamera = cam;
                Debug.Log("[VRCanvasPositioner] Event Camera asignada");
            }
        }

        // Configurar GraphicRaycaster si no existe
        if (GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
        {
            var raycaster = gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            raycaster.ignoreReversedGraphics = true;
            raycaster.blockingObjects = UnityEngine.UI.GraphicRaycaster.BlockingObjects.None;
            Debug.Log("[VRCanvasPositioner] GraphicRaycaster agregado");
        }

        // Configurar tamaño del canvas
        rectTransform.sizeDelta = canvasSize;

        // Configurar escala
        transform.localScale = Vector3.one * canvasScale;

        // Asegurar que esté en el layer UI
        gameObject.layer = LayerMask.NameToLayer("UI");

        isSetup = true;
        Debug.Log("[VRCanvasPositioner] Canvas configurado correctamente");
    }

    private void PositionCanvas()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("[VRCanvasPositioner] No hay cámara asignada");
            return;
        }

        // Calcular posición frente a la cámara
        Vector3 forward = playerCamera.forward;
        Vector3 position = playerCamera.position + forward * distanceFromCamera;

        // Aplicar offset de altura
        position += Vector3.up * heightOffset;

        // Posicionar canvas
        transform.position = position;

        // Hacer que el canvas mire a la cámara
        transform.LookAt(playerCamera);

        // Rotar 180 grados en Y para que mire al jugador correctamente
        transform.Rotate(0, 180, 0);

        Debug.Log($"[VRCanvasPositioner] Canvas posicionado en {position}");
    }

    private void FollowPlayerCamera()
    {
        if (playerCamera == null) return;

        // Calcular posición objetivo
        Vector3 targetPosition = playerCamera.position + playerCamera.forward * distanceFromCamera;
        targetPosition += Vector3.up * heightOffset;

        // Interpolar posición suavemente
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * followSpeed
        );

        // Rotar para mirar a la cámara
        transform.LookAt(playerCamera);
        transform.Rotate(0, 180, 0);
    }

    /// <summary>
    /// Reposicionar el canvas manualmente
    /// </summary>
    public void ResetPosition()
    {
        PositionCanvas();
    }

    /// <summary>
    /// Mostrar/Ocultar canvas con animación
    /// </summary>
    public void SetVisible(bool visible, bool animated = true)
    {
        if (animated)
        {
            StartCoroutine(AnimateVisibility(visible));
        }
        else
        {
            canvas.enabled = visible;
        }
    }

    private System.Collections.IEnumerator AnimateVisibility(bool visible)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        float duration = 0.3f;
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = visible ? 1f : 0f;

        canvas.enabled = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (!visible)
        {
            canvas.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (playerCamera == null) return;

        // Dibujar línea desde cámara al canvas
        Gizmos.color = Color.green;
        Vector3 canvasPos = playerCamera.position + playerCamera.forward * distanceFromCamera;
        canvasPos += Vector3.up * heightOffset;
        Gizmos.DrawLine(playerCamera.position, canvasPos);

        // Dibujar esfera en posición del canvas
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(canvasPos, 0.2f);
    }

    private void OnValidate()
    {
        // Actualizar en tiempo de edición
        if (Application.isPlaying && isSetup)
        {
            SetupCanvas();
            PositionCanvas();
        }
    }
}
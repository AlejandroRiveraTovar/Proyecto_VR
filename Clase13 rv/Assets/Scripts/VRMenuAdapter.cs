using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Adaptador que hace que los menús funcionen tanto en Desktop como en VR
/// Detecta automáticamente el modo y configura la interacción apropiada
/// </summary>
public class VRMenuAdapter : MonoBehaviour
{
    [Header("Detección Automática")]
    [SerializeField] private bool autoDetectVR = true;
    [SerializeField] private bool forceVRMode = false;

    [Header("Referencias")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private EventSystem eventSystem;

    [Header("Configuración Desktop")]
    [SerializeField] private float desktopCanvasDistance = 0f; // Screen Space Overlay

    [Header("Configuración VR")]
    [SerializeField] private float vrCanvasDistance = 3f;
    [SerializeField] private float vrCanvasScale = 0.002f;
    [SerializeField] private Vector3 vrCanvasOffset = new Vector3(0, 0, 0);

    [Header("Input VR")]
    [SerializeField] private bool enableVRPointer = true;
    [SerializeField] private LineRenderer laserPointer;
    [SerializeField] private float laserLength = 10f;
    [SerializeField] private Color laserColor = Color.cyan;

    private bool isVRMode = false;
    private Transform vrCamera;
    private GameObject leftController;
    private GameObject rightController;

    private void Awake()
    {
        DetectVRMode();
        ConfigureMenuForMode();
    }

    private void Start()
    {
        if (isVRMode)
        {
            SetupVRInteraction();
        }
    }

    private void DetectVRMode()
    {
        if (forceVRMode)
        {
            isVRMode = true;
            Debug.Log("[VRMenuAdapter] Modo VR forzado");
            return;
        }

        if (!autoDetectVR)
        {
            isVRMode = false;
            return;
        }

        // Detectar si hay XR Origin en la escena
        GameObject xrOrigin = GameObject.Find("XR Origin");
        if (xrOrigin != null)
        {
            isVRMode = true;
            Debug.Log("[VRMenuAdapter] ✓ Modo VR detectado (XR Origin encontrado)");

            // Buscar cámara y controladores
            Transform cameraOffset = xrOrigin.transform.Find("Camera Offset");
            if (cameraOffset != null)
            {
                vrCamera = cameraOffset.Find("Main Camera");
                leftController = cameraOffset.Find("LeftHand Controller")?.gameObject;
                rightController = cameraOffset.Find("RightHand Controller")?.gameObject;
            }
        }
        else
        {
            isVRMode = false;
            Debug.Log("[VRMenuAdapter] Modo Desktop detectado");
        }

        // También detectar por XR Settings
#if UNITY_ANDROID || UNITY_IOS
        isVRMode = true; // En build móvil, asumir VR
        Debug.Log("[VRMenuAdapter] Plataforma móvil detectada - Modo VR");
#endif
    }

    private void ConfigureMenuForMode()
    {
        if (menuCanvas == null)
        {
            menuCanvas = GetComponent<Canvas>();
        }

        if (menuCanvas == null)
        {
            Debug.LogError("[VRMenuAdapter] ❌ No se encontró Canvas!");
            return;
        }

        if (isVRMode)
        {
            ConfigureForVR();
        }
        else
        {
            ConfigureForDesktop();
        }
    }

    private void ConfigureForDesktop()
    {
        Debug.Log("[VRMenuAdapter] Configurando menú para Desktop...");

        // Screen Space Overlay para desktop
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Asegurar que EventSystem tenga Standalone Input Module
        if (eventSystem == null)
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }

        if (eventSystem != null)
        {
            // Verificar que tenga StandaloneInputModule
            var standaloneInput = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (standaloneInput == null)
            {
                eventSystem.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        Debug.Log("[VRMenuAdapter] ✓ Menú configurado para Desktop");
    }

    private void ConfigureForVR()
    {
        Debug.Log("[VRMenuAdapter] Configurando menú para VR...");

        // World Space para VR
        menuCanvas.renderMode = RenderMode.WorldSpace;

        // Buscar cámara si no la tenemos
        if (vrCamera == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                vrCamera = mainCam.transform;
            }
        }

        // Asignar Event Camera
        if (vrCamera != null)
        {
            Camera cam = vrCamera.GetComponent<Camera>();
            if (cam != null)
            {
                menuCanvas.worldCamera = cam;
                Debug.Log("[VRMenuAdapter] ✓ Event Camera asignada");
            }
        }

        // Posicionar canvas frente al jugador
        PositionCanvasForVR();

        // Configurar escala
        menuCanvas.transform.localScale = Vector3.one * vrCanvasScale;

        // Configurar EventSystem para VR
        ConfigureEventSystemForVR();

        Debug.Log("[VRMenuAdapter] ✓ Menú configurado para VR");
    }

    private void PositionCanvasForVR()
    {
        if (vrCamera == null) return;

        // Posicionar canvas frente a la cámara
        Vector3 position = vrCamera.position + vrCamera.forward * vrCanvasDistance + vrCanvasOffset;
        menuCanvas.transform.position = position;

        // Rotar para mirar a la cámara
        menuCanvas.transform.LookAt(vrCamera);
        menuCanvas.transform.Rotate(0, 180, 0);

        Debug.Log($"[VRMenuAdapter] Canvas posicionado en: {position}");
    }

    private void SetupVRInteraction()
    {
        // Asegurar que los controladores tengan XR Ray Interactor
        EnsureControllerHasRayInteractor(leftController);
        EnsureControllerHasRayInteractor(rightController);
    }

    private void EnsureControllerHasRayInteractor(GameObject controller)
    {
        if (controller == null) return;

        var rayInteractor = controller.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
        if (rayInteractor == null)
        {
            Debug.LogWarning($"[VRMenuAdapter] {controller.name} no tiene XR Ray Interactor");
        }
        else
        {
            Debug.Log($"[VRMenuAdapter] ✓ {controller.name} tiene Ray Interactor");
        }
    }

    private void ConfigureEventSystemForVR()
    {
        if (eventSystem == null)
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }

        if (eventSystem == null)
        {
            Debug.LogError("[VRMenuAdapter] ❌ No se encontró EventSystem!");
            return;
        }

        // Remover Standalone Input Module si existe
        var standaloneInput = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (standaloneInput != null)
        {
            standaloneInput.enabled = false;
        }

        // Agregar XR UI Input Module si no existe
        var xrInput = eventSystem.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
        if (xrInput == null)
        {
            eventSystem.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
            Debug.Log("[VRMenuAdapter] ✓ XR UI Input Module agregado");
        }
        else
        {
            Debug.Log("[VRMenuAdapter] ✓ XR UI Input Module ya existe");
        }
    }

    /// <summary>
    /// Reposicionar canvas manualmente (útil si el jugador se mueve)
    /// </summary>
    public void RepositionCanvas()
    {
        if (isVRMode && vrCamera != null)
        {
            PositionCanvasForVR();
        }
    }

    public bool IsVRMode()
    {
        return isVRMode;
    }

    private void OnDrawGizmos()
    {
        if (!isVRMode || vrCamera == null) return;

        // Dibujar posición del canvas en Scene View
        Vector3 canvasPos = vrCamera.position + vrCamera.forward * vrCanvasDistance + vrCanvasOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(canvasPos, new Vector3(2, 1, 0.1f));
        Gizmos.DrawLine(vrCamera.position, canvasPos);
    }
}

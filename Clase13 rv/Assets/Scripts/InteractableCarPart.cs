using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Componente para partes interactuables del vehículo
/// Permite remover e instalar partes como tapa de aceite, varilla medidora, etc.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class InteractableCarPart : MonoBehaviour
{
    public enum PartType
    {
        OilCap,
        Dipstick,
        OilFilter,
        DrainPlug
    }

    [Header("Configuración")]
    [SerializeField] private PartType partType;
    [SerializeField] private OilChangeInteraction controller;
    [SerializeField] private Transform installedPosition;
    [SerializeField] private Transform removedPosition;

    [Header("Configuración de Instalación")]
    [SerializeField] private float snapDistance = 0.1f;
    [SerializeField] private bool requiresRotation = true;
    [SerializeField] private float rotationThreshold = 15f; // Grados

    [Header("Audio")]
    [SerializeField] private AudioSource partAudioSource;
    [SerializeField] private AudioClip removeSound;
    [SerializeField] private AudioClip installSound;
    [SerializeField] private AudioClip snapSound;

    [Header("Haptics")]
    [SerializeField] private float hapticIntensity = 0.5f;
    [SerializeField] private float hapticDuration = 0.2f;

    private XRGrabInteractable grabInteractable;
    private bool isInstalled = true;
    private bool isGrabbed = false;
    private Rigidbody rb;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        originalPosition = transform.position;
        originalRotation = transform.rotation;

        SetupInteractable();
    }

    private void SetupInteractable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;

        if (isInstalled)
        {
            // Intento de remoción
            AttemptRemoval(args.interactorObject as XRBaseInteractor);
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;

        if (!isInstalled)
        {
            // Intento de instalación
            AttemptInstallation();
        }
    }

    private void AttemptRemoval(XRBaseInteractor interactor)
    {
        if (!isInstalled) return;

        // Verificar si la acción es correcta según el estado del proceso
        bool canRemove = CanRemoveInCurrentState();

        if (canRemove)
        {
            PerformRemoval();
            TriggerHaptic(interactor);
            NotifyController(true);
        }
        else
        {
            // No se puede remover en este momento
            StartCoroutine(ReturnToPosition());
            NotifyController(false);
        }
    }

    private bool CanRemoveInCurrentState()
    {
        // Esta lógica debería verificar con el controlador principal
        // si esta parte puede ser removida en el estado actual
        return true; // Simplificado para el ejemplo
    }

    private void PerformRemoval()
    {
        isInstalled = false;

        if (removedPosition != null)
        {
            // Permitir movimiento libre después de remover
            rb.isKinematic = false;
        }

        PlaySound(removeSound);

        // Efectos visuales
        CreateRemovalEffect();
    }

    private void AttemptInstallation()
    {
        if (isInstalled || installedPosition == null) return;

        float distance = Vector3.Distance(transform.position, installedPosition.position);

        if (distance <= snapDistance)
        {
            bool correctRotation = !requiresRotation || CheckRotation();

            if (correctRotation)
            {
                PerformInstallation();
            }
            else
            {
                // Retroalimentación: rotación incorrecta
                PlaySound(null); // Sonido de error
            }
        }
    }

    private bool CheckRotation()
    {
        float angleDiff = Quaternion.Angle(transform.rotation, installedPosition.rotation);
        return angleDiff <= rotationThreshold;
    }

    private void PerformInstallation()
    {
        isInstalled = true;

        // Snap a la posición correcta
        transform.position = installedPosition.position;
        transform.rotation = installedPosition.rotation;

        rb.isKinematic = true;

        PlaySound(installSound);
        PlaySound(snapSound);

        CreateInstallationEffect();

        NotifyControllerInstallation();
    }

    private System.Collections.IEnumerator ReturnToPosition()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.position = Vector3.Lerp(startPos, originalPosition, t);
            transform.rotation = Quaternion.Lerp(startRot, originalRotation, t);

            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    private void NotifyController(bool success)
    {
        if (controller == null) return;

        switch (partType)
        {
            case PartType.OilCap:
                if (success)
                    controller.OnOilCapRemoved();
                break;
            case PartType.Dipstick:
                if (success)
                    controller.OnDipstickRemoved();
                break;
        }
    }

    private void NotifyControllerInstallation()
    {
        if (controller == null) return;

        switch (partType)
        {
            case PartType.OilCap:
                controller.OnOilCapReplaced();
                break;
        }
    }

    private void TriggerHaptic(XRBaseInteractor interactor)
    {
        if (interactor != null)
        {
            // Haptic feedback para el controlador
            var controller = interactor.GetComponent<XRBaseController>();
            if (controller != null)
            {
                controller.SendHapticImpulse(hapticIntensity, hapticDuration);
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (partAudioSource != null && clip != null)
        {
            partAudioSource.PlayOneShot(clip);
        }
    }

    private void CreateRemovalEffect()
    {
        // Aquí puedes agregar partículas o efectos visuales al remover
        // Por ejemplo: polvo, chispas, etc.
    }

    private void CreateInstallationEffect()
    {
        // Aquí puedes agregar partículas o efectos visuales al instalar
        // Por ejemplo: destello, partículas de confirmación, etc.
    }

    private void OnDrawGizmosSelected()
    {
        // Visualización en el editor de Unity
        if (installedPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(installedPosition.position, snapDistance);
            Gizmos.DrawLine(transform.position, installedPosition.position);
        }
    }
}

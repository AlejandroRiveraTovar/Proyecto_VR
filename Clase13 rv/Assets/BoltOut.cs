using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Controla el ciclo del tornillo cuando es liberado o removido.
/// - Detecta cuando el tornillo sale del trigger o del socket.
/// - Cambia entre la herramienta XRLever (interactabletool) y la herramienta normal (grabbabletool).
/// - Reactiva el tornillo suelto como XRGrabInteractable.
/// - Reactiva el socket final cuando es seguro.
/// </summary>
public class BoltOut : MonoBehaviour
{
    [Header("Tool / Bolt References")]
    public GameObject grabbabletool;      // La herramienta GRABBABLE final
    public GameObject interactabletool;   // XRLever versión herramienta activa
    public XRSocketInteractor boltsocketInteractor;

    private void Start()
    {
        if (boltsocketInteractor != null)
            boltsocketInteractor.selectExited.AddListener(OnBoltReleasedFromSocket);
    }

    /// <summary>
    /// Cuando el tornillo sale del trigger del espacio donde estaba encajado.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bolts"))
        {
            StartCoroutine(ReleaseBoltSafe(other.gameObject));
        }
    }

    /// <summary>
    /// Cuando el tornillo es retirado del socket final.
    /// </summary>
    private void OnBoltReleasedFromSocket(SelectExitEventArgs args)
    {
        GameObject bolt = args.interactableObject.transform.gameObject;
        StartCoroutine(ReleaseBoltSafe(bolt));
    }

    /// <summary>
    /// Maneja el proceso seguro de soltar, rehabilitar y activar el tornillo.
    /// Esto evita los bugs comunes en XR (jitter, pegado, rotación incorrecta).
    /// </summary>
    private IEnumerator ReleaseBoltSafe(GameObject bolt)
    {
        // XR necesita 2 frames para completar sus ciclos internos
        yield return null;
        yield return null;

        if (bolt == null)
            yield break;

        Rigidbody rb = bolt.GetComponent<Rigidbody>();
        Collider col = bolt.GetComponent<Collider>();
        XRGrabInteractable grab = bolt.GetComponent<XRGrabInteractable>();

        // Seguridad
        if (rb == null || col == null || grab == null)
            yield break;

        // Convertir el tornillo en "suelto"
        rb.isKinematic = false;
        col.isTrigger = false;

        // Cambiar herramientas (XRLever  Grabbable)
        if (grabbabletool != null)
            grabbabletool.SetActive(true);

        if (interactabletool != null)
            interactabletool.SetActive(false);

        // Reactivar el grab interactable del tornillo
        yield return null;
        grab.enabled = true;

        // Reactivar el socket
        yield return null;
        if (boltsocketInteractor != null)
            boltsocketInteractor.enabled = true;
    }
}

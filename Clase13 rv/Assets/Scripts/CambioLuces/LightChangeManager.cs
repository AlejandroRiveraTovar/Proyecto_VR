using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Administra el comportamiento del cambio de luz dependiendo del estado de los tornillos (BoltOut).
/// Controla:
/// - El socket que recibe la luz.
/// - El objeto de luz insertado.
/// - Materiales según el tipo de bombillo.
/// - Cambio de Interaction Layers según si los tornillos están ajustados.
/// </summary>
public class LightChangeManager : MonoBehaviour
{
    /// <summary>
    /// Lista de tornillos que deben estar completamente retirados para permitir el cambio de luz.
    /// </summary>
    public List<BoltOut> boltOuts = new List<BoltOut>();

    /// <summary>
    /// Capa interactuable del socket y bombillo cuando SE PERMITE interactuar.
    /// </summary>
    public InteractionLayerMask layerMask;

    /// <summary>
    /// Capa interactuable del socket y bombillo cuando NO se permite interactuar.
    /// </summary>
    public InteractionLayerMask layerMask2;

    /// <summary>
    /// Socket XR donde se inserta la luz.
    /// </summary>
    public XRSocketInteractor lightSpace;

    /// <summary>
    /// Objeto XR insertado actualmente en el socket (si existe).
    /// </summary>
    private XRGrabInteractable light;

    /// <summary>
    /// Material aplicado si la luz insertada es válida (GoodLight).
    /// </summary>
    public Material lMat;

    /// <summary>
    /// Material aplicado cuando no hay luz o es incorrecta.
    /// </summary>
    public Material dlMat;


    /// <summary>
    /// Sonido que se reproduce al insertar la luz correcta..
    /// </summary>
    public AudioSource audioSource;

    /// <summary>
    /// Obtiene el socket al inicio para evitar referencias nulas.
    /// </summary>
    private void Start()
    {
        lightSpace = GetComponent<XRSocketInteractor>();
    }


    /// <summary>
    /// Actualiza cada frame:
    /// - Detecta si hay una luz insertada en el socket.
    /// - Cambia el material según el tipo de luz.
    /// - Verifica si los tornillos están libres para permitir o bloquear el cambio.
    /// </summary>
    private void Update()
    {
        // ---------------------------------------------------------
        // Obtener el objeto actualmente insertado en el socket
        // ---------------------------------------------------------
        var selected = lightSpace.GetOldestInteractableSelected();

        if (selected != null)
        {
            light = selected.transform.GetComponent<XRGrabInteractable>();

            // Cambiar material si es una luz válida
            if (light.CompareTag("GoodLight"))
            {
                light.gameObject.GetComponent<MeshRenderer>().material = lMat;
                audioSource.Play();
            }
            else
            {
                light.gameObject.GetComponent<MeshRenderer>().material = dlMat;
            }
        }
        else
        {
            light = null;
        }

        // ---------------------------------------------------------
        // Revisar estado de los tornillos
        // Si cualquiera está aún colocado  bloquear interacción
        // ---------------------------------------------------------
        for (int i = 0; i < boltOuts.Count; i++)
        {
            if (boltOuts[i].setBolt)
            {
                DisableLightChange();
                return;
            }
        }

        // Si ninguno está colocado activar el cambio de luz
        EnableLightChange();
    }


    /// <summary>
    /// Permite la interacción con el socket y la luz.
    /// </summary>
    private void EnableLightChange()
    {
        // Habilitar el socket
        lightSpace.interactionLayers = layerMask;

        // Habilitar la luz si está presente
        if (light != null)
            light.interactionLayers = layerMask;
    }


    /// <summary>
    /// Bloquea la interacción con el socket y la luz.
    /// Esto ocurre cuando los tornillos están aún colocados.
    /// </summary>
    private void DisableLightChange()
    {
        // Bloquear el socket
        lightSpace.interactionLayers = layerMask2;

        // Bloquear la luz si está insertada
        if (light != null)
            light.interactionLayers = layerMask2;
    }
}

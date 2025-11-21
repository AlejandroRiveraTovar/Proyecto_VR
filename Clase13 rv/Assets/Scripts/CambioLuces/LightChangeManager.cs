using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class LightChangeManager : MonoBehaviour
{
    public List<BoltOut> boltOuts = new List<BoltOut>();
    public InteractionLayerMask layerMask;
    public InteractionLayerMask layerMask2;
    public XRSocketInteractor lightSpace;  // Ahora es directamente el socket
    private XRGrabInteractable light;      // Este será el objeto insertado dinámicamente
    public Material lMat;
    public Material dlMat;


    private void Start()
    {
        lightSpace = GetComponent<XRSocketInteractor>();
    }
    void Update()
    {
        // Obtener objeto insertado en el socket
        var selected = lightSpace.GetOldestInteractableSelected();
        if (selected != null)
        {
            light = selected.transform.GetComponent<XRGrabInteractable>();
            if (light.CompareTag("GoodLight"))
            {
                light.gameObject.GetComponent<MeshRenderer>().material = lMat;
            }
        }
        else
        {
            light = null;
        }

        // Verificar los tornillos
        for (int i = 0; i < boltOuts.Count; i++)
        {
            if (boltOuts[i].setBolt)
            {
                DisableLightChange();
                return;
            }
        }

        EnableLightChange();
    }

    private void EnableLightChange()
    {
        // Habilitar socket
        lightSpace.interactionLayers = layerMask;

        // Si hay un objeto insertado, aplicarle la capa
        if (light != null)
            light.interactionLayers = layerMask;
           
    }

    private void DisableLightChange()
    {
        // Deshabilitar socket
        lightSpace.interactionLayers = layerMask2;

        // Si hay un objeto insertado, aplicarle la capa
        if (light != null)
            light.interactionLayers = layerMask2;
    }
}

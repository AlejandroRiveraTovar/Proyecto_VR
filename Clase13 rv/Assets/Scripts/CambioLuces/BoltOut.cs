using System.Collections;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;
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
    public GameObject boltSocket;
    public GameObject boltGrabbable;
    public GameObject toolGrabbable;
    public GameObject toolLever;
    public Transform BoltSet;

    public bool setBolt;
    private void Start()
    {
        setBolt = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bolts")) 
        {
            boltGrabbable.SetActive(true);
            boltSocket.SetActive(false);
            toolGrabbable.SetActive(true);
            toolLever.SetActive(false);
            setBolt = false;
        }

    }

    void Update()
    {
        if (boltSocket.transform.position.z <= BoltSet.position.z && toolLever.GetComponent<BoltTorqueMover>().InvertirDireccion == true)
        {
            toolLever.SetActive(false);
            toolGrabbable.SetActive(true);
            toolLever.GetComponent<BoltTorqueMover>().InvertirDireccion = false;
            boltSocket.GetComponent<XRSocketInteractor>().socketActive = true;
            setBolt = true;
        }
    }
}

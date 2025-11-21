using System.Collections;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Controla el estado de un perno ("bolt") que puede ser retirado y colocado.
/// Reacciona a eventos de disparador, habilita/deshabilita objetos relacionados,
/// y detecta si el perno ha sido reinsertado correctamente mediante posición y torque.
/// </summary>
public class BoltOut : MonoBehaviour
{
    /// <summary>
    /// Socket donde el perno debe ser colocado cuando está ajustado.
    /// </summary>
    public GameObject boltSocket;

    /// <summary>
    /// Objeto agarrable que representa el perno cuando está fuera.
    /// </summary>
    public GameObject boltGrabbable;

    /// <summary>
    /// Herramienta que el usuario puede tomar para interactuar con el perno.
    /// </summary>
    public GameObject toolGrabbable;

    /// <summary>
    /// Herramienta o palanca utilizada para girar el perno dentro del socket.
    /// </summary>
    public GameObject toolLever;

    /// <summary>
    /// Transform objetivo que indica la posición donde el perno se considera asentado correctamente.
    /// </summary>
    public Transform BoltSet;

    /// <summary>
    /// Determina si el perno está colocado en su posición correcta.
    /// True = perno colocado, False = perno retirado.
    /// </summary>
    public bool setBolt;

    /// <summary>
    /// Inicializa el estado del perno en "colocado".
    /// </summary>
    private void Start()
    {
        setBolt = true;
    }

    /// <summary>
    /// Detecta cuando el objeto con tag "Bolts" sale del área del trigger.
    /// Esto ocurre cuando el usuario extrae el perno.
    /// </summary>
    /// <param name="other">Collider que salió del trigger.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bolts"))
        {
            // El perno sale: se activa la versión agarrable
            boltGrabbable.SetActive(true);
            boltSocket.SetActive(false);

            // Se habilita la herramienta y se deshabilita la palanca de torque
            toolGrabbable.SetActive(true);
            toolLever.SetActive(false);

            // Marcamos el perno como retirado
            setBolt = false;
        }
    }

    /// <summary>
    /// Revisa cada frame si el perno ha sido reinsertado.
    /// Se comprueba la posición del socket y el estado de la herramienta.
    /// Cuando las condiciones se cumplen, se considera que el perno está nuevamente colocado.
    /// </summary>
    void Update()
    {
        // Verifica: el socket ha alcanzado la posición correcta y la herramienta está girando en la dirección correcta
        if (boltSocket.transform.position.z <= BoltSet.position.z &&
            toolLever.GetComponent<BoltTorqueMover>().InvertirDireccion == true)
        {
            // Apaga la palanca de torque y habilita la herramienta normal
            toolLever.SetActive(false);
            toolGrabbable.SetActive(true);

            // Resetea el sentido de giro
            toolLever.GetComponent<BoltTorqueMover>().InvertirDireccion = false;

            // Reactiva el socket para permitir interacción XR
            boltSocket.GetComponent<XRSocketInteractor>().socketActive = true;

            // Marca el perno como colocado correctamente
            setBolt = true;
        }
    }
}

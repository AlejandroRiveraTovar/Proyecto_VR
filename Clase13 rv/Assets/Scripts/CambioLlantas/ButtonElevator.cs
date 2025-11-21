using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Controla el movimiento vertical de un objeto usando botones u otros eventos
public class ButtonElevator : MonoBehaviour
{
    // Referencia al objeto que se desea mover en el eje Y
    public Transform objeto;

    // Velocidad a la que se moverá el objeto
    public float speed = 1f;

    // Variable que indica si el objeto debe subir
    private bool subir;
    // Variable que indica si el objeto debe bajar
    private bool bajar;

    void Update()
    {
        // Si 'subir' está activa, mueve el objeto hacia arriba (Vector3.up)
        // Se multiplica por speed para ajustar la velocidad
        // Se multiplica por Time.deltaTime para asegurar un movimiento suave e independiente del framerate
        if (subir)
            objeto.position += Vector3.up * speed * Time.deltaTime;

        // Si 'bajar' está activa, mueve el objeto hacia abajo (Vector3.down)
        if (bajar)
            objeto.position += Vector3.down * speed * Time.deltaTime;
    }

    // Activa para iniciar el movimiento hacia arriba
    public void StartMoveUp()
    {
        subir = true;
    }

    // Desactiva para detener el movimiento hacia arriba
    public void StopMoveUp()
    {
        subir = false;
    }

    // Activa para iniciar el movimiento hacia abajo
    public void StartMoveDown()
    {
        bajar = true;
    }

    // Desactiva para detener el movimiento hacia abajo
    public void StopMoveDown()
    {
        bajar = false;
    }
}

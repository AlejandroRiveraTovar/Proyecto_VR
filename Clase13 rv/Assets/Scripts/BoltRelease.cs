using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Detecta si el objeto con HingeJoint rota en -Z y mueve otro objeto hacia adelante
/// proporcional al torque aplicado.
/// </summary>
public class BoltTorqueMover : MonoBehaviour
{
    [Header("Referencias")]
    public HingeJoint hinge;                // El tornillo / perno XR con hinge
    public Transform objetoAMover;          // El objeto que se moverá hacia adelante

    [Header("Parámetros")]
    public float sensibilidadTorque = 0.001f; // Qué tanto se mueve el objeto
    public float velocidadMax = 0.1f;         // Límite de velocidad de empuje
    public bool invertirDireccion = false;    // Si el movimiento debe invertirse

    private float rotacionAnterior;

    void Start()
    {
        if (hinge == null)
            hinge = GetComponent<HingeJoint>();

        rotacionAnterior = hinge.angle;
    }

    void Update()
    {
        float rotacionActual = hinge.angle;
        float deltaRot = rotacionActual - rotacionAnterior;

        // Detecta rotación en dirección -Z (deltaRot negativo)
        if (deltaRot < 0f)
        {
            // Medir torque aproximado del usuario
            float torque = Mathf.Abs(deltaRot);

            // Calcular desplazamiento
            float desplazamiento = torque * sensibilidadTorque;
            desplazamiento = Mathf.Clamp(desplazamiento, 0, velocidadMax);

            Vector3 direccion = invertirDireccion ? -objetoAMover.forward : objetoAMover.forward;

            objetoAMover.position += direccion * desplazamiento * Time.deltaTime;
        }

        rotacionAnterior = rotacionActual;
    }
}

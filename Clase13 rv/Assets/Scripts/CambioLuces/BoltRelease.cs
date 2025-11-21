using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Controla dos comportamientos simultáneos:
/// 1. Avance de un objeto en función del torque del HingeJoint.
/// 2. Rotación simple en el eje Z basada en la dirección de giro del usuario.
///
/// Este script es seguro contra NaN y rotaciones inválidas.
/// </summary>
public class BoltTorqueMover : MonoBehaviour
{
    // -------------------------------------------------------
    //  REFERENCIAS
    // -------------------------------------------------------

    /// <summary>
    /// HingeJoint que representa el perno o herramienta giratoria.
    /// La rotación del hinge se usa para medir torque.
    /// </summary>
    [Header("Referencias")]
    public HingeJoint hinge;

    /// <summary>
    /// Objeto que se moverá hacia adelante y rotará al aplicar torque.
    /// </summary>
    public Transform objetoAMover;


    // -------------------------------------------------------
    //  PARÁMETROS DE COMPORTAMIENTO
    // -------------------------------------------------------

    /// <summary>
    /// Factor que determina cuánta distancia avanza el objeto
    /// por unidad de torque detectado.
    /// </summary>
    [Header("Parámetros")]
    public float sensibilidadTorque = 0.001f;

    /// <summary>
    /// Velocidad máxima permitida para el movimiento hacia adelante.
    /// </summary>
    public float velocidadMax = 0.1f;

    /// <summary>
    /// Velocidad (en grados/segundo) a la que el objeto rota en el eje Z.
    /// </summary>
    public float velocidadRotacion = 45f;

    /// <summary>
    /// Indica si el movimiento hacia adelante debe invertirse.
    /// </summary>
    [SerializeField]
    private bool _invertirDireccion = false;

    /// <summary>
    /// Sonido que se reproduce al aplicar torque.
    /// </summary>
    public AudioSource audioSource;


    /// <summary>
    /// Propiedad pública que controla el estado de inversión del movimiento
    /// y notifica cambios mediante el evento OnDireccionCambiada.
    /// </summary>
    public bool InvertirDireccion
    {
        get => _invertirDireccion;
        set
        {
            if (_invertirDireccion != value)
            {
                _invertirDireccion = value;
                OnDireccionCambiada?.Invoke(value);
            }
        }
    }

    /// <summary>
    /// Evento que se dispara cuando la dirección de movimiento es cambiada externamente.
    /// </summary>
    public event System.Action<bool> OnDireccionCambiada;


    // -------------------------------------------------------
    //  VARIABLES INTERNAS
    // -------------------------------------------------------

    /// <summary>
    /// Ángulo previo del hinge usado para calcular el delta de rotación.
    /// </summary>
    private float rotacionAnterior;


    // -------------------------------------------------------
    //  MÉTODOS PRINCIPALES
    // -------------------------------------------------------

    /// <summary>
    /// Inicializa el hinge y almacena el ángulo inicial.
    /// </summary>
    void Start()
    {
        if (hinge == null)
            hinge = GetComponent<HingeJoint>();

        rotacionAnterior = hinge.angle;
    }

    /// <summary>
    /// Actualiza el movimiento y la rotación del objeto basado en el cambio de rotación del hinge.
    /// </summary>
    void Update()
    {
        float rotacionActual = hinge.angle;
        float deltaRot = rotacionActual - rotacionAnterior;

        // -------------------------------------------------------
        // 1. Movimiento hacia adelante según torque
        // -------------------------------------------------------
        if (deltaRot < 0f)
        {
            float torque = Mathf.Abs(deltaRot);
            float desplazamiento = torque * sensibilidadTorque;
            desplazamiento = Mathf.Clamp(desplazamiento, 0, velocidadMax);

            Vector3 direccion = _invertirDireccion ? -objetoAMover.forward : objetoAMover.forward;

            objetoAMover.position += direccion * desplazamiento * Time.deltaTime;
            audioSource.Play();
        }

        // -------------------------------------------------------
        // 2. Rotación suave en Z 
        // -------------------------------------------------------
        if (Mathf.Abs(deltaRot) > 0.0001f)
        {
            float dir = deltaRot < 0f ? -1f : 1f;

            objetoAMover.Rotate(0f, 0f, dir * velocidadRotacion * Time.deltaTime, Space.Self);
        }

        rotacionAnterior = rotacionActual;
    }



    /// <summary>
    /// Asigna manualmente si el movimiento debe invertirse.
    /// </summary>
    /// <param name="valor">True para invertir, False para normal.</param>
    public void SetInvertirDireccion(bool valor) => InvertirDireccion = valor;

    /// <summary>
    /// Alterna entre dirección normal e invertida.
    /// </summary>
    public void ToggleInvertirDireccion() => InvertirDireccion = !InvertirDireccion;
}

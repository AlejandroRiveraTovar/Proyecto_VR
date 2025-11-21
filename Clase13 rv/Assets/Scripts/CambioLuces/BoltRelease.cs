using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BoltTorqueMover : MonoBehaviour
{
    [Header("Referencias")]
    public HingeJoint hinge;
    public Transform objetoAMover;

    [Header("Parámetros")]
    public float sensibilidadTorque = 0.001f;
    public float velocidadMax = 0.1f;
    public float velocidadRotacion = 45f; // grados por segundo

    [SerializeField]
    private bool _invertirDireccion = false;

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

    public event System.Action<bool> OnDireccionCambiada;

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

        //-------------------------------------------
        // Movimiento hacia adelante (tu lógica)
        //-------------------------------------------
        if (deltaRot < 0f)
        {
            float torque = Mathf.Abs(deltaRot);
            float desplazamiento = torque * sensibilidadTorque;
            desplazamiento = Mathf.Clamp(desplazamiento, 0, velocidadMax);

            Vector3 direccion = _invertirDireccion ? -objetoAMover.forward : objetoAMover.forward;
            objetoAMover.position += direccion * desplazamiento * Time.deltaTime;
        }

        //-------------------------------------------
        // ROTACIÓN SIMPLE EN Z (sin NaN, sin asserts)
        //-------------------------------------------
        if (Mathf.Abs(deltaRot) > 0.0001f)
        {
            float dir = deltaRot < 0f ? -1f : 1f;

            // Aplicar rotación directa en Z
            objetoAMover.Rotate(0f, 0f, dir * velocidadRotacion * Time.deltaTime, Space.Self);
        }

        rotacionAnterior = rotacionActual;
    }

    // Métodos públicos para control externo
    public void SetInvertirDireccion(bool valor) => InvertirDireccion = valor;
    public void ToggleInvertirDireccion() => InvertirDireccion = !InvertirDireccion;
}

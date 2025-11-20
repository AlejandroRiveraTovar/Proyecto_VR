using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Content.Interaction;

public class LeverController : MonoBehaviour
{
    public XRLever palanca;   
    public float upSpeed = 1f;
    public float downSpeed = 1f;

    public float minY = 0f;
    public float maxY = 5f;

    void Update()
    {
        float velocidadY = 0f;

        // Si la palanca está activada → subir
        if (palanca.value)
        {
            velocidadY = upSpeed;
        }
        else
        {
            // Si la palanca está suelta → bajar
            velocidadY = -downSpeed;
        }

        // Aplicar movimiento
        transform.position += new Vector3(0, velocidadY, 0) * Time.deltaTime;

        // Limitar movimiento
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;

    }
}

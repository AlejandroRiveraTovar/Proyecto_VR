using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Content.Interaction;

public class LeverController : MonoBehaviour
{
    // Velocidad de movimiento en el eje Y
    public float speed = 1f;
    // Límite inferior de la posición Y del objeto
    public float minY = 0f;
    // Límite superior de la posición Y del objeto
    public float maxY = 5f;

    // Dirección del movimiento
    private int direccion = 0;

    // Método público que activa el movimiento hacia arriba
    public void SubirPuerta()
    {
        direccion = 1;   // Establece la dirección a positiva
    }

    // Método público que activa el movimiento hacia abajo
    public void BajarPuerta()
    {
        direccion = -1;    // Establece la dirección a negativa
    }

    void Update()
    {
        // Solo se mueve si la dirección no es cero
        if (direccion != 0)
        {
            // Suma al transform una posición en Y según la velocidad, dirección y deltaTime
            transform.position += new Vector3(0, direccion * speed, 0) * Time.deltaTime;
            // Guarda la posición actual en una variable temporal
            var pos = transform.position;
            // Restringe (clamp) la posición Y para que no salga de los límites minY y maxY
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            // Asigna la posición corregida al transform
            transform.position = pos;
        }
    }
}

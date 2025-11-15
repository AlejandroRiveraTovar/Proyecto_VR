using UnityEngine;

/// <summary>
/// Ejemplo: libera un tornillo o faro cuando la rotación se completa.
/// </summary>
public class BoltRelease : MonoBehaviour
{
    public void Release()
    {
        Debug.Log("Tornillo aflojado");
        // ejemplo: permitir desmontar la pieza
        GetComponent<Collider>().enabled = false;
        transform.parent = null;
    }
}

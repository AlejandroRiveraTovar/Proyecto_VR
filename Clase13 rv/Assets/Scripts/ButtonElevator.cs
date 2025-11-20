using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ButtonElevator : MonoBehaviour
{
    public Transform objeto;     // lo que quiere mover
    public float speed = 1f;

    private bool subir;
    private bool bajar;

    void Update()
    {
        if (subir)
            objeto.position += Vector3.up * speed * Time.deltaTime;

        if (bajar)
            objeto.position += Vector3.down * speed * Time.deltaTime;
    }

    public void StartMoveUp()
    {
        subir = true;
    }

    public void StopMoveUp()
    {
        subir = false;
    }

    public void StartMoveDown()
    {
        bajar = true;
    }

    public void StopMoveDown()
    {
        bajar = false;
    }
}

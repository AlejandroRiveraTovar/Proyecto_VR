using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class tire : MonoBehaviour
{
    public Rigidbody rb;
    public GameManager GM;
    public GameObject caliperAndPads;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    private void Update()
    {
        Debug.Log("Grav = " + rb.useGravity + ", Kin = " + rb.isKinematic);
    }

    public void EnableRBGravity()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        GM.score += 5;
        GM.UpdateScore();
        caliperAndPads.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = true;

        Debug.Log("Tire Gravity...COMMENCE!");
    }
}

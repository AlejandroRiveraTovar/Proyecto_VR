using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


public class tire : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject caliperAndPads;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void EnableRBGravity()
    {
        rb.useGravity = true;
        rb.isKinematic = false;

        XRGrabInteractable grab = caliperAndPads.GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.enabled = true;
    }
}

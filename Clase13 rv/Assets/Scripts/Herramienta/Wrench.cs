using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Wrench : MonoBehaviour
{
    public GameObject tire;

    private bool wrenchOn = false;
    private int nutsRemovedCount = 0;

    [SerializeField] XRController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (!wrenchOn) return;

        if (other.CompareTag("Nut"))
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            Collider col = other.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = false;

            XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
            if (grab != null)
                grab.enabled = true;

            nutsRemovedCount++;

            if (nutsRemovedCount >= 5)
            {
                XRGrabInteractable tireGrab = tire.GetComponent<XRGrabInteractable>();
                if (tireGrab != null)
                    tireGrab.enabled = true;
            }
        }
    }

    public void StartHaptics()
    {
        wrenchOn = true;
        InvokeRepeating(nameof(SendHaptics), 0f, 0.2f);
    }

    public void StopHaptics()
    {
        wrenchOn = false;
        CancelInvoke(nameof(SendHaptics));
    }

    private void SendHaptics()
    {
        controller?.SendHapticImpulse(0.7f, 0.2f);
    }
}

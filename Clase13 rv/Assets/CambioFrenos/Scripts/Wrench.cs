using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Wrench : MonoBehaviour
{
    public GameObject nutButton1;
    public GameObject nutButton2;
    public GameObject nutButton3;
    public GameObject nutButton4;
    public GameObject nutButton5;
    public GameObject tire;
    public Collider otherCollider;
    private bool wrenchOn;
    public GameManager GM;

    public int nutsRemovedCount = 0;


    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable Grabbable;

    public bool wrenchOnNut;
    [SerializeField] XRBaseController controller;

    private void Start()
    {
        
        wrenchOn = false;
        //Grabbable = GetComponent<XRGrabInteractable>();
    }


    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("SOMETHING HIT THAT IS A TRIGGER");

        if (other.CompareTag("Nut") && wrenchOn == true)
        {
            //Debug.Log("Entered Nut");
            wrenchOnNut = true;
            other.attachedRigidbody.isKinematic = false;
            other.attachedRigidbody.useGravity = true;
            otherCollider = other.GetComponent<BoxCollider>();
            otherCollider.isTrigger = false;
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable Grabbable = other.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            Grabbable.enabled = true;
            nutsRemovedCount++;
            //Add points
            GM.score += 1;
            GM.UpdateScore();

            if (nutsRemovedCount >= 5)
            {
                Debug.Log("All Nuts Removed. Tire Grabbable!");
                tire.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = true;
                //tire.GetComponent<Rigidbody>().isKinematic = false;
                //tire.GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Nut"))
        {
            //Debug.Log("Exited Nut");
            wrenchOnNut = false;

        }
    }

    public void StartHaptics()
    {
        InvokeRepeating("SendHaptics", 0.0f, 0.2f);
        wrenchOn = true;
    }

    public void StopHaptics()
    {
        wrenchOn = false;
        CancelInvoke();
    }

    public void SendHaptics()
    {
        if (controller != null)
        {
            controller.SendHapticImpulse(0.7f, 0.2f);
            //Debug.Log("Haptics?");
        }
    }
}

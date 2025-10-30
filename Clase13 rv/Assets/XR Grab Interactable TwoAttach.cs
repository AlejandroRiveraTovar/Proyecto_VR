using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Al seleccionar, asigna el attachTransform según la mano (left/right).
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class SnapToHandGrab : XRGrabInteractable
{
    [Header("Attach points")]
    public Transform attachPointLeft;
    public Transform attachPointRight;
    [Header("Interactors")]
    public XRBaseInteractor leftHandInteractor;
    public XRBaseInteractor rightHandInteractor;

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        XRBaseInteractor interactor = args.interactorObject as XRBaseInteractor;

        if (interactor != null)
        {
            
            if (interactor == leftHandInteractor && attachPointLeft != null)
            {
                attachTransform = attachPointLeft;
            }
            else if (interactor == rightHandInteractor && attachPointRight != null)
            {
                attachTransform = attachPointRight;
            }
        }
        base.OnSelectEntering(args);
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
    }
}

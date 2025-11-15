using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public class RotatingSocketInteractor : XRSocketInteractor
{
    [Header("Rotación")]
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 1.0f;

    [Header("Tornillo")]
    public Transform screwPivot;
    public float screwLoosenDistance = 0.02f;
    public float requiredRotation = 360f;

    public UnityEvent OnScrewFullyRemoved;

    Quaternion lastToolRotation;
    float currentRotation = 0f;
    bool hasTool = false;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        hasTool = true;
        lastToolRotation = args.interactableObject.transform.rotation;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        hasTool = false;
    }

    void Update()
    {
        if (!hasTool || firstInteractableSelected == null)
            return;

        Transform tool = firstInteractableSelected.transform;

        Quaternion currentRot = tool.rotation;
        Quaternion deltaRot = currentRot * Quaternion.Inverse(lastToolRotation);
        deltaRot.ToAngleAxis(out float angleDeg, out Vector3 axis);

        // Convertir rango >180 a rango -180 a 180
        if (angleDeg > 180f) angleDeg -= 360f;

        // Aplicar deadzone para evitar temblores
        if (Mathf.Abs(angleDeg) < 1.5f) // prueba valores entre 0.5 y 3
            return;

        // Determinar dirección relativa al eje configurado
        float direction = Mathf.Sign(Vector3.Dot(axis, transform.TransformDirection(rotationAxis)));

        float appliedRotation = angleDeg * direction * rotationSpeed;

        currentRotation += appliedRotation;
        lastToolRotation = currentRot;

        // Aplicar rotación al tornillo
        if (screwPivot)
        {
            screwPivot.localRotation = Quaternion.AngleAxis(currentRotation, rotationAxis);

            float t = Mathf.Clamp01(Mathf.Abs(currentRotation) / requiredRotation);
            screwPivot.localPosition = new Vector3(0, 0, t * screwLoosenDistance);
        }

        if (Mathf.Abs(currentRotation) >= requiredRotation)
        {
            if (screwPivot.TryGetComponent(out Rigidbody rb))
                rb.isKinematic = false;

            OnScrewFullyRemoved?.Invoke();
            enabled = false;
        }
    }

}


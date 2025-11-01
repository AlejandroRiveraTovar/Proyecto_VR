using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Gestiona la transici�n entre poses de mano al interactuar con un objeto en XR.
/// Permite ajustar la posici�n, rotaci�n y configuraci�n de los dedos cuando una mano
/// agarra o suelta un objeto.
/// </summary>
/// <remarks>
/// Este script debe estar asociado a un objeto con un componente <see cref="XRGrabInteractable"/>.
/// Se utiliza junto con <see cref="HandData"/> para definir las poses de la mano izquierda y derecha.
/// </remarks>
public class GrabHandPose : MonoBehaviour
{
    [Header("HandPoses")]
    /// <summary>
    /// Datos de la pose de la mano izquierda que se aplicar�n al agarrar el objeto.
    /// </summary>
    public HandData leftHandData;

    /// <summary>
    /// Datos de la pose de la mano derecha que se aplicar�n al agarrar el objeto.
    /// </summary>
    public HandData rightHandData;

    // Variables internas para almacenar la posici�n y rotaci�n inicial/final
    private Vector3 startingPos;
    private Quaternion startingRot;
    private Vector3 endingPos;
    private Quaternion endingRot;

    private quaternion[] startingFingerRot;
    private quaternion[] endingFingerRot;

    /// <summary>
    /// Inicializa el componente y registra los eventos de agarre y liberaci�n del objeto.
    /// </summary>
    private void Start()
    {
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(SetUpPose);
        grabInteractable.selectExited.AddListener(UnSetPose);

        leftHandData.gameObject.SetActive(false);
        rightHandData.gameObject.SetActive(false);
    }

    /// <summary>
    /// Configura la pose de la mano al agarrar el objeto.
    /// </summary>
    /// <param name="args">Argumentos del evento que contienen informaci�n del interactor.</param>
    public void SetUpPose(BaseInteractionEventArgs args)
    {
        XRBaseInteractor interactor = args.interactorObject as XRBaseInteractor;
        if (interactor != null)
        {
            Debug.Log(args.interactorObject.transform.name);
            HandData handData = args.interactorObject.transform.GetComponent<HandData>();
            handData.animator.enabled = false;

            if (handData.type == HandData.HandType.left)
            {
                SetHandDataValues(handData, leftHandData);
            }
            else
            {
                SetHandDataValues(handData, rightHandData);
            }

            SetHandPose(handData, endingPos, endingRot, endingFingerRot);
        }
    }

    /// <summary>
    /// Restaura la pose de la mano a su posici�n original al soltar el objeto.
    /// </summary>
    /// <param name="args">Argumentos del evento que contienen informaci�n del interactor.</param>
    public void UnSetPose(BaseInteractionEventArgs args)
    {
        XRBaseInteractor interactor = args.interactorObject as XRBaseInteractor;
        if (interactor != null)
        {
            Debug.Log(args.interactorObject.transform.name);
            HandData handData = args.interactorObject.transform.GetComponent<HandData>();
            handData.animator.enabled = true;

            SetHandPose(handData, startingPos, startingRot, startingFingerRot);
        }
    }

    /// <summary>
    /// Copia las posiciones, rotaciones y datos de los huesos de una mano de referencia a otra.
    /// </summary>
    /// <param name="d1">Datos de la mano que realizar� el agarre (mano activa).</param>
    /// <param name="d2">Datos de la pose objetivo (referencia).</param>
    public void SetHandDataValues(HandData d1, HandData d2)
    {
        startingPos = d1.root.localPosition;
        endingPos = d2.root.localPosition;

        startingRot = d1.root.localRotation;
        endingRot = d2.root.localRotation;

        startingFingerRot = new quaternion[d1.fingerBones.Length];
        endingFingerRot = new quaternion[d2.fingerBones.Length];

        for (int i = 0; i < d1.fingerBones.Length; i++)
        {
            startingFingerRot[i] = d1.fingerBones[i].localRotation;
            endingFingerRot[i] = d2.fingerBones[i].localRotation;
        }
    }

    /// <summary>
    /// Aplica una pose espec�fica a la mano, modificando la posici�n, rotaci�n y orientaci�n de los dedos.
    /// </summary>
    /// <param name="h">Instancia de <see cref="HandData"/> que representa la mano a modificar.</param>
    /// <param name="newPos">Nueva posici�n local de la ra�z de la mano.</param>
    /// <param name="newRot">Nueva rotaci�n local de la ra�z de la mano.</param>
    /// <param name="newBoneRot">Arreglo de rotaciones locales de cada hueso del dedo.</param>
    public void SetHandPose(HandData h, Vector3 newPos, quaternion newRot, quaternion[] newBoneRot)
    {
        h.root.localPosition = newPos;
        h.root.localRotation = newRot;

        for (int i = 0; i < newBoneRot.Length; i++)
        {
            h.fingerBones[i].localRotation = newBoneRot[i];
        }
    }
}

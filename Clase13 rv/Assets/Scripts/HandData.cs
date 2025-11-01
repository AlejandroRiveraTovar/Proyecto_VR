using UnityEngine;

public class HandData : MonoBehaviour
{
    /// <summary>
    /// Almacena la posicion, el animator y el tipo de mano
    /// </summary>
    public enum HandType { left, right }

    [Header("Hand data")]
    public HandType type;
    public Transform root;
    public Animator animator;
    [Header("Fingers")]
    public Transform[] fingerBones;

}

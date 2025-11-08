using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Content.Interaction;

public class LeverController : MonoBehaviour
{
    public XRLever palanca;

    public float upSpeed;

    void Update()
    {
        float speedArriba = upSpeed * (palanca.value ? 1 : 0);

        Vector3 velocity = new Vector3(0, speedArriba, 0);
        transform.position += velocity * Time.deltaTime;

    }
}

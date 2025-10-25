using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Pistola : MonoBehaviour
{
    public GameObject ShootFx, Hitfx;
    public Transform firepoint;
    public LineRenderer line;
    public int damage;
    void Start()
    {
        XRGrabInteractable grabInteract = GetComponent<XRGrabInteractable>();
        grabInteract.activated.AddListener(x => Disparando());
    }

    public void Disparando() 
    {
        StartCoroutine(Disparo());
    }

    IEnumerator Disparo() 
    {
        RaycastHit hit;
        bool hitInfo = Physics.Raycast(firepoint.position, firepoint.forward, out hit, 50f);

        Instantiate(ShootFx, firepoint.position, Quaternion.identity);

        if (hitInfo)
        {
            line.SetPosition(0, firepoint.position);
            line.SetPosition(1, hit.point);

            Instantiate(Hitfx, hit.point, Quaternion.identity);
        }
        else
        {
            line.SetPosition(0, firepoint.position);
            line.SetPosition(1, firepoint.position + firepoint.forward * 20);

        }
        line.enabled = true;

        yield return new WaitForSeconds(0.02f);

        line.enabled = false;
    }
    void Update()
    {
        
    }
}

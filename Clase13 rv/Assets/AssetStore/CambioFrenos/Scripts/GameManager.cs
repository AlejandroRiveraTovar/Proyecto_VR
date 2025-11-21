using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;



public class GameManager : MonoBehaviour
{
    public GameObject Car;
    public GameObject Wrench;
    private float timeValue;
    public float score;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeElapsedText;
    public GameObject caliperandPads;
    public GameObject frontPad;
    public GameObject backPad;
    public GameManager GM;
    
    //public GameObject[] nuts;


    // Start is called before the first frame update
    void Start()
    {
        Car = GameObject.FindGameObjectWithTag("Car");
        Wrench = GameObject.FindGameObjectWithTag("Wrench");
        score = 0;
        scoreText.text = score.ToString();
    }

    
    void Update()
    {
        //Display Stopwatch Timer
        timeValue += Time.deltaTime;
        double rounded = System.Math.Round(timeValue, 2);
        timeElapsedText.text = rounded.ToString();

        //Show updated score
        
    }

    public void UpdateScore()
    {
        scoreText.text = score.ToString();
        
    }

    public void GrabbedCaliperAndPads()
    {
        GM.score += 1;
        GM.UpdateScore();
        frontPad.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = true;
        backPad.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = true;
        Debug.Log("Both Pads grabbable");
    }

    public void ActivatedCaliperAndPads()
    {
        GM.score += 1;
        GM.UpdateScore();
        frontPad.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = true;
        backPad.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = true;
        Debug.Log("Activated Caliper + Pads, made grabbable");
    }

    public void RemovedFrontPad()
    {
        GM.score += 1;
        GM.UpdateScore();
    }

    public void RemovedBackPad()
    {
        GM.score += 1;
        GM.UpdateScore();
    }

    public void ReplacedFrontPad()
    {
        GM.score += 1;
        GM.UpdateScore();
        frontPad.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = true;
    }

    public void ReplacedBackPad()
    {
        GM.score += 1;
        GM.UpdateScore();
        backPad.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = true;
    }
}

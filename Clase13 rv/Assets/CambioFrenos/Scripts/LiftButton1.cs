//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class LiftButton1 : MonoBehaviour
//{
//    public GameObject platform;
//    public float platformSpeed = .2f;
//    public XRButton upButton;
//    public XRButton downButton;
//    private bool hasLifted = false;
//    public GameManager GM;
    


//    private void Start()
//    {
       
//    }
//    // Update is called once per frame
//    void Update()
//    {
//        if (upButton.isHovered)
//        {
//            platform.transform.Translate(Vector3.up * Time.deltaTime * platformSpeed, Space.World);
//            if (!hasLifted)
//            {
//                GM.score += 5;
//                GM.UpdateScore();
//                hasLifted = true;
//                Debug.Log("LIFTBUTTON says Score: " + GM.score);
//            }
            
//        }

//        if (downButton.isHovered)
//        {
//            platform.transform.Translate(Vector3.down * Time.deltaTime * platformSpeed, Space.World);
//        }
//    }

//    public void ButtonPushed()
//    {
//        Debug.Log("You pushed the button!");
//    }
//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasterEggs : MonoBehaviour
{
    public GameObject spareTire;
    public GameObject tireSpawnPoint2;
    public Vector3 tireSpawnPoint;
    public GameManager GM;

    // Start is called before the first frame update
    void Start()
    {
        tireSpawnPoint = tireSpawnPoint2.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnTire()
    {
        Instantiate(spareTire, tireSpawnPoint, Quaternion.identity);
        GM.score += 1;
        GM.UpdateScore();
    }
}

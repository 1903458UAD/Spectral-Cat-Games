using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawnScript : MonoBehaviour
{
    public GameObject customerPrefab; // Customer prefab
    public Camera playerCam;

    void Update()
    {
        if(playerCam.enabled == true)
        {
            GameManager.Instance.SpawnCustomer();
        }
    }
}

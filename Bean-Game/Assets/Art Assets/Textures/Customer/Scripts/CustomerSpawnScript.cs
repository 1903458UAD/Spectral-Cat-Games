using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawnScript : MonoBehaviour
{
    public GameObject customerPrefab; // Customer prefab

    // Update is called once per frame
    void Update()
    {
        GameManager.Instance.SpawnCustomer();
    }
}

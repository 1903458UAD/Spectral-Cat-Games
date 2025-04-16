using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawnScript : MonoBehaviour
{
    public GameObject customerPrefab; // Customer prefab

    void Update()
    {
        GameManager.Instance.SpawnCustomer();
    }
}

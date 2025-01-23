using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawnScript : MonoBehaviour
{
    public GameObject customerPrefab; // Customer prefab

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Customer") == null) // If there are no customers
        {
            Instantiate(customerPrefab, transform.position, Quaternion.identity); // Instantiate new customer
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeInteraction : MonoBehaviour
{

    private bool inContactWithCustomerWindow = false;
    private CustomerWindow customerWindow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CustomerWindow"))
        {
            inContactWithCustomerWindow = true;
            customerWindow = other.GetComponent<CustomerWindow>();
            Debug.Log("Coffee in contact with window");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CustomerWindow"))
        {
            inContactWithCustomerWindow = false;
            customerWindow = null;
        }
    }


    public void TryAddToCustomerWindow()
    {
        if (inContactWithCustomerWindow && customerWindow != null)
        {
            Debug.Log("Adding Coffee to CustomerWindow!");
            customerWindow.GiveCoffeeToWindow(this);
        }
        else
        {
            Debug.Log("Bean is not near the Customer Window!");
        }
    }
}


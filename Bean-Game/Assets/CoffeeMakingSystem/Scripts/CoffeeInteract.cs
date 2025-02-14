using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeInteraction : MonoBehaviour
{

    private bool inContactWithCustomerWindow = false;
    private CustomerWindow customerWindow;
    public int beanCount;

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
            Debug.Log("Coffee left the window trigger area");
        }
    }


    public bool TryAddToCustomerWindow()
    {
        if (customerWindow != null)
        {
            customerWindow.GiveCoffeeToWindow(this); // Assuming this method exists in CustomerWindow to handle the coffee
            return true; // Successfully delivered the coffee
        }
        return false; // Failed to deliver coffee
    }

    public void SetBeanCount(int count)
    {
        beanCount = count;
    }
}


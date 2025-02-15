using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonForCoffeeMachine : MonoBehaviour
{
    [SerializeField] private GameObject buttonLid;
    public CoffeeMachine coffeeMachine; // Reference to the coffee machine

    public void PressButton()
    {
        Debug.Log("Coffee Machine Button Pressed!");
        buttonLid.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); // Reset button lid
        coffeeMachine.ActivateMachine(); // Start coffee machine
    }
   
}
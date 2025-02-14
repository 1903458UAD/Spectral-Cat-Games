using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonForCoffeeMachine : MonoBehaviour
{
    [SerializeField] private GameObject buttonLid;
    public CoffeeMachine coffeeMachine; // Reference to the coffee machine
    public float interactionDistance = 2.0f; // Distance required for the player to interact with the button
    private Camera playerCamera; // Reference to the player's camera for raycasting
    private KeyCode interaction = KeyCode.Joystick1Button2; // Controller interaction keycode - set to 'Y' button

    private void Start()
    {
        playerCamera = Camera.main; // Get the POV camera
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(interaction)) // Check 'e' or controller key being pressed
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2)); // Fire ray from the center of the screen
            RaycastHit hit;

            // Check if the ray hits something within the interaction distance
            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                if (hit.collider.gameObject == gameObject) // Check if it hits this button
                {
                    Debug.Log("Button pressed");
                    buttonLid.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    coffeeMachine.ActivateMachine();
                }
            }
        }
    }
}
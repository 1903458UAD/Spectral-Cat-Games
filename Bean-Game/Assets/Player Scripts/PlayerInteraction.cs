using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using FMOD.Studio;
using FMODUnity;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private UpgradeData upgradeData;
    [SerializeField] private UpgradeData rangeUpgradeData;

    public float interactionDistance = 2f; //Default interaction distance for interactable objects (Might need some fine tuning for balancing)
    public LayerMask InteractableObjectLayer;
    public LayerMask FunctionalObjectLayer;
    public Transform cameraTransform;

    public InteractableObject heldObjectRight; // Right-hand object
    public InteractableObject heldObjectLeft;  // Left-hand object
    private bool isPickupBothHands; // Enable dual wielding

    private KeyCode Pickup_AND_Interact;// = KeyCode.Joystick1Button5; // Pickup keycode - used for responding to controller input - set to right bumper
    private KeyCode Drop;// = KeyCode.Joystick1Button4; // Pickup keycode - set to left bumper
    //private KeyCode interaction = KeyCode.Joystick1Button2; // Interaction keycode - set to 'Y' button

    private enum InputType { Controller, Keyboard }; // Enum - used to determine whether input is controller or keyboard - likely will move to GameManager in future!
    private InputType currentInput;

    [SerializeField] private EventReference pickupSound;

    [SerializeField] private string parameterName;
    [SerializeField] private float parameterValue;

    private void Start()
    {
        interactionDistance = rangeUpgradeData.internalBaseValue;

        Pickup_AND_Interact = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("InteractKey", "Mouse0")); //Player pref saves over game sessions, It is also a new concept for me, Documentation: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerPrefs.html
        Drop = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("DropKey", "Mouse1"));
    }

    public void UpdateKeybindings()
    {
        
        Pickup_AND_Interact = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("InteractKey", "Mouse0")); 
        Drop = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("DropKey", "Mouse1"));
    }

    private void Update()
    {

        if (UIManager.Instance.IsGamePaused())
        {
            return; //To fix bug where player can interact when paused
        }

        if (cameraTransform == null)
        {
            return;
        }

        if (heldObjectLeft != null && Input.GetKeyDown(Drop)) 
        {

            BeanInteraction bean = heldObjectLeft.GetComponent<BeanInteraction>();
            if (bean != null)
            {
                bean.OnDrop();
                GameManager.Instance.ExclamationOff();
            }

            heldObjectLeft.ReleaseObject(); //Call function to release object being held from left hand
            heldObjectLeft = null;// Clear reference after release
            return;
        }
        else if (heldObjectRight != null && Input.GetKeyDown(Drop))
        {
            BeanInteraction bean = heldObjectRight.GetComponent<BeanInteraction>();
            if (bean != null)
            {
                bean.OnDrop();
                GameManager.Instance.ExclamationOff();
            }

            heldObjectRight.ReleaseObject(); //Call function to release object being held from left hand
            heldObjectRight = null;// Clear reference after release
            return;
        }

        if (heldObjectLeft == null && heldObjectRight == null)
        {
            AudioManager.instance.SetAmbianceParameter("Activate Beat", 0);
        }

        if (Input.GetKeyDown(Pickup_AND_Interact))
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 0.1f);

            RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance, ~0);
            foreach (var h in hits)
            {
            }

            if (Physics.Raycast(ray, out hit, interactionDistance, InteractableObjectLayer))
            {
                GameObject hitObject = hit.collider.gameObject;
                InteractableObject interactable = hitObject.GetComponent<InteractableObject>();

                if (interactable != null)
                {
                    // Pick up object if hand is free
                    if (heldObjectRight == null)
                    {
                        interactable.PickUpObject(true);
                        heldObjectRight = interactable;

                        BeanInteraction bean = interactable.GetComponent<BeanInteraction>();
                        if (bean != null)
                        {
                            bean.OnPickUp();
                            GameManager.Instance.ExclamationOn();
                        }
                    }
                    else if (heldObjectLeft == null && upgradeData.internalUpgradeEnabled) // Allow left-hand pickup if dual-wielding is active
                    {
                        interactable.PickUpObject(false);
                        heldObjectLeft = interactable;
                        
                        BeanInteraction bean = interactable.GetComponent<BeanInteraction>();
                        if (bean != null)
                        {
                            bean.OnPickUp();
                            GameManager.Instance.ExclamationOn();
                        }
                    }

                    if (heldObjectRight != null || heldObjectLeft != null)
                    {
                        AudioManager.instance.PlayOneShot(pickupSound, this.transform.position);
                        AudioManager.instance.SetAmbianceParameter(parameterName, parameterValue);
                    }
                }
            }

            else if (Physics.Raycast(ray, out hit, interactionDistance, FunctionalObjectLayer)) //-- Prioritise function over pick up
            {
                GameObject hitObject = hit.collider.gameObject;
                CoffeeMachine coffeeMachine = hitObject.GetComponent<CoffeeMachine>();
                
                CustomerWindow customerWindow = hitObject.GetComponent<CustomerWindow>();
                Till till = hitObject.GetComponent<Till>();
                ButtonForCoffeeMachine coffeeButton = hitObject.GetComponent<ButtonForCoffeeMachine>();
                PowerCutScript powercut = hitObject.GetComponent<PowerCutScript>();
                Hiding_Spots cage = hitObject.GetComponent<Hiding_Spots>();

                CoffeeInteraction coffee = null;
                   
                if (heldObjectRight != null)
                {
                    coffee = heldObjectRight.GetComponent<CoffeeInteraction>();
                }

                if (coffee == null && heldObjectLeft != null)
                {
                    coffee = heldObjectLeft?.GetComponent<CoffeeInteraction>();
                }

                SyrupBottle syrupBottle = hitObject.GetComponent<SyrupBottle>();

                if (syrupBottle != null)
                {
                    if (hitObject.CompareTag("SyrupPeanutButter"))
                    {
                        syrupBottle.TryAddSyrup(this);
                        return;
                    }
                    else if (hitObject.CompareTag("SyrupVanilla"))
                    {
                        syrupBottle.TryAddSyrup(this);
                        return;
                    }
                    else if (hitObject.CompareTag("SyrupIcedTea"))
                    {
                        syrupBottle.TryAddSyrup(this);
                        return;
                    }
                    else if (hitObject.CompareTag("SyrupCaramel"))
                    {
                        syrupBottle.TryAddSyrup(this);
                        return;
                    }
                }

                if (powercut != null)
                {
                    powercut.fixPower();
                    return;
                }

                if (coffeeButton != null)
                {
                    coffeeButton.PressButton();
                    return;
                }

                if (till != null)
                {
                    UIUpgradeManager.Instance.EnableUpgradeMenu();
                }

                if (coffeeMachine != null)
                {
                    if (heldObjectRight)
                    {
                        heldObjectRight.GetComponent<BeanInteraction>().TryAddToCoffeeMachine(coffeeMachine);
                        return;
                    }
                    else if (heldObjectLeft)
                    {
                        heldObjectLeft.GetComponent<BeanInteraction>().TryAddToCoffeeMachine(coffeeMachine);
                        return;
                    }
                }
                
                if (cage != null)
                {
                    if (heldObjectRight)
                    {
                        heldObjectRight.GetComponent<BeanInteraction>().TryAddToCage(cage);
                        heldObjectRight = null;
                        return;
                    }
                    else if (heldObjectLeft)
                    {
                        heldObjectLeft.GetComponent<BeanInteraction>().TryAddToCage(cage);
                        heldObjectLeft = null;
                        return;
                    }
                }

                if (customerWindow != null)
                {
                    if (heldObjectRight && heldObjectRight.GetComponent<CoffeeInteraction>())
                    {
                        heldObjectRight.GetComponent<CoffeeInteraction>().TryAddToCustomerWindow();
                        return;
                    }
                    else if (heldObjectLeft && heldObjectLeft.GetComponent<CoffeeInteraction>())
                    {
                        heldObjectLeft.GetComponent<CoffeeInteraction>().TryAddToCustomerWindow();
                        return;
                    }

                    return;
                }
            }
            else
            {
            } 
        }
    }
}

 


using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    public float interactionDistance = 2f; //Default interaction distance for interactable objects (Might need some fine tuning for balancing)
    public LayerMask FunctionalObjectLayer;
    public LayerMask InteractableObjectLayer;
    public Transform cameraTransform;

    bool highlighted = false;
    float highlight = 0;
    private Renderer[] rend;
    private GameObject[] gameObjects;
    private GameObject currentObject;

    public string hitObjectName;
  

    void Start()
    {
        gameObjects = GameObject.FindGameObjectsWithTag("Button");

        rend = gameObject.GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 0.1f);

        if (Physics.Raycast(ray, out hit, interactionDistance, FunctionalObjectLayer)) //-- Prioritise function over pick up
        {
            Debug.Log("RayCast Hit a functional Object");
            GameObject hitObject = hit.collider.gameObject;
            ButtonForCoffeeMachine coffeeButton = hitObject.GetComponent<ButtonForCoffeeMachine>();
            CoffeeMachine coffeeMachine = hitObject.GetComponent<CoffeeMachine>();
            CustomerWindow customerWindow = hitObject.GetComponent<CustomerWindow>();
            Till till = hitObject.GetComponent<Till>();
            SyrupBottle syrupbottle = hitObject.GetComponent<SyrupBottle>();

            if (coffeeButton != null)
            {
                gameObjects = GameObject.FindGameObjectsWithTag("Button");
                currentObject = GameObject.FindGameObjectWithTag("ButtonMain");
                rend = currentObject.GetComponentsInChildren<Renderer>();

                SetShaderParameters(gameObjects, rend, 0.0025f);
            }

            if (coffeeMachine != null)
            {
                gameObjects = GameObject.FindGameObjectsWithTag("Hopper");
                currentObject = GameObject.FindGameObjectWithTag("HopperMain");
                rend = currentObject.GetComponentsInChildren<Renderer>();

                SetShaderParameters(gameObjects, rend, 0.0025f);

                var objects = GameObject.FindGameObjectsWithTag("MachineComponent");
                var objectCount = objects.Length;
                foreach (var obj in objects)
                {
                    Renderer r = obj.GetComponent<Renderer>();
                    r.material.SetFloat("_OutlineThickness", 0.0035f);
                }
            }

            if (till != null)
            {
                var objs = GameObject.FindGameObjectsWithTag("Till");
                foreach (var obj in objs)
                {
                    Renderer r = obj.GetComponent<Renderer>();
                    r.material.SetFloat("_OutlineThickness", 0.0025f);
                }
            }

            if (syrupbottle != null)
            {
                string tag = syrupbottle.tag;
                string childrentag;

                switch(tag)
                {
                    case "SyrupPeanutButter":
                        childrentag = "PeanutButter";
                        break;

                    case "SyrupVanilla":
                        childrentag = "Vanilla";
                        break;

                    case "SyrupIcedTea":
                        childrentag = "IcedTea";
                        break;

                    default:
                        childrentag = "Caramel";
                        break;

                }

                gameObjects = GameObject.FindGameObjectsWithTag(childrentag);
                currentObject = GameObject.FindGameObjectWithTag(tag);
                rend = currentObject.GetComponentsInChildren<Renderer>();

                SetShaderParameters(gameObjects, rend, 0.0025f);
            }

            if (customerWindow != null)
            {
                var objs = GameObject.FindGameObjectsWithTag("CarComp");
                foreach (var obj in objs)
                {
                    Renderer r = obj.GetComponent<Renderer>();
                    r.material.SetFloat("_OutlineThickness", 0.0015f);
                }
            }
        }

        else
        {
            gameObjects = GameObject.FindGameObjectsWithTag("Button");
            currentObject = GameObject.FindGameObjectWithTag("ButtonMain");
            rend = currentObject.GetComponentsInChildren<Renderer>();

            SetShaderParameters(gameObjects, rend, 0);

            gameObjects = GameObject.FindGameObjectsWithTag("Hopper");
            currentObject = GameObject.FindGameObjectWithTag("HopperMain");
            rend = currentObject.GetComponentsInChildren<Renderer>();

            SetShaderParameters(gameObjects, rend, 0);

            gameObjects = GameObject.FindGameObjectsWithTag("PeanutButter");
            currentObject = GameObject.FindGameObjectWithTag("SyrupPeanutButter");
            rend = currentObject.GetComponentsInChildren<Renderer>();

            SetShaderParameters(gameObjects, rend, 0);

            gameObjects = GameObject.FindGameObjectsWithTag("Vanilla");
            currentObject = GameObject.FindGameObjectWithTag("SyrupVanilla");
            rend = currentObject.GetComponentsInChildren<Renderer>();

            SetShaderParameters(gameObjects, rend, 0);

            gameObjects = GameObject.FindGameObjectsWithTag("IcedTea");
            currentObject = GameObject.FindGameObjectWithTag("SyrupIcedTea");
            rend = currentObject.GetComponentsInChildren<Renderer>();

            SetShaderParameters(gameObjects, rend, 0);

            gameObjects = GameObject.FindGameObjectsWithTag("Caramel");
            currentObject = GameObject.FindGameObjectWithTag("SyrupCaramel");
            rend = currentObject.GetComponentsInChildren<Renderer>();

            SetShaderParameters(gameObjects, rend, 0);

            var objects = GameObject.FindGameObjectsWithTag("MachineComponent");
                var objectCount = objects.Length;
                foreach (var obj in objects)
                {
                    Renderer r = obj.GetComponent<Renderer>();
                    r.material.SetFloat("_OutlineThickness", 0);
                }

            var objs = GameObject.FindGameObjectsWithTag("CarComp");
            foreach (var obj in objs)
            {
                Renderer r = obj.GetComponent<Renderer>();
                r.material.SetFloat("_OutlineThickness", 0);
            }

            var objss = GameObject.FindGameObjectsWithTag("Till");
            foreach (var obj in objss)
            {
                Renderer r = obj.GetComponent<Renderer>();
                r.material.SetFloat("_OutlineThickness", 0.0f);
            }
        }

        if(Physics.Raycast(ray, out hit, interactionDistance, InteractableObjectLayer))
        {
           Debug.Log("RayCast Hit an Interactable object");
           GameObject hitObject = hit.collider.gameObject;
           InteractableObject coffee = hitObject.GetComponent<InteractableObject>();
           hitObjectName = hitObject.name;

            if (coffee != null)
            {
                Renderer[] children = coffee.GetComponentsInChildren<Renderer>();
              
                for(int i = 0; i < children.Length; i++)
                {
                    children[i].material.SetFloat("_OutlineThickness", 0.0035f);
                }
            }
        }

        else
        {
            var objs = GameObject.FindGameObjectsWithTag("CupComponent");
            foreach (var obj in objs)
            {
                Renderer r = obj.GetComponent<Renderer>();
                r.material.SetFloat("_OutlineThickness", 0);
            }
        }


    }


    private void SetShaderParameters(GameObject[] go, Renderer[] r, float highlight)
    {
        for (int i = 0; i < go.Length; i++)
        {
            r[i].material.SetFloat("_OutlineThickness", highlight);
        }
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    public float interactionDistance = 2f; //Default interaction distance for interactable objects (Might need some fine tuning for balancing)
    public LayerMask FunctionalObjectLayer;
    public Transform cameraTransform;

    bool highlighted = false;
    float highlight = 0;
    private Renderer[] rend;
    private GameObject[] gameObjects;
    private GameObject currentObject;
  

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

            if (coffeeButton != null)
            {
                gameObjects = GameObject.FindGameObjectsWithTag("Button");
                currentObject = GameObject.FindGameObjectWithTag("ButtonMain");
                rend = currentObject.GetComponentsInChildren<Renderer>();
           
                SetShaderParameters(gameObjects, rend, 1);
            }

            if (coffeeMachine != null)
            {
                gameObjects = GameObject.FindGameObjectsWithTag("Hopper");
                currentObject = GameObject.FindGameObjectWithTag("HopperMain");
                rend = currentObject.GetComponentsInChildren<Renderer>();

                SetShaderParameters(gameObjects, rend, 1);

                var objects = GameObject.FindGameObjectsWithTag("MachineComponent");
                var objectCount = objects.Length;
                foreach (var obj in objects)
                {
                    Renderer r = obj.GetComponent<Renderer>();
                    r.material.SetFloat("_HighlightObject", 1);
                }
            }

            if (customerWindow != null)
            {
                var objs = GameObject.FindGameObjectsWithTag("CarComp");
                foreach (var obj in objs)
                {
                    Renderer r = obj.GetComponent<Renderer>();
                    r.material.SetFloat("_HighlightObject", 1);
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

                var objects = GameObject.FindGameObjectsWithTag("MachineComponent");
                var objectCount = objects.Length;
                foreach (var obj in objects)
                {
                    Renderer r = obj.GetComponent<Renderer>();
                    r.material.SetFloat("_HighlightObject", 0);
                }

            var objs = GameObject.FindGameObjectsWithTag("CarComp");
            foreach (var obj in objs)
            {
                Renderer r = obj.GetComponent<Renderer>();
                r.material.SetFloat("_HighlightObject", 0);
            }

        }
    }


    private void SetShaderParameters(GameObject[] go, Renderer[] r, float highlight)
    {
        for (int i = 0; i < go.Length; i++)
        {
            r[i].material.SetFloat("_HighlightObject", highlight);
        }
    }

}
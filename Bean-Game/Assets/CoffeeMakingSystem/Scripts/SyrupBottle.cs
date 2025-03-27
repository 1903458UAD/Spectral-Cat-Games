using TMPro;
using UnityEngine;
using System.Collections;

public class SyrupBottle : MonoBehaviour
{
    public string syrupType; 


    public Transform lidTransform; 
    private Vector3 initialPosition;

    public float pressDepth = 0.05f; 
    public float pressDuration = 0.1f;

    void Start()
    {
        if (lidTransform == null)
        {
            Debug.LogError("Lid Transform is not assigned in the Inspector!");
        }
        else
        {
            lidTransform.gameObject.isStatic = false;
            initialPosition = lidTransform.localPosition;
            Debug.Log($"Initial Lid Position: {initialPosition}");
        }
      
    }

    private IEnumerator MoveLid(Vector3 targetPosition)
    {

        
        if (lidTransform == null)
        {
            Debug.LogError("lidTransform is NULL! Cannot move lid.");
            yield break;
        }
        float elapsedTime = 0f;
       

        Vector3 startPosition = lidTransform.localPosition;

        while (elapsedTime < pressDuration)
        {
            lidTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / pressDuration);
            elapsedTime += Time.deltaTime;

            lidTransform.hasChanged = true;

            Debug.Log($"Lid moving to: {lidTransform.localPosition}");

            yield return null;
        }

        lidTransform.localPosition = targetPosition;


        elapsedTime = 0f;
        startPosition = lidTransform.localPosition;
        while (elapsedTime < pressDuration)
        {
            lidTransform.localPosition = Vector3.Lerp(startPosition, initialPosition, elapsedTime / pressDuration);
            elapsedTime += Time.deltaTime;
            //Debug.Log($"Lid moving back up to: {lidTransform.localPosition}");
            yield return null;
        }

        
        lidTransform.localPosition = initialPosition;
        Debug.Log("Lid moved back to initial position: " + lidTransform.localPosition);
    }

    public void TryAddSyrup(PlayerInteraction player)
    {
        if (player.heldObjectRight != null && player.heldObjectRight.GetComponent<CoffeeInteraction>())
        {
            CoffeeInteraction coffee = player.heldObjectRight.GetComponent<CoffeeInteraction>();
            coffee.AddSyrup(syrupType);
            //Debug.Log($"Added {syrupType} syrup to coffee!");


            StartCoroutine(MoveLid(initialPosition - new Vector3(0f, pressDepth, 0f)));

        }
        else if (player.heldObjectLeft != null && player.heldObjectLeft.GetComponent<CoffeeInteraction>())
        {
            CoffeeInteraction coffee = player.heldObjectLeft.GetComponent<CoffeeInteraction>();
            coffee.AddSyrup(syrupType);
            //Debug.Log($"Added {syrupType} syrup to coffee");


            StartCoroutine(MoveLid(initialPosition - new Vector3(0f, pressDepth, 0f)));

        }
        else
        {
            Debug.Log("You must be holding a coffee to use syrup");
        }
    }
}
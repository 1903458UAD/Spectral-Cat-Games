using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CustomerScript : MonoBehaviour
{
    private GameObject player;
    public GameObject driveThrough;
    public GameObject exit;
    private GameObject nextLocation;

    public TMP_Text timerDisplay;

    private float speed = 0.01f;
    private float patienceTimer;
    private float initialTimer;
    private bool orderDelivered;
    private bool drive;

    private float threshold;
    private float tipFactor;

    private PlayerHealth playerHealth;

    public void SetIsOrderedTrue()
    {
        orderDelivered = true;

    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player?.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            UnityEngine.Debug.LogError("[CustomerScript] PlayerHealth component not found on Player!");
        }

        drive = true;
        orderDelivered = false;
        nextLocation = driveThrough;

        int rand = UnityEngine.Random.Range(0, 3);

        switch (rand)
        {
            case 0:
                threshold = 0.10f;
                patienceTimer = 120;
                initialTimer = 120;
                tipFactor = 0.030f;
                break;
            case 1:
                threshold = 0.25f;
                patienceTimer = 90;
                initialTimer = 90;
                tipFactor = 0.020f;
                break;
            case 2:
                threshold = 0.50f;
                patienceTimer = 60;
                initialTimer = 60;
                tipFactor = 0.015f;
                break;
        }
    }

    void Update()
    {
        if (drive)
        {
            DriveForward();
        }
        else if (!orderDelivered)
        {
            Wait();
        }
        else
        {
            Pay();
        }
    }

    void DriveForward()
    {
        transform.position = Vector3.MoveTowards(transform.position, nextLocation.transform.position, speed);

        if (Vector3.Distance(transform.position, nextLocation.transform.position) < 0.001f)
        {
            if (nextLocation == exit)
            {

                Destroy(gameObject); // Destroy customer //Reverted back to heathers orginal code as the game manager code currently breaks it
                //GameManager.Instance.RemoveCustomer(gameObject); // ✅ Moved customer removal to GameManager
            }

            drive = false;
        }
    }

    void Wait()
    {
        if (patienceTimer > 0)
        {
            patienceTimer -= Time.deltaTime;
            DisplayTime();
        }
        else
        {
            UnityEngine.Debug.Log("[CustomerScript] Customer ran out of patience!");
            playerHealth?.LoseLife();
            nextLocation = exit;
            drive = true;
        }
    }

    void Pay()
    {
        float income = 1.0f;

        if (patienceTimer > (initialTimer * threshold))
        {
            income += patienceTimer * tipFactor;
        }

        nextLocation = exit; // Move customer towards exit //Reverted back to heathers orginal code as the game manager code currently breaks it
        drive = true; // Begin driving //Reverted back to heathers orginal code as the game manager code currently breaks it
        orderDelivered = false; //Reverted back to heathers orginal code as the game manager code currently breaks it

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateIncome(income);
            GameManager.Instance.RemoveCustomer(gameObject);
        }
        else
        {
            UnityEngine.Debug.LogError("[CustomerScript] GameManager instance is null! Cannot update income.");
        }
    }

    void DisplayTime()
    {
        if (patienceTimer > 0)
        {
            float minutes = Mathf.FloorToInt(patienceTimer / 60);
            float seconds = Mathf.FloorToInt(patienceTimer % 60);
            timerDisplay.text = $"{minutes:00}:{seconds:00}";
        }
        else
        {
            timerDisplay.text = null;
        }
    }
}

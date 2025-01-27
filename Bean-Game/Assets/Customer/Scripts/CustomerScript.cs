using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class CustomerScript : MonoBehaviour
{
    // Game Objects
    private GameObject player; // Player
    public GameObject driveThrough; // Stationed where customer will stop and patience timer begins
    public GameObject exit; // Stationed where customer will despawn
    private GameObject nextLocation; // Store which game object the customer is moving towards

    // Text Timer Display
    public TMP_Text timerDisplay;

    public float speed = 1.0f; // Speed of customer
    public float patienceTimer; // Timer - determines how long customer will wait at drive through
    private bool orderDelivered;
    private bool drive; // Determines whether customer is moving or stationary

    public void setIsOrderedTrue()
    {
        orderDelivered = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        drive = true; // Initially set customer to moving
        orderDelivered = false;
        nextLocation = driveThrough; // Set customers destination to drive through
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            orderDelivered = true;
        }

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
        transform.position = Vector3.MoveTowards(transform.position, nextLocation.transform.position, speed); // Move customer towards destination

        // If customer has reached destination
        if (Vector3.Distance(transform.position, nextLocation.transform.position) < 0.001f)
        {
            // If destination is exit
            if(nextLocation == exit)
            {
                Destroy(gameObject); // Destroy customer
            }

            drive = false; // If destination is drive through, stop driving
        }
    }

    void Wait()
    {
        if (patienceTimer > 0)
        {
            patienceTimer -= Time.deltaTime; // Decrease timer by delta time
            DisplayTime(); // Update timer display
        }

        else
        {
            Debug.Log("Customer ran out of patience!");
            nextLocation = exit; // Move customer towards exit
            drive = true; // Begin driving
        }
    }

    void Pay() // Include add to income here
    {
        player.TryGetComponent<IncomeSystem>(out IncomeSystem income);
        income.IncreaseIncome(1);

        Debug.Log("Customer happy");
        Debug.Log(income.GetIncome());
        nextLocation = exit; // Move customer towards exit
        drive = true; // Begin driving

        timerDisplay.text = null;
    }

    void DisplayTime()
    {
        if (patienceTimer > 0)
        {
            float minutes = Mathf.FloorToInt(patienceTimer / 60);
            float seconds = Mathf.FloorToInt(patienceTimer % 60);

            timerDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        else
        {
            timerDisplay.text = null;
        }
    }
}

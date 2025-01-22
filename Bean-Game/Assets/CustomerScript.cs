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
    public GameObject driveThrough;
    public GameObject exit;
    public GameObject nextLocation;
    public float speed = 1.0f;
    public float patienceTimer;
    private bool drive;
    public TMP_Text timerDisplay;

    // Start is called before the first frame update
    void Start()
    {
        drive = true;
        nextLocation = driveThrough;
    }

    // Update is called once per frame
    void Update()
    {
        if (drive)
        {
            DriveForward();
        }

        else
        {
            Wait();
        }
    }

    void DriveForward()
    {
        transform.position = Vector3.MoveTowards(transform.position, nextLocation.transform.position, speed);

        if (Vector3.Distance(transform.position, nextLocation.transform.position) < 0.001f)
        {
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
            Debug.Log("Customer has run out of patience!");
            nextLocation = exit;
            drive = true;
        }
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

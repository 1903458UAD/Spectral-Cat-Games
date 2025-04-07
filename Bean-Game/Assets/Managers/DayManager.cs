using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    public int currentDay = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist this object across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate managers if reloaded
        }
    }

    public void NextDay()
    {
        currentDay++;
        Debug.Log("It is now day: " + currentDay);
    }

    public void ResetDay()
    {
        currentDay = 1;
    }
}

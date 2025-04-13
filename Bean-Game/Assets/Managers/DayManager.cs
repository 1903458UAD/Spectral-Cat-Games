using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    public int currentDay = 1;

    //private DayManager dayManager;
  





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

    private void Start()
    {

        // Trigger UI on start if it's not the first day
        if (currentDay > 1)
        {
            UIManager.Instance?.ShowNextDayPanel(currentDay);
        }
    }




    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
       
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNextDayPanel(currentDay);
        }
    }




    public void NextDay()
    {
        currentDay++;
        Debug.Log("It is now day: " + currentDay);

        // Trigger new day splash from UI
        UIManager.Instance?.ShowNextDayPanel(currentDay);
    }

    public void ResetDay()
    {
        currentDay = 1;
    }
}

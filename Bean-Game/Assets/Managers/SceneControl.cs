using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class SceneControl : MonoBehaviour
{

    [SerializeField] private GameObject PauseMenuUI;
    private bool isPaused = false;

    private void Start()
    {
        GameManager.Instance.SetIncome(StaticData.incomePassed);
        UIManager.Instance.UpdateIncomeDisplay(GameManager.Instance.GetIncome());

        if (PauseMenuUI != null)
        {
            PauseMenuUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            GameManager.Instance.ChangeScene(1);
        }

        //Toggle Pause: (Currently Escape key)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }


    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // Pause the game
            PauseMenuUI.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f; // Resume the game
            PauseMenuUI.SetActive(false);
        }
    }


    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        PauseMenuUI.SetActive(false);
    }

    public void GoToMainMenu()
    {
        //SceneManager.LoadScene(0); 
    }
    public void Settings()
    {
    //Send to another menu
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}


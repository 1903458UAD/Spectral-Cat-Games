﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; //Instance of UIManager

    [Header("UI Elements")] //Headers that show up in inspector
    public GameObject gameOverScreen; //Reference to game over ui
    public Image crosshair; //Refeerence to Crosshair UI
    public TMP_Text incomeText;//Reference to the income UI

    private Color defaultCrosshairColor = Color.black; //Default colouyr of crosshair
    private Color interactableCrosshairColor = Color.red;//Colour of the crosshair when looking at an interactable object

    private void Awake() // When instance is being loaded
    {
        if (Instance == null) //If no instance of the UIManager exists
        {
            Instance = this; //Set this instance as UIManager
        }
        else
        {
            Destroy(gameObject); //Destory duplicates
        }
    }

    private void Start() 
    {
        HideGameOverScreen(); //Set the game over screen is hidden
        SetCrosshairDefault(); //Set the crosshair to default
    }

    public void ShowGameOverScreen() // Show death screen when player dies
    {
        gameOverScreen.SetActive(true); //Activate the game over UI (Which shows it to player)
    }

    public void HideGameOverScreen() //Hide Death screen, To be called when reset
    {
        gameOverScreen.SetActive(false); //De-Activate the game over UI (Which hides it from the player)
    }

    public void SetCrosshairInteractable() //Set the colour of crosshair when not targeting an interactable object
    {
        crosshair.color = interactableCrosshairColor; //change to the crosshair colour (Initally or currently: Red)
    }

    public void SetCrosshairDefault() //Set the colour of the crosshair when not targeting an interactable object
    {
        crosshair.color = defaultCrosshairColor; //change to the crosshair colour (Initally or currently: Black)
    }

    public void UpdateIncomeDisplay(float income)
    {

        if (incomeText != null)
        {
            incomeText.text = $"£{income:F2}";
        }
        else
        {
            UnityEngine.Debug.LogError("[UIManager] Income text UI element is null!");
        }
    }
}

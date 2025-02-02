using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public GameObject deathScreen;
    public Image crosshair;

    private Color defaultCrosshairColor = Color.black;
    private Color interactableCrosshairColor = Color.red;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        HideDeathScreen();
        SetCrosshairDefault();
    }

    // Show death screen when player dies
    public void ShowDeathScreen()
    {
        deathScreen.SetActive(true);
    }

    public void HideDeathScreen()
    {
        deathScreen.SetActive(false);
    }

    // Change crosshair colour
    public void SetCrosshairInteractable()
    {
        crosshair.color = interactableCrosshairColor;
    }

    public void SetCrosshairDefault()
    {
        crosshair.color = defaultCrosshairColor;
    }
}

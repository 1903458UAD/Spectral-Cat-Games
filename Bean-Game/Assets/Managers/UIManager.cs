using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public GameObject gameOverScreen;
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
        HideGameOverScreen();
        SetCrosshairDefault();
    }

    // Show death screen when player dies
    public void ShowGameOverScreen()
    {
        gameOverScreen.SetActive(true);
    }

    public void HideGameOverScreen()
    {
        gameOverScreen.SetActive(false);
    }

    public void SetCrosshairInteractable()
    {
        crosshair.color = interactableCrosshairColor;
    }

    public void SetCrosshairDefault()
    {
        crosshair.color = defaultCrosshairColor;
    }

    public void UpdateIncomeDisplay(float income)
    {
        // ✅ FIXED: Prevents null reference error if `incomeText` is not assigned
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

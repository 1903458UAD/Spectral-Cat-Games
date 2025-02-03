using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public GameObject deathScreen;
    public UnityEngine.UI.Image crosshair;
    public TMP_Text incomeText;

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

    public void ShowDeathScreen()
    {
        // ✅ FIXED: Prevents null reference error if `deathScreen` is not assigned
        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
        }
        else
        {
            UnityEngine.Debug.LogError("[UIManager] Death screen UI element is null!");
        }
    }

    public void HideDeathScreen()
    {
        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }
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

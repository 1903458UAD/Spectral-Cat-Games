using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIUpgradeManager : MonoBehaviour
{
    public static UIUpgradeManager Instance;
    public GameObject upgradeMenu;
    public TMP_Text incomeText;
    public float income;

    [Header("Upgrade Variables")]
    public bool dualWield;
    public float customerPatience;
    public float playerSpeed;

    [Header("Camera Script")]
    public MonoBehaviour cameraScript;

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

    void Start()
    {
        if (upgradeMenu != null)
        {
            upgradeMenu.SetActive(false); // Ensure menu is hidden at the start
        }
    }
    
    public void EnableUpgradeMenu()
    {
        if (!upgradeMenu.activeSelf)
        {
            // Disable camera movement
            if (cameraScript != null)
            {
                cameraScript.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Time.timeScale = 0f;
            UIManager.Instance.HideGameplayUI();
            incomeText.text = string.Format("£{0}", GameManager.Instance.GetIncome());
            upgradeMenu.SetActive(true);

            income = GameManager.Instance.GetIncome();
        }
    }

   public void BackButton()
    {
        if (upgradeMenu.activeSelf)
        {
            GameManager.Instance.SetIncome(income);
            UIManager.Instance.UpdateIncomeDisplay(income);

            // Disable camera movement
            if (cameraScript != null)
            {
                cameraScript.enabled = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Time.timeScale = 1f;
            UIManager.Instance.ShowGameplayUI();
            upgradeMenu.SetActive(false);
        }
    }

    public void DualWieldUpgrade()
    {
        if (income > 1 && StaticData.dualWieldUpgrade == false)
        {
            income -= 1;
            dualWield = true;
            incomeText.text = string.Format("£{0}", income);
        }
        Debug.Log("Click!");
    }

    public void CustomerSpeedUpgrade()
    {
        if (income > 0.5f)
        {
            customerPatience += 0.5f;
            income -= 0.5f;
            incomeText.text = string.Format("£{0}", income);
        }

        Debug.Log("Click!");
    }

    public void PlayerSpeedUpgrade()
    {
        if (income > 0.5f)
        {
            playerSpeed += 0.5f;
            income -= 0.5f;
            incomeText.text = string.Format("£{0}", income);

            StaticData.speedPassed = playerSpeed;
        }

        Debug.Log("Click!");
    }
}

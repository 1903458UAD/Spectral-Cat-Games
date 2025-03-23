using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using FMODUnity;

public class UIUpgradeManager : MonoBehaviour
{
    public static UIUpgradeManager Instance;
    public GameObject upgradeMenu;
    public TMP_Text incomeText;
    public float income;

    public TMP_Text upgradeNameText;
    public TMP_Text upgradeDescriptionText;
    public TMP_Text upgradeCostText;

    [Header("Camera Script")]
    public MonoBehaviour cameraScript;

    private Upgrade selectedUpgrade;

    [SerializeField] public UpgradeData[] upgrades;
    [SerializeField] private EventReference upgradePurchaseFX;
    [SerializeField] private EventReference upgradeFailedFX;
    [SerializeField] private EventReference upgradeSelectFX;

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

    void Start()
    {
        if (upgradeMenu != null)
        {


            upgradeMenu.SetActive(false);


            foreach (UpgradeData upgrade in upgrades)
            {
                upgrade.ResetUpgrade();
                //Debug.Log(upgrade.upgradeEnabled);

            }
        }
    }

    public void OnUpgradeClick(Upgrade upgrade)
    {
        selectedUpgrade = upgrade;
        UpdateDisplay();
        AudioManager.instance.PlayOneShot(upgradeSelectFX, this.transform.position);
    }

    private void UpdateDisplay()
    {
        upgradeNameText.text = selectedUpgrade.GetUpgradeName();
        upgradeDescriptionText.text = selectedUpgrade.GetUpgradeDescription();
        upgradeCostText.text = string.Format("�{0}", selectedUpgrade.GetCost());
    }


    public void EnableUpgradeMenu()
    {
        if (!upgradeMenu.activeSelf)
        {
            if (cameraScript != null)
            {
                cameraScript.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Time.timeScale = 0f;
            UIManager.Instance.HideGameplayUI();
            incomeText.text = string.Format("�{0}", GameManager.Instance.GetIncome());
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

    public void PurchaseUpgrade()
    {
        if (selectedUpgrade != null)
        {
            if (selectedUpgrade.GetCost() <= income)
            {
               income = selectedUpgrade.ApplyUpgrade(income);
               incomeText.text = string.Format("�{0}", income);
               AudioManager.instance.PlayOneShot(upgradePurchaseFX, this.transform.position);
            }
            else
            {
               AudioManager.instance.PlayOneShot(upgradeFailedFX, this.transform.position);
            }
        }
    }
}

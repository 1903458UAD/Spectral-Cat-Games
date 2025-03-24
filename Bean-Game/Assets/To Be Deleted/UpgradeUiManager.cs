using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SocialPlatforms;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class UpgradeUiManager : MonoBehaviour
{
    public TMP_Text incomeText;

    private void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        incomeText.text = string.Format("£{0}", StaticData.incomePassed);

        if (Input.GetKeyDown(KeyCode.M))
        {
            // Switch back to game scene
            GameManager.Instance.ChangeScene(1);
            Debug.Log("Scene Switch!");
        }
    }

    public void DualWieldUpgrade()
    {
        if (!StaticData.dualWieldUpgrade && StaticData.incomePassed > 1)
        {
            StaticData.incomePassed -= 1;
            StaticData.dualWieldUpgrade = true;
        }
        Debug.Log("Click!");
    }

    public void BlendTimeUpgrade()
    {
        StaticData.incomePassed -= 1;
        Debug.Log("Click!");
    }

    public void CustomerSpeedUpgrade()
    {
        if (StaticData.incomePassed > 0.5)
        {
            StaticData.customerPatience += 0.5f;
            StaticData.incomePassed -= 0.5f;
        }

        Debug.Log("Click!");
    }

    public void PlayerSpeedUpgrade()
    {
        if (StaticData.incomePassed > 0.5)
        { 
            StaticData.speedPassed += 0.5f;
            StaticData.incomePassed -= 0.5f;
        }

        Debug.Log("Click!");
    }

    public void TipUp()
    {
        if (StaticData.incomePassed > 0.5)
        {
            StaticData.tipAmount += 0.5f;
            StaticData.incomePassed -= 0.5f;
        }

        Debug.Log("Click!");
    }

    public void longerArms()
    {
        if (StaticData.incomePassed > 0.5)
        {
            StaticData.longArm += 0.5f;
            StaticData.incomePassed -= 0.5f;
        }

        Debug.Log("Click!");
    }

    public void BeanSpotted()
    {
        if (StaticData.incomePassed > 0.5)
        {
            StaticData.alert = true;
            StaticData.alertSize *= 0.1f; 
            StaticData.incomePassed -= 0.5f;
        }

        Debug.Log("Click!");
    }


    public void TrapPurchased()
    {
        if (StaticData.incomePassed > 0.5)
        {
            StaticData.trapPurchased += 1.0f;
            StaticData.trapCheck = true;
            StaticData.incomePassed -= 0.5f;
        }

        Debug.Log("Click!");
    }

    public void cagePurchased()
    {
        if (StaticData.incomePassed > 0.5)
        {
            StaticData.cagePurchased += 1;
            StaticData.incomePassed -= 0.5f;
        }

        Debug.Log("Click!");
    }

    public void InflateBean()
    {
        if (StaticData.incomePassed > 0.5)
        {
            StaticData.inflateBean = true;
            StaticData.incomePassed -= 0.5f;
        }

        Debug.Log("Click!");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SocialPlatforms;

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
            GameManager.Instance.ChangeScene(2);
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
}

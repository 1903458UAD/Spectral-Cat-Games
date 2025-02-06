using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SceneControl : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.SetIncome(StaticData.incomePassed);
        UIManager.Instance.UpdateIncomeDisplay(GameManager.Instance.GetIncome());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            GameManager.Instance.ChangeScene(1);
        }
    }
}


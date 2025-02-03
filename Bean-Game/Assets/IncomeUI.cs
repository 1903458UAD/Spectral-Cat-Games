using UnityEngine;
using TMPro;

public class IncomeUI : MonoBehaviour
{
    public TMP_Text incomeDisplay;

    void Update()
    {
        if (GameManager.Instance != null)
        {
            incomeDisplay.text = $"£{GameManager.Instance.GetIncome():F2}";
        }
    }
}

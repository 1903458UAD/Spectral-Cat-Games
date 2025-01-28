using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncomeSystem : MonoBehaviour
{
    private float income;

    // Start is called before the first frame update
    void Start()
    {
        income = 0;
    }

    public void IncreaseIncome(float amount)
    {
        income += amount;
        income = Mathf.Round(income * 100.0f) * 0.01f; // Round income every update
    }

    public void DecreaseIncome(float amount)
    {
        income -= amount;
        income = Mathf.Round(income * 100.0f) * 0.01f; // Round income every update
    }

    public float GetIncome()
    {
        return income;
    }
}

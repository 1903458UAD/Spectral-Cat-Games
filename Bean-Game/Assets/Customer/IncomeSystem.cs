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
    }

    public void DecreaseIncome(float amount)
    {
        income -= amount;
    }

    public float GetIncome()
    {
        return income;
    }

}

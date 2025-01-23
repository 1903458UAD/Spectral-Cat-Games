using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IncomeUI : MonoBehaviour
{
    private GameObject player; // Player
    public TMP_Text incomeDisplay;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        player.TryGetComponent<IncomeSystem>(out IncomeSystem income);

        incomeDisplay.text = string.Format("£{0}", income.GetIncome());
    }
}

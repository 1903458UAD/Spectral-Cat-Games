using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class EODScript : MonoBehaviour
{
    //Daily values
    public TMP_Text dayCount;
    public TMP_Text dailyIncome;
    public TMP_Text dailyTips;
    public TMP_Text dailyBeans;

    //Total values
    public TMP_Text totalIncome;
    public TMP_Text totalTips;
    public TMP_Text totalBeans;

    public string[] beanKilled;
    private string beanString;

    private void Start()
    {
        beanString = beanKilled[Random.Range(0,beanKilled.Length)];
    }

    private void Update()
    {
        dayCount.text = $"Day: {DayManager.Instance.currentDay}";
        dailyIncome.text = $"Today's Income: {StaticData.dailyIncome}";
        dailyTips.text = $"Today's Tips: {StaticData.dailyTips}";
        dailyBeans.text = $"Beans {beanString}: {StaticData.dailyBeans}";

        totalIncome.text = $"Total Income: {StaticData.totalIncome}";
        totalTips.text = $"Today's Tips: {StaticData.totalTips}";
        totalBeans.text = $"Beans {beanString}: {StaticData.totalBeans}";
    }
}

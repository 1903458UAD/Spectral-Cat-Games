using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StaticData : MonoBehaviour
{
    public static float incomePassed = 0;
    public static float speedPassed = 1;
    public static float tipAmount = 50;
    public static float customerPatience = 1;
    public static float longArm = 2;
    public static float alertSize = 1;
    public static float cagePurchased = 0;
    public static float trapPurchased = 0;

    public static int lowerQuotaLimit = 3;
    public static int higherQuotaLimit = 5;

    public static float dailyIncome;
    public static float dailyTips;
    public static int dailyBeans;

    public static float totalIncome;
    public static float totalTips;
    public static int totalBeans;

    public static bool dualWieldUpgrade = false;
    public static bool alert = false;
    public static bool trapCheck = false;

    public static bool inflateBean = false;

    public static bool skipIntro = false;
}

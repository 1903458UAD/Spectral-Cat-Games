using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade : MonoBehaviour
{
    [SerializeField] private UpgradeData upgradeData;

    public float ApplyUpgrade(float income)
    {
        upgradeData.EnableUpgrade();
        upgradeData.IncreaseStat();
        return upgradeData.PayForUpgrade(income);
    }

    public string GetUpgradeName() { return upgradeData.upgradeName; }
    public string GetUpgradeDescription() {  return upgradeData.upgradeDescription; }
    public int GetCost() { return upgradeData.cost; }
    public int GetTier() { return upgradeData.internalBaseTier;}

}

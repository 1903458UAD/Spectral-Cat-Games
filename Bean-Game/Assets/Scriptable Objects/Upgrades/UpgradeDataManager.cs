using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeDataManager : MonoBehaviour
{
    [SerializeField] private UpgradeData[] upgrades;

    public void ResetUpgrades()
    {
        foreach (UpgradeData upgrade in upgrades)
        {
            upgrade.ResetUpgrade();
            Debug.Log(upgrade.upgradeEnabled);
           
        }

        Debug.Log("Upgrades reset for new game!");
    }
}

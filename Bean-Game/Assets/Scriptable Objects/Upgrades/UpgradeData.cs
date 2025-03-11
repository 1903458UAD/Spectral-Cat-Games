using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Upgrades/UpgradeData")]
public class UpgradeData : ScriptableObject
{
     public string upgradeName;
     public string technicalName;
     [TextArea]public string upgradeDescription;
     public int cost;

     public float baseValue; // Base value for stat upgrades
     public float increaseValue; // Values to increase stat upgrades by

     public int tier; // Base tier value for tiered upgrades
     public int maxTier; // Maximum tier for tiered upgrades

     public bool upgradeEnabled; // Bool for upgrades that are either enabled or disabled

    public float internalBaseValue;
    public int internalBaseTier;
    public bool internalUpgradeEnabled;


    public void ResetUpgrade()
    {
        internalBaseValue = baseValue;
        internalBaseTier = tier;
        internalUpgradeEnabled = upgradeEnabled;

    }

    public void IncreaseStat()
    {
        if (internalBaseTier < maxTier)
        {
            internalBaseValue += increaseValue;
            internalBaseTier++;
        }
    }

    public float PayForUpgrade(float income)
    {
        if (internalBaseTier < maxTier)
        {
            income -= cost;
        }

        return income;
    }

    public void EnableUpgrade()
    {
        if (!internalUpgradeEnabled)
        {
            internalUpgradeEnabled = true;
        }
    }
}

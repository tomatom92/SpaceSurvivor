using UnityEngine;

public abstract class Upgrade : ScriptableObject
{
    public string upgradeName;
    public Sprite icon;
    public UpgradeBehaviour.UpgradeType upgradeType;
    public string description;    
}
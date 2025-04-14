using UnityEngine;

[CreateAssetMenu(fileName = "MaxHealthUpgrade", menuName = "Upgrades/MaxHealthUpgrade")]
public class MaxHealthUpgrade : Upgrade, IUpgradeEffect
{
    public float additionalHealth = 20f;

    public void ApplyEffect(PlayerHealth playerHealth)
    {
        playerHealth.IncreaseMaxHealth(additionalHealth);
    }

    public void ApplyEffect(PlayerShoot playerShoot)
    {
        // No effect on PlayerShoot
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "RapidFireUpgrade", menuName = "Upgrades/RapidFireUpgrade")]
public class RapidFireUpgrade : Upgrade, IUpgradeEffect
{
    public float newAttackDelay = 0.1f;

    public void ApplyEffect(PlayerShoot playerShoot)
    {
        playerShoot.attackDelay = newAttackDelay;
    }

    public void ApplyEffect(PlayerHealth playerHealth)
    {
        //throw new System.NotImplementedException();
    }
}

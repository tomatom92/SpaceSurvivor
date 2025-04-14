using UnityEngine;

[CreateAssetMenu(fileName = "SpreadShotUpgrade", menuName = "Upgrades/SpreadShotUpgrade")]
public class SpreadShotUpgrade : Upgrade, IUpgradeEffect
{
    public int numberOfProjectiles = 3;
    public float spreadAngle = 15f;

    public void ApplyEffect(PlayerShoot playerShoot)
    {
        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float angle = (i - (numberOfProjectiles - 1) / 2f) * spreadAngle;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
            playerShoot.ShootBullet(direction);
        }
    }

    public void ApplyEffect(PlayerHealth playerHealth)
    {
        //throw new System.NotImplementedException();
    }
}

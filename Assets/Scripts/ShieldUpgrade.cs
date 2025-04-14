using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "ShieldUpgrade", menuName = "Upgrades/ShieldUpgrade")]
public class ShieldUpgrade : Upgrade, IUpgradeEffect
{
    public float shieldRechargeTime = 5f;
    private bool shieldActive = false;
    private bool isShieldRecharging = false;
    private static bool shieldHit;
    public GameObject shieldVisual;

    public void ApplyEffect(PlayerHealth playerHealth)
    {
        shieldActive = true;
        playerHealth.StartCoroutine(HandleShield(playerHealth));
    }

    public void ApplyEffect(PlayerShoot playerShoot)
    {
        // No effect on PlayerShoot
    }

    public static bool GetShieldHit()
    {
        return shieldHit;
    }

    public static void DeactivateShield()
    {
        shieldHit = true;
    }

    private IEnumerator HandleShield(PlayerHealth playerHealth)
    {
        while (true)
        {
            if (shieldActive)
            {

                GameObject shield = Instantiate(shieldVisual, playerHealth.transform.position, Quaternion.identity); //spawn shield visual on player
                shieldVisual.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f); //reset shield color

                
                yield return new WaitUntil(() => shieldHit); // Wait for a shot to be blocked
                shieldActive = false;
                isShieldRecharging = true;

                shieldVisual.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f); //transparent shield while recharging

                yield return new WaitForSeconds(shieldRechargeTime);
                shieldActive = true;
                isShieldRecharging = false;
            }
            yield return null;
        }
    }
}

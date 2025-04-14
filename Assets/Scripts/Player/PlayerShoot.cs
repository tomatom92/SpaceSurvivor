using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private PlayerInputSubscription GetInput;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private PlayerMovement playerMovement; // Reference to the PlayerMovement script

    [SerializeField] private float bulletSpeed = 20f;
    public float attackDelay = 0.4f; // Made public to allow modification by upgrades
    private float attackCooldown = 0;

    [SerializeField] private List<IUpgradeEffect> activeUpgradeEffects = new List<IUpgradeEffect>(); // List of active upgrade effects

    public static PlayerShoot instance;
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // Decrease the cooldown timer
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }

        // Check if we can fire
        if (GetInput.FireInput && attackCooldown <= 0)
        {
            // Reset cooldown timer
            attackCooldown = attackDelay;
            ShootBullet(Vector2.up);

            // Apply all active upgrade effects
            foreach (var upgradeEffect in activeUpgradeEffects)
            {
                upgradeEffect.ApplyEffect(this);
                Debug.Log($"Upgrade effect applied: {upgradeEffect}");
            }
        }
    }

    public void ShootBullet(Vector2 direction)
    {
        // Create the projectile
        GameObject projectile = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // Determine the bullet direction based on player movement
        if (playerMovement != null)
        {
            if (playerMovement.IsMovingLeft())
            {
                direction += Vector2.left * 0.2f; // Adjust the value as needed
            }
            else if (playerMovement.IsMovingRight())
            {
                direction += Vector2.right * 0.2f; // Adjust the value as needed
            }
        }

        // Normalize the direction and apply force
        direction.Normalize();
        projectile.GetComponent<Rigidbody2D>().AddForce(direction * bulletSpeed, ForceMode2D.Impulse);

        // Destroy the projectile after 2 seconds
        Destroy(projectile, 2f);
    }

    public void AddUpgradeEffect(IUpgradeEffect upgradeEffect)
    {
        if (!activeUpgradeEffects.Contains(upgradeEffect))
        {
            activeUpgradeEffects.Add(upgradeEffect);
        }
    }

    public void RemoveUpgradeEffect(IUpgradeEffect upgradeEffect)
    {
        if (activeUpgradeEffects.Contains(upgradeEffect))
        {
            activeUpgradeEffects.Remove(upgradeEffect);
        }
    }
}

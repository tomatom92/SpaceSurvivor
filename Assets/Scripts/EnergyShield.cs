using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyShield : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("EnemyBullet"))
        {
            ShieldUpgrade.DeactivateShield();
            Debug.Log("Bullet destroyed by shield");
        }
    }
    private void Update()
    {
        transform.position = PlayerMovement.instance.transform.position;
    }
}

using StarterAssets;
using UnityEngine;

public class SpikedWoodenClub : MonoBehaviour
{
    public int damage = 100;
    public Enemy enemyScript;
    public float hitTimer = 0;
    public float timeBwtHit = 1f;
    public bool canHit = false;

    void FixedUpdate()
    {
        CheckIfCanHit();
    }

    void CheckIfCanHit()
    {
        if (!canHit)
        {
            hitTimer += Time.deltaTime;

            if (hitTimer >= timeBwtHit)
            {
                canHit = true;
                hitTimer = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canHit && enemyScript.isAttacking)
        {
            ThirdPersonController player = other.GetComponent<ThirdPersonController>();
            player.TakeDamage(damage);
            canHit = false;
        }
    }
}

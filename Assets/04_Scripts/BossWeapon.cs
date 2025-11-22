using StarterAssets;
using UnityEngine;

public class BossWeapon : MonoBehaviour
{
    public int damage = 100;
    public Boss bossScript;
    public float hitTimer = 0;
    public float timeBwtHit = 1f;
    public bool canHit = false;

    private void Start()
    {
        bossScript = GameObject.FindGameObjectWithTag("Boss").GetComponent<Boss>();
    }

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
        if (other.CompareTag("Player") && canHit && bossScript.isAttacking)
        {
            ThirdPersonController playerController = other.GetComponent<ThirdPersonController>();
            if (playerController == null)
            {
                playerController = other.GetComponentInChildren<ThirdPersonController>();
            }
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }
            canHit = false;
        }
    }
}

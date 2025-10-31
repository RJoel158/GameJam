using StarterAssets;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] ThirdPersonController thirdPersonController;
    public int damage = 10;
    public float hitTimer = 0;
    public float timeBwtHit = 1f;
    public bool canHit = false;

    void Update()
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
        if (other.CompareTag("Enemy") && canHit && !thirdPersonController.dead && thirdPersonController.isAttacking)
        {
            Enemy enemy = other.GetComponentInChildren<Enemy>();
            enemy.TakeDamage(damage);
            canHit = false;
        }
    }
}

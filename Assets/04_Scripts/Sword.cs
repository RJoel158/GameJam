using StarterAssets;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] ThirdPersonController thirdPersonController;
    public int damage = 10;
    public float hitTimer = 0;
    public float timeBwtHit = 1f;
    public bool canHit = false;

    private BoxCollider swordCollider;
    private bool wasAttacking = false;

    void Awake()
    {
        // Log this GameObject's info
        Debug.Log($"[Sword] Script attached to: {gameObject.name}, full path: {GetTransformPath(transform)}");

        // Get ALL components on this GameObject
        Component[] allComponents = GetComponents<Component>();
        Debug.Log($"[Sword] Total components on this GameObject: {allComponents.Length}");
        foreach (var comp in allComponents)
        {
            Debug.Log($"  - Component: {comp.GetType().Name}");
        }

        // Now try to find collider
        Collider[] colliders = GetComponents<Collider>();
        Collider foundCollider = null;

        Debug.Log($"[Sword] Found {colliders.Length} collider(s) on this GameObject");

        if (colliders.Length > 0)
        {
            foundCollider = colliders[0];
        }
        else
        {
            // Try to find collider in children
            foundCollider = GetComponentInChildren<Collider>();
            if (foundCollider != null)
            {
                Debug.Log($"[Sword] Found collider in child: {foundCollider.gameObject.name}");
            }
        }

        if (foundCollider != null)
        {
            Debug.Log($"<color=cyan>[Sword] ✓ Collider configured: type={foundCollider.GetType().Name}, isTrigger={foundCollider.isTrigger}, layer={LayerMask.LayerToName(foundCollider.gameObject.layer)}, path={GetTransformPath(foundCollider.transform)}</color>");
            if (!foundCollider.isTrigger)
            {
                Debug.LogWarning($"<color=orange>[Sword] Collider found but Is Trigger = false! Mark it as trigger in Inspector for proper detection.</color>");
            }

            // Store reference to sword collider for manual overlap detection
            swordCollider = foundCollider as BoxCollider;

            // Force sword to Default layer (layer 0) so it can collide with Enemy/Wizard layers
            // Player layer often has collision matrix restrictions that prevent enemy detection
            if (gameObject.layer != 0)
            {
                gameObject.layer = 0; // Default layer
                Debug.Log($"<color=green>[Sword] ✓ Changed layer to 'Default' (0) for proper enemy collision detection</color>");
            }
            else
            {
                Debug.Log($"<color=green>[Sword] ✓ Already on 'Default' layer for collision detection</color>");
            }
        }
        else
        {
            Debug.LogWarning("[Sword] NO COLLIDER FOUND! Sword (or its children) should have a Collider component with Is Trigger = true for combat to work properly.");
        }
    }

    private string GetTransformPath(Transform t)
    {
        if (t == null) return "<null>";
        string path = t.name;
        var cur = t.parent;
        while (cur != null)
        {
            path = cur.name + "/" + path;
            cur = cur.parent;
        }
        return path;
    }

    void Update()
    {
        CheckIfCanHit();

        // Manual overlap detection when attacking (fixes collision matrix issues)
        if (thirdPersonController != null && !thirdPersonController.dead)
        {
            // Detect attack start (transition from not attacking to attacking)
            if (thirdPersonController.isAttacking && !wasAttacking && canHit)
            {
                Debug.Log("<color=cyan>[Sword] Attack started - performing manual overlap check</color>");
                PerformManualHitDetection();
            }
            wasAttacking = thirdPersonController.isAttacking;
        }
    }

    void PerformManualHitDetection()
    {
        if (swordCollider == null)
        {
            Debug.LogWarning("[Sword] No BoxCollider found for manual detection");
            return;
        }

        // Get collider bounds in world space
        Vector3 center = swordCollider.bounds.center;
        Vector3 halfExtents = swordCollider.bounds.extents;

        // Check all layers that might have enemies: Enemy, Wizard, Boss
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int wizardLayer = LayerMask.NameToLayer("Wizard");
        int bossLayer = LayerMask.NameToLayer("Boss");

        int layerMask = 0;
        if (enemyLayer != -1) layerMask |= (1 << enemyLayer);
        if (wizardLayer != -1) layerMask |= (1 << wizardLayer);
        if (bossLayer != -1) layerMask |= (1 << bossLayer);

        Debug.Log($"<color=cyan>[Sword] OverlapBox at {center}, size={halfExtents * 2}, layerMask={layerMask}</color>");

        // Perform overlap check
        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation, layerMask, QueryTriggerInteraction.Collide);

        Debug.Log($"<color=yellow>[Sword] Manual detection found {hits.Length} potential targets</color>");

        foreach (var hit in hits)
        {
            Debug.Log($"<color=green>[Sword] Hit detected: {hit.gameObject.name}, tag={hit.tag}, layer={LayerMask.LayerToName(hit.gameObject.layer)}</color>");

            // Apply damage
            ApplyDamage(hit);
        }
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
        // Log ALL collisions for debugging with detailed info
        Debug.Log($"<color=magenta>[Sword] OnTriggerEnter: {other.gameObject.name}, tag={other.tag}, layer={LayerMask.LayerToName(other.gameObject.layer)}, canHit={canHit}, isAttacking={thirdPersonController.isAttacking}, dead={thirdPersonController.dead}</color>");

        // Verificar si puede atacar
        if (!canHit || thirdPersonController.dead || !thirdPersonController.isAttacking)
        {
            Debug.Log($"<color=yellow>[Sword] Skipping collision: canHit={canHit}, isAttacking={thirdPersonController.isAttacking}, dead={thirdPersonController.dead}</color>");
            return;
        }

        // Ignore player collision
        if (other.CompareTag("Player"))
            return;

        // Apply damage to detected target
        ApplyDamage(other);
    }

    private void ApplyDamage(Collider other)
    {
        // Check by layer name as fallback (in case tag is not set correctly)
        string layerName = LayerMask.LayerToName(other.gameObject.layer);

        Debug.Log($"<color=lime>[Sword] ApplyDamage called on: {other.gameObject.name}, tag={other.tag}, layer={layerName}</color>");

        bool damageApplied = false;

        // Atacar enemigos normales
        if (other.CompareTag("Enemy") || layerName == "Enemy")
        {
            Enemy enemy = other.GetComponentInChildren<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                damageApplied = true;
                Debug.Log($"<color=yellow>[Sword] ✓ Golpeaste a un enemigo por {damage} de daño!</color>");
            }
            else
            {
                // Try SendMessage as fallback
                other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                if (other.transform.parent != null)
                {
                    other.transform.parent.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                }
                damageApplied = true;
                Debug.Log($"<color=yellow>[Sword] ✓ Sent TakeDamage to Enemy via SendMessage</color>");
            }
        }

        // Atacar al BOSS
        if (other.CompareTag("Boss") || layerName == "Boss")
        {
            Debug.Log($"[Sword] Boss detected: {other.gameObject.name}");
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            if (other.transform.parent != null)
            {
                other.transform.parent.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
            damageApplied = true;
            Debug.Log($"<color=red>[Sword] ✓ GOLPEASTE AL BOSS por {damage} de daño!</color>");
        }

        // Atacar Wizards
        if (other.CompareTag("Wizard") || layerName == "Wizard")
        {
            Debug.Log($"[Sword] Wizard detected: {other.gameObject.name}");
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            if (other.transform.parent != null)
            {
                other.transform.parent.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
            damageApplied = true;
            Debug.Log($"<color=yellow>[Sword] ✓ ¡Golpeaste a un Wizard por {damage} de daño!</color>");
        }

        if (damageApplied)
        {
            canHit = false;
        }
    }
}

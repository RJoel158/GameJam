using System.Collections;
using UnityEngine;

// Simple meteor projectile that falls toward a target position and spawns an AoE on impact.
public class MeteorProjectile : MonoBehaviour
{
    public float fallSpeed = 12f;
    public Vector3 targetPosition;
    public GameObject impactPrefab;
    public float impactPrefabScale = 1f;
    public float impactRadius = 2.5f;
    public float impactDPS = 5f;
    public float impactDuration = 3f;
    public AudioClip impactSfx;
    public float impactSfxVolume = 1f;
    public int impactSfxPriority = 0; // lower = higher priority
    // Toggle verbose debug logging for meteor AoE
    public bool debugMeteorAoE = false;

    private bool hasImpacted = false;

    private void Update()
    {
        if (hasImpacted) return;
        // Move toward targetPosition, primarily downwards but allow slight homing in XZ
        Vector3 dir = (new Vector3(targetPosition.x, transform.position.y, targetPosition.z) - transform.position);
        // move mainly downwards with horizontal correction
        Vector3 horizontal = new Vector3(dir.x, 0f, dir.z);
        Vector3 move = (Vector3.down * fallSpeed * Time.deltaTime) + (horizontal * 0.5f * Time.deltaTime);
        transform.position += move;

        // check for ground/close to target Y
        if (transform.position.y <= targetPosition.y + 0.5f)
        {
            Impact();
        }
    }

    private void Impact()
    {
        hasImpacted = true;
        try
        {
            // determine impact position and spawn impact prefab
            Vector3 impactPos = transform.position;
            if (impactPrefab != null)
            {
                var go = Instantiate(impactPrefab, impactPos, Quaternion.identity);
                try { go.transform.localScale *= impactPrefabScale; } catch { }
                Destroy(go, impactDuration + 0.25f);
            }

            // play impact SFX with a temporary AudioSource at the impact position using high priority so it won't be interrupted
            try
            {
                if (impactSfx != null)
                {
                    var sgo = new GameObject("_MeteorImpactSfx");
                    sgo.transform.position = transform.position;
                    var src = sgo.AddComponent<AudioSource>();
                    src.spatialBlend = 1f; // 3D
                    src.clip = impactSfx;
                    src.priority = Mathf.Clamp(impactSfxPriority, 0, 256);
                    src.volume = Mathf.Clamp01(impactSfxVolume);
                    src.playOnAwake = false;
                    src.minDistance = 0.1f;
                    src.maxDistance = Mathf.Max(10f, src.maxDistance);
                    src.rolloffMode = AudioRolloffMode.Linear;
                    try { src.Play(); } catch { src.PlayOneShot(impactSfx, src.volume); }
                    Destroy(sgo, impactSfx.length + 0.2f);
                }
            }
            catch { }

            // damage over time in area (use captured impactPos)
            StartCoroutine(DoAoE(impactPos));

            // Strong fallback: apply an immediate small burst of damage to the player
            // so the impact is not completely harmless if the AoE coroutine misses the player.
            try
            {
                var playerGo = GameObject.FindGameObjectWithTag("Player");
                if (playerGo != null)
                {
                    float pdist3D = Vector3.Distance(playerGo.transform.position, impactPos);
                    float pdistXZ = Vector2.Distance(new Vector2(playerGo.transform.position.x, playerGo.transform.position.z), new Vector2(impactPos.x, impactPos.z));
                    if (pdistXZ <= impactRadius || pdist3D <= impactRadius)
                    {
                        int burst = Mathf.Max(1, Mathf.RoundToInt(impactDPS * 0.5f));
                        var ptpc = playerGo.GetComponent<StarterAssets.ThirdPersonController>();
                        if (ptpc != null)
                        {
                            try { ptpc.TakeDamage(burst); Debug.Log($"[MeteorProjectile] Impact immediate burst {burst} applied to Player (TPC). pdistXZ={pdistXZ:F2}"); } catch { }
                        }
                        else
                        {
                            try { playerGo.SendMessage("TakeDamage", burst, SendMessageOptions.DontRequireReceiver); Debug.Log($"[MeteorProjectile] Impact immediate burst {burst} SentMessage to Player. pdistXZ={pdistXZ:F2}"); } catch { }
                        }
                    }
                }
            }
            catch { }
        }
        catch { }
        finally
        {
            // destroy meteor visual
            Destroy(gameObject);
        }
    }

    private IEnumerator DoAoE(Vector3 center)
    {
        float t = 0f;
        float accumulator = 0f;
        while (t < impactDuration)
        {
            float dt = Time.deltaTime;
            // accumulate damage over time to avoid zero-rounding on small DPS
            accumulator += impactDPS * dt;

            Collider[] hits = Physics.OverlapSphere(center, impactRadius);
            if (debugMeteorAoE) Debug.Log($"[MeteorProjectile] AoE tick t={t:F2} center={center} radius={impactRadius} hits={hits.Length} accumulator={accumulator:F2}");

            // Determine integer damage to apply this tick (shared among targets)
            int dmgThisTick = Mathf.FloorToInt(accumulator);
            // Track unique GameObjects we've damaged this tick to avoid double application
            var damagedThisTick = new System.Collections.Generic.HashSet<GameObject>();

            foreach (var c in hits)
            {
                if (c == null) continue;

                GameObject root = c.gameObject;
                try { root = c.transform.root.gameObject; } catch { }

                if (debugMeteorAoE)
                {
                    string compNames = "";
                    try
                    {
                        var comps = c.gameObject.GetComponents<Component>();
                        for (int i = 0; i < comps.Length && i < 6; ++i) { if (comps[i] != null) compNames += comps[i].GetType().Name + ","; }
                    }
                    catch { }
                    Debug.Log($"[MeteorProjectile] Overlap hit: name={c.gameObject.name}, root={root.name}, tag={c.gameObject.tag}, comps={compNames}");
                }

                // Prefer StarterAssets controller or the Player tag
                var playerTpc = c.GetComponent<StarterAssets.ThirdPersonController>() ?? c.GetComponentInParent<StarterAssets.ThirdPersonController>();
                if (playerTpc != null)
                {
                    if (dmgThisTick > 0 && !damagedThisTick.Contains(root))
                    {
                        try { playerTpc.TakeDamage(dmgThisTick); Debug.Log($"[MeteorProjectile] Applied {dmgThisTick} damage to {root.name} via TPC."); } catch { Debug.LogWarning("[MeteorProjectile] Failed to call TakeDamage on TPC."); }
                        damagedThisTick.Add(root);
                    }
                    continue;
                }

                // Fallback: send damage via SendMessage only if we have integer damage and haven't damaged this root yet
                if (dmgThisTick > 0 && !damagedThisTick.Contains(root))
                {
                    try
                    {
                        root.SendMessage("TakeDamage", dmgThisTick, SendMessageOptions.DontRequireReceiver);
                        if (debugMeteorAoE) Debug.Log($"[MeteorProjectile] SentMessage TakeDamage={dmgThisTick} to {root.name}");
                    }
                    catch { if (debugMeteorAoE) Debug.LogWarning($"[MeteorProjectile] SendMessage failed on {root.name}"); }
                    damagedThisTick.Add(root);
                }
            }

            // If we didn't damage anything via OverlapSphere, try a direct player fallback by tag and horizontal distance
            if (damagedThisTick.Count == 0)
            {
                try
                {
                    var playerGo = GameObject.FindGameObjectWithTag("Player");
                    if (playerGo != null)
                    {
                        float pdist3D = Vector3.Distance(playerGo.transform.position, center);
                        float pdistXZ = Vector2.Distance(new Vector2(playerGo.transform.position.x, playerGo.transform.position.z), new Vector2(center.x, center.z));
                        if (pdistXZ <= impactRadius || pdist3D <= impactRadius)
                        {
                            if (dmgThisTick > 0)
                            {
                                var ptpc = playerGo.GetComponent<StarterAssets.ThirdPersonController>();
                                if (ptpc != null)
                                {
                                    try { ptpc.TakeDamage(dmgThisTick); Debug.Log($"[MeteorProjectile] Direct fallback applied {dmgThisTick} damage to Player via TPC. pdistXZ={pdistXZ:F2}"); } catch { }
                                }
                                else
                                {
                                    try { playerGo.SendMessage("TakeDamage", dmgThisTick, SendMessageOptions.DontRequireReceiver); Debug.Log($"[MeteorProjectile] Direct fallback SentMessage TakeDamage={dmgThisTick} to Player. pdistXZ={pdistXZ:F2}"); } catch { }
                                }
                                damagedThisTick.Add(playerGo);
                            }
                        }
                        else if (debugMeteorAoE)
                        {
                            Debug.Log($"[MeteorProjectile] Player fallback check: pdist3D={pdist3D:F2}, pdistXZ={pdistXZ:F2} > radius {impactRadius}");
                        }
                    }
                }
                catch { }
            }

            // If we applied integer damage to any target, consume that integer portion from the accumulator (once per tick)
            if (dmgThisTick > 0)
            {
                accumulator = Mathf.Repeat(accumulator, 1f);
            }

            t += dt;
            yield return null;
        }
    }
}

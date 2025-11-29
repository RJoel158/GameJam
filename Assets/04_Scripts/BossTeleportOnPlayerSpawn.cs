using System.Collections;
using UnityEngine;

/// <summary>
/// Attach this to the Player prefab (or player GameObject in the scene).
/// On Start it will request the BossSpawner to teleport the boss in front of the player
/// and play the provided Hovl effect prefab.
/// </summary>
public class BossTeleportOnPlayerSpawn : MonoBehaviour
{
    [Tooltip("Optional effect prefab (from Hovl Studio) to play when boss teleports to the player")]
    public GameObject hovlEffectPrefab;

    [Tooltip("Duration to keep the spawned effect (seconds). 0 = do not auto destroy")]
    public float effectDuration = 5f;

    [Tooltip("If true, call teleport on Start; otherwise call manually")]
    public bool teleportOnStart = true;

    private void Start()
    {
        if (!teleportOnStart) return;
        StartCoroutine(NotifySpawnerNextFrame());
    }

    private System.Collections.IEnumerator NotifySpawnerNextFrame()
    {
        // wait a frame to ensure all Awake/Start on other objects ran
        yield return null;

        var spawner = FindObjectOfType<BossSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("[BossTeleportOnPlayerSpawn] No BossSpawner found in scene.");
            yield break;
        }

        // Call the public helper to teleport boss and play the effect
        spawner.TeleportBossToPlayerWithEffect(hovlEffectPrefab, effectDuration);

        Debug.Log("[BossTeleportOnPlayerSpawn] Requested boss teleport and VFX.");
    }
}

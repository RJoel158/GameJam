using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Utility methods to align/transmit objects to ground reliably.
/// Use AlignToGround_Average for robust alignment (raycasts + terrain + navmesh fallback).
/// </summary>
public static class GroundUtils
{
    public static bool AlignToGround(Transform t, LayerMask groundMask, float rayStartOffset = 2f, float maxDown = 20f, float smooth = 1f, float minAboveGround = 0.05f)
    {
        Vector3 pos = t.position;
        Vector3 origin = pos + Vector3.up * rayStartOffset;

        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, maxDown, groundMask, QueryTriggerInteraction.Ignore))
        {
            float targetY = hit.point.y;
            // ensure we don't place below the terrain if terrain exists
            if (Terrain.activeTerrain != null)
            {
                float terrainY = Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.GetPosition().y;
                targetY = Mathf.Max(targetY, terrainY + minAboveGround);
            }
            t.position = Vector3.Lerp(t.position, new Vector3(pos.x, targetY, pos.z), smooth);
            return true;
        }

        if (Terrain.activeTerrain != null)
        {
            float terrainY = Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.GetPosition().y;
            if (!float.IsNaN(terrainY))
            {
                t.position = Vector3.Lerp(t.position, new Vector3(pos.x, terrainY + minAboveGround, pos.z), smooth);
                return true;
            }
        }

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(pos, out navHit, 5f, NavMesh.AllAreas))
        {
            float navY = navHit.position.y;
            if (Terrain.activeTerrain != null)
            {
                float terrainY = Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.GetPosition().y;
                navY = Mathf.Max(navY, terrainY + minAboveGround);
            }
            t.position = Vector3.Lerp(t.position, new Vector3(navHit.position.x, navY, navHit.position.z), smooth);
            return true;
        }

        return false;
    }

    public static bool AlignToGround_Average(Transform t, LayerMask groundMask, float radius = 0.5f, int samples = 4, float rayStartOffset = 2f, float maxDown = 20f, float smooth = 1f, float minAboveGround = 0.05f)
    {
        Vector3 pos = t.position;
        Vector3[] offsets = new Vector3[]
        {
            Vector3.zero,
            new Vector3(radius,0,0),
            new Vector3(-radius,0,0),
            new Vector3(0,0,radius),
            new Vector3(0,0,-radius)
        };

        float sumY = 0f;
        int hits = 0;
        int toSample = Mathf.Clamp(samples + 1, 1, offsets.Length);

        for (int i = 0; i < toSample; i++)
        {
            Vector3 samplePos = pos + offsets[i];
            Vector3 origin = samplePos + Vector3.up * rayStartOffset;
            RaycastHit hit;
            if (Physics.Raycast(origin, Vector3.down, out hit, maxDown, groundMask, QueryTriggerInteraction.Ignore))
            {
                sumY += hit.point.y;
                hits++;
            }
            else if (Terrain.activeTerrain != null)
            {
                float terrainY = Terrain.activeTerrain.SampleHeight(samplePos) + Terrain.activeTerrain.GetPosition().y;
                sumY += terrainY;
                hits++;
            }
        }

        if (hits > 0)
        {
            float avgY = sumY / hits;
            // ensure average is not under terrain
            if (Terrain.activeTerrain != null)
            {
                float terrainYAtPos = Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.GetPosition().y;
                avgY = Mathf.Max(avgY, terrainYAtPos + minAboveGround);
            }
            t.position = Vector3.Lerp(t.position, new Vector3(pos.x, avgY, pos.z), smooth);
            return true;
        }

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(pos, out navHit, 5f, NavMesh.AllAreas))
        {
            float navY = navHit.position.y;
            if (Terrain.activeTerrain != null)
            {
                float terrainYAtPos = Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.GetPosition().y;
                navY = Mathf.Max(navY, terrainYAtPos + minAboveGround);
            }
            t.position = Vector3.Lerp(t.position, new Vector3(navHit.position.x, navY, navHit.position.z), smooth);
            return true;
        }

        return false;
    }

    // Helper to warp agent to nav position if needed
    public static bool WarpAgentToNavMesh(NavMeshAgent agent, float maxSampleDistance = 5f)
    {
        if (agent == null) return false;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(agent.transform.position, out hit, maxSampleDistance, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return true;
        }
        return false;
    }
}

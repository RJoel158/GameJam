using UnityEngine;

// Script de prueba: presiona L para simular subir de nivel y P para añadir 1 punto.
public class TestStatsDebug : MonoBehaviour
{
    public PlayerStats playerStats;

    void Update()
    {
        if (playerStats == null) return;

        if (Input.GetKeyDown(KeyCode.L))
        {
            playerStats.LevelUp(3);
            Debug.Log($"LevelUp llamado: nivel={playerStats.level}, puntos={playerStats.availablePoints}");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            playerStats.AddPoints(1);
            Debug.Log($"AddPoints llamado: puntos={playerStats.availablePoints}");
        }
    }
}

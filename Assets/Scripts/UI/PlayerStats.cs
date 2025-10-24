using System;
using UnityEngine;

// Modelo simple de estadísticas del jugador. Mantén esto en un componente en el Player GameObject.
public class PlayerStats : MonoBehaviour
{
    public enum StatType { Strength = 0, Agility = 1, Intelligence = 2 }

    [Header("Basic")]
    public string playerName = "Player";
    public int level = 1;

    [Header("Points")]
    public int availablePoints = 0; // puntos para gastar en subir stats

    [Header("Attributes")]
    public int strength = 1;
    public int agility = 1;
    public int intelligence = 1;

    // Evento simple para notificar cambios a la UI
    public event Action OnStatsChanged;

    public int GetStat(StatType stat)
    {
        switch (stat)
        {
            case StatType.Strength: return strength;
            case StatType.Agility: return agility;
            case StatType.Intelligence: return intelligence;
            default: return 0;
        }
    }

    // Intenta incrementar y devuelve true si tuvo éxito
    public bool IncreaseStat(StatType stat)
    {
        if (availablePoints <= 0) return false;

        switch (stat)
        {
            case StatType.Strength: strength++; break;
            case StatType.Agility: agility++; break;
            case StatType.Intelligence: intelligence++; break;
        }

        availablePoints--;
        OnStatsChanged?.Invoke();
        return true;
    }

    // Útil para pruebas: añadir puntos (p. ej. al subir nivel)
    public void AddPoints(int pts)
    {
        if (pts <= 0) return;
        availablePoints += pts;
        OnStatsChanged?.Invoke();
    }

    // Lógica simple de subir de nivel: incrementa level y añade puntos por nivel.
    // Puedes personalizar cuántos puntos se otorgan por nivel.
    public void LevelUp(int pointsPerLevel = 3)
    {
        level++;
        AddPoints(pointsPerLevel);
    }
}

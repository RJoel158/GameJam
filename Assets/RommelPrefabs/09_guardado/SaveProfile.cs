using UnityEngine;

[CreateAssetMenu(menuName = "Save/SaveProfile")]
public class SaveProfile : ScriptableObject
{
    [Header("Scene Data")]
    public Vector3 playerPosition;
    public string sceneName;
    public bool hasSave = false;

    [Header("Player Stats")]
    public int health;
    public int maxHealth;
    public int force;
    public int maxForce;

    // Puedes agregar más datos aquí si necesitas
    // por ejemplo: inventario, misiones completadas, etc.
}
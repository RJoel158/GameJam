using UnityEngine;

[CreateAssetMenu(menuName = "Save/SaveProfile")]
public class SaveProfile : ScriptableObject
{
    public Vector3 playerPosition;
    public string sceneName;
    public bool hasSave = false;
}

using UnityEngine;

/// <summary>
/// StateMachineBehaviour que activa el collider del arma solo durante el ataque.
/// Se coloca en el estado "Attack" del Animator.
/// </summary>
public class WeaponActivator : StateMachineBehaviour
{
    [Tooltip("Nombre del GameObject que contiene el collider del arma (ej: 'garras_mano_R')")]
    public string weaponObjectName = "garras_mano_R";

    [Tooltip("Tiempo normalizado (0-1) en que se activa el collider")]
    [Range(0f, 1f)]
    public float activateAt = 0.3f;

    [Tooltip("Tiempo normalizado (0-1) en que se desactiva el collider")]
    [Range(0f, 1f)]
    public float deactivateAt = 0.7f;

    private GameObject weaponObject;
    private Collider weaponCollider;
    private bool hasActivated = false;
    private bool hasDeactivated = false;

    // OnStateEnter se llama cuando entra al estado de ataque
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Buscar el objeto del arma si no lo tenemos
        if (weaponObject == null)
        {
            Transform weaponTransform = FindChildRecursive(animator.transform, weaponObjectName);
            if (weaponTransform != null)
            {
                weaponObject = weaponTransform.gameObject;
                weaponCollider = weaponObject.GetComponent<Collider>();
                
                if (weaponCollider != null)
                {
                    Debug.Log($"<color=cyan>[WeaponActivator] Encontrado collider en {weaponObjectName}</color>");
                }
                else
                {
                    Debug.LogError($"<color=red>[WeaponActivator] No se encontró Collider en {weaponObjectName}</color>");
                }
            }
            else
            {
                Debug.LogError($"<color=red>[WeaponActivator] No se encontró GameObject '{weaponObjectName}'</color>");
            }
        }

        // Desactivar el collider al iniciar el ataque
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }

        hasActivated = false;
        hasDeactivated = false;
    }

    // OnStateUpdate se llama en cada frame mientras está en el estado
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (weaponCollider == null) return;

        float normalizedTime = stateInfo.normalizedTime % 1f; // Para loops

        // Activar en el momento del golpe
        if (!hasActivated && normalizedTime >= activateAt)
        {
            weaponCollider.enabled = true;
            hasActivated = true;
            Debug.Log($"<color=green>[WeaponActivator] Arma ACTIVADA en {normalizedTime:F2}</color>");
        }

        // Desactivar después del golpe
        if (hasActivated && !hasDeactivated && normalizedTime >= deactivateAt)
        {
            weaponCollider.enabled = false;
            hasDeactivated = true;
            Debug.Log($"<color=yellow>[WeaponActivator] Arma DESACTIVADA en {normalizedTime:F2}</color>");
        }
    }

    // OnStateExit se llama cuando sale del estado
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Asegurar que el collider queda desactivado
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
            Debug.Log("<color=orange>[WeaponActivator] Arma desactivada al salir del estado</color>");
        }
    }

    /// <summary>
    /// Busca un hijo por nombre recursivamente
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform found = FindChildRecursive(child, childName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
}

using UnityEngine;

/// <summary>
/// StateMachineBehaviour que reproduce el sonido de ataque cuando entra al estado Attack.
/// Se debe agregar como Behaviour en el estado "Attack" del Animator Controller.
/// </summary>
public class AttackSoundBehaviour : StateMachineBehaviour
{
    // OnStateEnter se llama cuando entra al estado de ataque
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Buscar el componente FastEnemy en el mismo GameObject o en el padre
        FastEnemy fastEnemy = animator.GetComponent<FastEnemy>();
        if (fastEnemy == null)
        {
            fastEnemy = animator.GetComponentInParent<FastEnemy>();
        }

        // Reproducir sonido de ataque
        if (fastEnemy != null)
        {
            fastEnemy.PlayFastAttackSound();
            Debug.Log("<color=cyan>[AttackSoundBehaviour] Reproduciendo sonido de ataque</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>[AttackSoundBehaviour] No se encontró FastEnemy para reproducir sonido</color>");
        }
    }
}

using StarterAssets;
using UnityEngine;

public class AttackSystem : MonoBehaviour
{
    [HideInInspector] public bool exitComboTriggered = false;

    [SerializeField] float timePassed = 0f;
    [SerializeField] float clipLength = 0f;
    [SerializeField] float clipSpeed = 0f;

    [SerializeField]
    ThirdPersonController thirdPersonController;
    [SerializeField]
    StarterAssetsInputs starterAssetsInputs;

    [Header("Sword Slashes")]
    public GameObject swordSlah;

    public void StartAnimationAttack()
    {
        thirdPersonController.inAttackAnimation = true;
    }

    public void EndAnimationAttack()
    {
        thirdPersonController.inAttackAnimation = false;
    }

    public void EnterAttack()
    {
        thirdPersonController.isAttacking = true;

        if (PlayerAudioManager.Instance != null)
        {
            PlayerAudioManager.Instance.PlayAttackSwordSound();
            swordSlah.SetActive(true);
        }

        if (thirdPersonController.stamina > 0)
        {
            thirdPersonController.stamina -= thirdPersonController.maxHealth / 2;

            if (thirdPersonController.stamina <= 0)
            {
                thirdPersonController.stamina = 0;
            }
        }

        //thirdPersonController._animator.applyRootMotion = true;
        //timePassed = 0;
    }

    public void ExitAttack()
    {
        thirdPersonController.isAttacking = false;
        swordSlah.SetActive(false);
        //thirdPersonController._animator.applyRootMotion = false;
        //timePassed = 0;
    }

    void Start()
    {

    }


    void Update()
    {
        timePassed += Time.deltaTime;

        if (thirdPersonController.inAttackAnimation)
        {
            // Validar que la capa tenga clips antes de acceder
            AnimatorClipInfo[] clipInfo = thirdPersonController._animator.GetCurrentAnimatorClipInfo(1);

            if (clipInfo.Length > 0)
            {
                clipLength = clipInfo[0].clip.length;
                clipSpeed = thirdPersonController._animator.GetCurrentAnimatorStateInfo(1).speed;

                //Debug.Log($"Attack Clip Length: {clipLength} / Speed: {clipSpeed}");
                //Debug.Log($"Time: {clipLength / clipSpeed}");

                if (timePassed <= (clipLength / clipSpeed) && starterAssetsInputs.attack)
                {
                    thirdPersonController._animator.SetTrigger("Attack");
                    timePassed = 0f;
                }

                if (timePassed >= (clipLength / clipSpeed) || thirdPersonController._animationBlend >= 0.01f)
                {
                    thirdPersonController._animator.SetTrigger("ExitCombo");
                    exitComboTriggered = true;
                    timePassed = 0f;
                }
            }
            else
            {
                // Si no hay clips en la capa, salir del ataque
                Debug.LogWarning("<color=orange>[AttackSystem] No hay clips en la capa 1 del Animator</color>");
                thirdPersonController.inAttackAnimation = false;
            }
        }
    }
}

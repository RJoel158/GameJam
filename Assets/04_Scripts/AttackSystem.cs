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
        //thirdPersonController._animator.applyRootMotion = true;
        //timePassed = 0;
    }

    public void ExitAttack()
    {
        thirdPersonController.isAttacking = false;
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
            // ERRROR DE DESBORDAMIENTO EN EL ARRAY
            clipLength = thirdPersonController._animator.GetCurrentAnimatorClipInfo(1)[0].clip.length;
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
    }
}

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Animator playerAnimator;

    [Header("Equipment")]
    [SerializeField]
    private GameObject sword;
    [SerializeField]
    private GameObject swordOnShoulder;
    public bool isEquipping;
    public bool isEquipped;

    [Header("Block")]
    public bool isBlocking;

    private void Equip()
    {
        if (Input.GetKeyDown(KeyCode.R) && playerAnimator.GetBool("Grounded"))
        {
            isEquipping = true;
            playerAnimator.SetTrigger("Equip");
        }
    }

    public void ActiveWeapon()
    {
        if (!isEquipped)
        {
            sword.SetActive(true);
            swordOnShoulder.SetActive(false);
            isEquipped = !isEquipped;
        }
        else
        {
            sword.SetActive(false);
            swordOnShoulder.SetActive(true);
            isEquipped = !isEquipped;
        }
    }

    public void Equipped()
    {
        isEquipping = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Equip();
        Block();
    }

    private void Block()
    {
        if (Input.GetKey(KeyCode.Mouse1) && playerAnimator.GetBool("Grounded"))
        {
            playerAnimator.SetBool("Block", true);
            isBlocking = true;
        }
        else
        {
            playerAnimator.SetBool("Block", false);
            isBlocking = false;
        }
    }
}

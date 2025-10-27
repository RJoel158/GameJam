using StarterAssets;
using UnityEngine;

public class EquipmentSystem : MonoBehaviour
{
    [SerializeField]
    ThirdPersonController thirdPersonController;

    [SerializeField] GameObject weaponHolder;
    [SerializeField] GameObject weapon;
    [SerializeField] GameObject weaponSheath;

    GameObject currentWeaponInSheath;
    GameObject currentWeaponInHand;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //currentWeaponInSheath = Instantiate(weapon, weaponSheath.transform);
        thirdPersonController = GetComponent<ThirdPersonController>();
    }

    //public void DrawSword()
    //{
    //    Destroy(currentWeaponInSheath);
    //    currentWeaponInHand = Instantiate(weapon, weaponHolder.transform);
    //}

    //public void SheathSword()
    //{

    //    currentWeaponInHand = Instantiate(weapon, weaponSheath.transform);
    //    Destroy(currentWeaponInHand);
    //}

    public void ActiveWeapon()
    {
        if (!thirdPersonController.isEquipped)
        {
            weaponHolder.SetActive(true);
            weaponSheath.SetActive(false);
            thirdPersonController.isEquipped = !thirdPersonController.isEquipped;
        }
        else
        {
            weaponHolder.SetActive(false);
            weaponSheath.SetActive(true);
            thirdPersonController.isEquipped = !thirdPersonController.isEquipped;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

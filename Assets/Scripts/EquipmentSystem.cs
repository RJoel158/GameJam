using StarterAssets;
using UnityEngine;

public class EquipmentSystem : MonoBehaviour
{
    [SerializeField]
    ThirdPersonController thridPersonController;

    [SerializeField] GameObject weaponHolder;
    [SerializeField] GameObject weapon;
    [SerializeField] GameObject weaponSheath;

    GameObject currentWeaponInSheath;
    GameObject currentWeaponInHand;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //currentWeaponInSheath = Instantiate(weapon, weaponSheath.transform);
        thridPersonController = GetComponent<ThirdPersonController>();
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
        if (!thridPersonController.isEquipped)
        {
            weaponHolder.SetActive(true);
            weaponSheath.SetActive(false);
            thridPersonController.isEquipped = !thridPersonController.isEquipped;
        }
        else
        {
            weaponHolder.SetActive(false);
            weaponSheath.SetActive(true);
            thridPersonController.isEquipped = !thridPersonController.isEquipped;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

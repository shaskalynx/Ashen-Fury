using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class EquipmentSystem : MonoBehaviour
{
    [SerializeField] GameObject weaponHolder;
    [SerializeField] GameObject[] weapons = new GameObject[3]; // Array of 3 weapons
    [SerializeField] GameObject weaponSheath;
 
    private int currentWeaponIndex = 0;
    public GameObject currentWeaponInHand;
    GameObject currentWeaponInSheath;

    void Start()
    {
        if (weapons[0] != null)
        {
            currentWeaponInSheath = Instantiate(weapons[0], weaponSheath.transform);
        }
    }

    void Update()
    {
        // Handle weapon switching
        if (Input.GetKeyDown(KeyCode.Alpha1) && weapons[0] != null) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) && weapons[1] != null) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) && weapons[2] != null) SwitchWeapon(2);
    }

    private void SwitchWeapon(int newIndex)
    {
        if (currentWeaponIndex == newIndex) return;
        currentWeaponIndex = newIndex;
        
        if (currentWeaponInHand != null)
        {
            // If weapon is drawn, switch it directly
            Destroy(currentWeaponInHand);
            currentWeaponInHand = Instantiate(weapons[currentWeaponIndex], weaponHolder.transform);
        }
        else if (currentWeaponInSheath != null)
        {
            // If weapon is sheathed, switch in sheath
            Destroy(currentWeaponInSheath);
            currentWeaponInSheath = Instantiate(weapons[currentWeaponIndex], weaponSheath.transform);
        }
    }

    public void DrawWeapon()
    {
        currentWeaponInHand = Instantiate(weapons[currentWeaponIndex], weaponHolder.transform);
        Destroy(currentWeaponInSheath);
        currentWeaponInSheath = null;
    }

    public void SheathWeapon()
    {
        currentWeaponInSheath = Instantiate(weapons[currentWeaponIndex], weaponSheath.transform);
        Destroy(currentWeaponInHand);
        currentWeaponInHand = null;
    }

    public void StartDealDamage()
    {
        currentWeaponInHand.GetComponentInChildren<DamageDealer>().StartDealDamage();
    }
    public void EndDealDamage()
    {
        currentWeaponInHand.GetComponentInChildren<DamageDealer>().EndDealDamage();
    }
}
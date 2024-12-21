using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public List<WeaponController> weaponSlots = new List<WeaponController>(4);
    public int[] weaponLevels = new int[4];
    public List<Image> weaponUISlots = new List<Image>(4);
    public List<PassiveItem> passiveItemSlots = new List<PassiveItem>(4);
    public int[] passiveItemLevels = new int[4];
    public List<Image> passiveItemUISlots = new List<Image>(4);

    public void AddWeapon(int slotIndex, WeaponController weapon) //Add weapon in slot
    {
        weaponSlots[slotIndex] = weapon;
        weaponLevels[slotIndex] = weapon.weaponData.Level;
        weaponUISlots[slotIndex].enabled = true; //Enable the image component
        weaponUISlots[slotIndex].sprite = weapon.weaponData.Icon;
    }

    public void AddPassiveItem(int slotIndex, PassiveItem passiveItem) //Add passive item in slot
    {
        passiveItemSlots[slotIndex] = passiveItem;
        passiveItemLevels[slotIndex] = passiveItem.passiveItemData.Level;
        passiveItemUISlots[slotIndex].enabled = true; //Enable the image component
        passiveItemUISlots[slotIndex].sprite = passiveItem.passiveItemData.Icon;
    }

    public void LevelUpWeapon(int slotIndex)
    {
        if(weaponSlots.Count > slotIndex)
        {
            WeaponController weapon = weaponSlots[slotIndex];
            if(!weapon.weaponData.NextLevelPrefab) //Check there is next level for weapon
            {
                Debug.LogError("No next level for" + weapon.name);
                return;
            }
            GameObject upgradedWeapon = Instantiate(weapon.weaponData.NextLevelPrefab, transform.position, Quaternion.identity);
            upgradedWeapon.transform.SetParent(transform); //Set the weapon to be a child of the player
            AddWeapon(slotIndex, upgradedWeapon.GetComponent<WeaponController>());
            Destroy(weapon.gameObject);
            weaponLevels[slotIndex] = upgradedWeapon.GetComponent<WeaponController>().weaponData.Level; //Make sure to get correct level
        }
    }

    public void LevelUpPassiveitem(int slotIndex)
    {
        PassiveItem passiveItem = passiveItemSlots[slotIndex];
        if(!passiveItem.passiveItemData.NextLevelPrefab) //Check there is next level for passive item
        {
            Debug.LogError("No next level for" + passiveItem.name);
            return;
        }
        GameObject upgradedPassiveItem = Instantiate(passiveItem.passiveItemData.NextLevelPrefab, transform.position, Quaternion.identity);
        upgradedPassiveItem.transform.SetParent(transform); //Set the passive item to be a child of the player
        AddPassiveItem(slotIndex, upgradedPassiveItem.GetComponent<PassiveItem>());
        Destroy(passiveItem.gameObject);
        passiveItemLevels[slotIndex] = upgradedPassiveItem.GetComponent<PassiveItem>().passiveItemData.Level; //Make sure to get correct level
    }
}

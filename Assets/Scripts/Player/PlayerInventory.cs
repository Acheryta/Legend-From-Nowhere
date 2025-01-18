using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public Item item;
        public Image image;

        public void Assign(Item assignedItem)
        {
            item = assignedItem;
            if (item is Weapon)
            {
                Weapon w = item as Weapon;
                image.enabled = true;
                image.sprite =  w.data.icon;
            }
            else
            {
                Passive p = item as Passive;
                image.enabled = true;
                image.sprite = p.data.icon;
            }
            Debug.Log(string.Format("Assigned {0} to player", item.name));
        }
        
        public void Clear()
        {
            item = null;
            image.enabled = false;
            image.sprite = null;
        }

        public bool IsEmpty() {return item == null;}
    }
    public List<Slot> weaponSlots = new List<Slot>(4);
    public List<Slot> passiveSlots = new List<Slot>(4);


    [Header("UI Elements")]
    public List<WeaponData> availableWeapons = new List<WeaponData>();
    public List<PassiveData> availablePassives = new List<PassiveData>();
    public UIUpgradeWindow upgradeWindow;

    PlayerStats player;

    void Start()
    {
        player = GetComponent<PlayerStats>();
    }

    // Check if inventory has an item of certain type
    public bool Has(ItemData type) { return Get(type); }
    public Item Get(ItemData type)
    {
        if(type is WeaponData) return Get(type as WeaponData);
        else if (type is PassiveData) return Get(type as PassiveData);
        return null;
    }

    // Find a passive of a certain type in the inventory
    public Passive Get(PassiveData type)
    {
        foreach (Slot s in passiveSlots)
        {
            Passive p = s.item as Passive;

            if (p && p.data == type) return p;
        }
        return null;
    }

    // Find a weapon of a certain type in the inventory
    public Weapon Get(WeaponData type)
    {
        foreach (Slot s in weaponSlots)
        {
            Weapon w = s.item as Weapon;

            if (w && w.data == type) return w;
        }
        return null;
    }

    // Remove a weapon of a particular type
    public bool Remove(WeaponData data, bool removeUpgradeAvailability = false)
    {
        // Remove this weapon from the upgrade pool
        if(removeUpgradeAvailability) availableWeapons.Remove(data);

        for(int i = 0; i < weaponSlots.Count; i++)
        {
            Weapon w = weaponSlots[i].item as Weapon;
            if(w.data == data)
            {
                weaponSlots[i].Clear();
                w.OnUnequip();
                Destroy(w.gameObject);
                return true;
            }
        }

        return false;
    }

    // Remove a passive of a particular type
    public bool Remove(PassiveData data, bool removeUpgradeAvailability = false)
    {
        // Remove this passive from the upgrade pool
        if(removeUpgradeAvailability) availablePassives.Remove(data);

        for(int i = 0; i < passiveSlots.Count; i++)
        {
            Passive p = passiveSlots[i].item as Passive;
            if(p.data == data)
            {
                passiveSlots[i].Clear();
                p.OnUnequip();
                Destroy(p.gameObject);
                return true;
            }
        }
        
        return false;
    }

    // If an ItemData is passed, determine what type it is and call the respective overload
    // also have an optional boolean to remove this item from the upgrade list
    
    public bool Remove(ItemData data, bool removeUpgradeAvailability = false)
    {
        if(data is PassiveData) return Remove(data as PassiveData, removeUpgradeAvailability);
        else if(data is WeaponData) return Remove(data as WeaponData, removeUpgradeAvailability);
        
        return false;
    }

    // Find an empty slot and add a weapon of a certain type 
    // and return the slot number that the item was put in
    
    public int Add(WeaponData data)
    {
        int slotNum = -1;

        // Try to find empty slot
        for(int i = 0; i < weaponSlots.Capacity; i++)
        {
            if(weaponSlots[i].IsEmpty())
            {
                slotNum = i;
                break;
            }
        }

        // If there is no empty slot. exit
        if(slotNum < 0) return slotNum;

        // Otherwise create the weapon in the slot
        // Get the type of the weapon want to spawn
        Type weaponType = Type.GetType(data.behaviour);

        if(weaponType != null)
        {
            // Spawn the weapon GameObject
            GameObject go = new GameObject(data.baseStats.name + " Controller");
            Weapon spawnedWeapon = (Weapon)go.AddComponent(weaponType);
            spawnedWeapon.transform.SetParent(transform); //Set the weapon to be a child of the player
            spawnedWeapon.transform.localPosition = Vector2.zero;
            spawnedWeapon.Initialise(data);
            spawnedWeapon.OnEquip();

            // Assign the weapon to the slot
            weaponSlots[slotNum].Assign(spawnedWeapon);

            // Close the level up UI if it is on
            if(GameManager.instance != null && GameManager.instance.choosingUpgrade)
            {
                GameManager.instance.EndLevelUp();
            }

            return slotNum;
        }
        else
        {
            Debug.LogWarning(string.Format("Invalid weapon type specified for {0}", data.name));
        }

        return -1;
    }

    // Find an empty slot and add a passive of a certain type 
    // and return the slot number that the item was put in
    
    public int Add(PassiveData data)
    {
        int slotNum = -1;

        // Try to find empty slot
        for(int i = 0; i < passiveSlots.Capacity; i++)
        {
            if(passiveSlots[i].IsEmpty())
            {
                slotNum = i;
                break;
            }
        }

        // If there is no empty slot. exit
        if(slotNum < 0) return slotNum;

        // Otherwise create the passive in the slot
        // Get the type of the passive want to spawn

        GameObject go = new GameObject(data.baseStats.name + " Passive");
        Passive p = go.AddComponent<Passive>();
        p.Initialise(data);
        p.transform.SetParent(transform); //Set the passive to be a child of the player
        p.transform.localPosition = Vector2.zero;
        
        // Assign the passive to the slot
        passiveSlots[slotNum].Assign(p);

        // Close the level up UI if it is on
        if(GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
        return slotNum;
    }

    public int Add(ItemData data)
    {
        if(data is PassiveData) return Add(data as PassiveData);
        else if(data is WeaponData) return Add(data as WeaponData);
        
        return -1;
    }

    // Overload so that we can use both ItemData or Item to level up item
    public bool LevelUp(ItemData data)
    {
        Item item = Get(data);
        if(item) return LevelUp(item);
        return false;
    }

    //Level up a selected weapon in inventory
    public bool LevelUp(Item item)
    {
        //Tries to level up the item
        if(!item.DoLevelUp())
        {
            Debug.LogWarning(string.Format("Failed to level up {0}", item.name));
            return false;
        }

        //Close the level up screen
        if(GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }

        //If it is a passive, recaculate player stats
        if(item is Passive) player.RecalculateStats();
        return true;
    }

    //Check a list of slots to see if ther are any slot left
    int GetSlotsLeft(List<Slot> slots)
    {
        int count = 0;
        foreach (Slot s in slots)
        {
            if(s.IsEmpty()) count++;
        }
        return count;
    }

    // Determines what upgrade options should appear.
    void ApplyUpgradeOptions()
    {
        List<ItemData> availableUpgrades = new List<ItemData>(); //list of ALL upgrades in PlayerInventory
        List<ItemData> allUpgrades = new List<ItemData>(availableWeapons); 
        allUpgrades.AddRange (availablePassives);

        //How many weapon / passive slots are left.
        int weaponSlotsLeft = GetSlotsLeft(weaponSlots);
        int passiveSlotsLeft = GetSlotsLeft(passiveSlots);

        // Filters through the available weapons and passives and add to possibly be an option.
        foreach(ItemData data in allUpgrades)
        {
            // If a weapon of this type exists, allow for the upgrade if its not max level
            Item obj = Get(data);
            if (obj)
            {
                if (obj.currentLevel < data.maxLevel) availableUpgrades.Add(data);
            }
            else
            {
                // If don't have this item in the inventory add it if still have slots left
                if (data is WeaponData && weaponSlotsLeft > 0) availableUpgrades.Add(data);
                else if (data is PassiveData && passiveSlotsLeft > 0) availableUpgrades.Add(data);
            }
        }

        //Show the UI upgrade window if still have available upgrade left
        int availUpgradeCount = availableUpgrades.Count;
        if(availUpgradeCount > 0)
        {
            bool getExtraItem = 1f - 1f / player.Stats.luck > UnityEngine.Random.value;
            if(getExtraItem || availUpgradeCount < 4) upgradeWindow.SetUpgrades(this, availableUpgrades, 3);
            else upgradeWindow.SetUpgrades(this, availableUpgrades, 3, "Increase your Luck stats for a chance to get 4 items!");
        }
        else if(GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }

    }

    public void RemoveAndApplyUpgrades()
    {
        ApplyUpgradeOptions();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public int currentLevel = 1, maxLevel = 1;
    protected ItemData.Evolution[] evolutionData;
    protected PlayerInventory inventory;
    protected PlayerStats owner;

    public PlayerStats Owner{get {return owner;}}

    [System.Serializable]
    public class LevelData
    {
        public string name, description;
    }

    public virtual void Initialise(ItemData data)
    {
        maxLevel = data.maxLevel;
        //Store the evolution data as we have to check
        // all the catalysts(passive) are in the inventory so that can evolve
        evolutionData = data.evolutionData;

        inventory = FindObjectOfType<PlayerInventory>();
        owner = FindObjectOfType<PlayerStats>();
    }

    // Call this function to get all the evolution that the weapon can evolve now
    public virtual ItemData.Evolution[] CanEvolve()
    {
        List<ItemData.Evolution> possibleEvolutions = new List<ItemData.Evolution>();

        // Check each listed evolution
        foreach (ItemData.Evolution e in evolutionData)
        {
            if(CanEvolve(e)) possibleEvolutions.Add(e);
        }

        return possibleEvolutions.ToArray();
    }

    // Check if a specific evolution is possible
    public virtual bool CanEvolve(ItemData.Evolution evolution, int levelUpAmount = 1)
    {
        //Cannot evolve if the item hasn't reach the level to evolve
        if(evolution.evolutionLevel > currentLevel + levelUpAmount)
        {
            Debug.LogWarning(string.Format("Evolution failed, current level {0} evolution level {1}", currentLevel, evolution.evolutionLevel));
            return false;
        }
        
        //Check to see if all catalysts are in inventory
        foreach(ItemData.Evolution.Config c in evolution.catalysts)
        {
            Item item = inventory.Get(c.itemType);
            if(!item || item.currentLevel < c.level)
            {
                Debug.LogWarning(string.Format("Evolution failed, missing {0}", c.itemType.name));
                return false;
            }
        }
        return true;
    }

    //AttemptEvolution will spawn a new weapon and remove the weapon that consumed for evolve
    public virtual bool AttemptEvolution(ItemData.Evolution evolutionData, int levelUpAmount = 1)
    {
        if(!CanEvolve(evolutionData, levelUpAmount)) return false;
        
        //Should consume passive / weapon
        bool consumePassives = (evolutionData.consumes & ItemData.Evolution.Consumption.passives) > 0;
        bool consumeWeapons = (evolutionData.consumes & ItemData.Evolution.Consumption.weapons) > 0;
        
        //Loop through all the catalyst and check if we should consume them
        foreach(ItemData.Evolution.Config c in evolutionData.catalysts)
        {
            if(c.itemType is PassiveData && consumePassives) inventory.Remove(c.itemType, true);
            if(c.itemType is WeaponData && consumeWeapons) inventory.Remove(c.itemType, true);
        }

        // Should we consume ourselves as well
        if(this is Passive && consumePassives) inventory.Remove((this as Passive).data, true);
        else if(this is Weapon && consumeWeapons) inventory.Remove((this as Weapon).data, true);

        // Add the new weapon to inventory
        inventory.Add(evolutionData.outcome.itemType);

        return true;
    }
    public virtual bool CanLevelUp()
    {
        return currentLevel <= maxLevel;
    }

    // Whenever an item level up, attempt to make it evolve
    public virtual bool DoLevelUp()
    {
        if(evolutionData == null) return true;
        
        // Tries to evolve into every listed evolution of this weapon,
        //if the weapon's evolution condition is leveling up
        foreach(ItemData.Evolution e in evolutionData)
        {
            if(e.condition == ItemData.Evolution.Condition.auto) 
            {
                AttemptEvolution(e);
            }
        }
        return true;
    }

    public virtual void OnEquip() {}

    public virtual void OnUnequip() {}
}

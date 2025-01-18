using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Data", menuName = "ScriptableObjects/Weapon Data")]
public class WeaponData : ItemData
{
    [HideInInspector] public string behaviour;
    public Weapon.Stats baseStats;
    public Weapon.Stats[] linearGrowth;
    public Weapon.Stats[] randomGrowth;

    // Give the stat growth / description of the next level
    public override Item.LevelData GetLevelData(int level)
    {   
        if(level <= 1) return baseStats;
        
        // Pick the stats from the next level
        if (level - 2 < linearGrowth.Length)
        {
            return linearGrowth[level - 2];
        }
        // Otherwise pick one of the stats from the random growth array
        if(randomGrowth.Length > 0)
        {
            return randomGrowth[Random.Range(0, randomGrowth.Length)];
        }

        // Return empty
        Debug.LogWarning(string.Format("Weapon doesn't have its level up stats for Level {0}", level));
        return new Weapon.Stats();
    }
}

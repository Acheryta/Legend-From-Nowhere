using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Passive Data", menuName ="ScriptableObjects/Passive Data")]
public class PassiveData : ItemData
{
    public Passive.Modifier baseStats;
    public Passive.Modifier[] growth;

    public override Item.LevelData GetLevelData(int level)
    {
        if (level <= 1) return baseStats;
        
        // Pick the stats from the next level
        if (level - 2 < growth.Length)
        {
            return growth[level - 2];
        }

        // Return an empty value and a warning
        Debug.LogWarning(string.Format("Passive doesn't have its level up stats for Level {0}", level));
        return new Passive.Modifier();
    }
}

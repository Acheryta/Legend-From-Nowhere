using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aura : WeaponEffect
{
    Dictionary<EnemyStats, float> affectedTargets = new Dictionary<EnemyStats, float>();
    List<EnemyStats> targetToUnaffect = new List<EnemyStats>();

    void Update()
    {
        Dictionary<EnemyStats, float> affectedTargsCopy = new Dictionary<EnemyStats, float>(affectedTargets);

        //Loop through every target by the aura, and reduce the cooldown
        // of the aura for it. If the cooldown reach 0, deal damge to it

        foreach(KeyValuePair<EnemyStats, float> pair in affectedTargsCopy)
        {
            affectedTargets[pair.Key] -= Time.deltaTime;
            if(pair.Value <= 0)
            {
                if(targetToUnaffect.Contains(pair.Key))
                {
                    //If target is marked for removeal. remove it
                    affectedTargets.Remove(pair.Key);
                    targetToUnaffect.Remove(pair.Key); 
                }
                else
                {
                    //Reset the cooldown and deal damage
                    Weapon.Stats stats = weapon.GetStats();
                    affectedTargets[pair.Key] = stats.cooldown * Owner.Stats.cooldown;
                    pair.Key.TakeDamage(GetDamge(), transform.position, stats.knockback);

                    //Play the hit effect
                    if(stats.hitEffect)
                    {
                        Destroy(Instantiate(stats.hitEffect, pair.Key.transform.position, Quaternion.identity), 5f);
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.TryGetComponent(out EnemyStats es))
        {
            // If the target is not yet affected by aura,
            // add it to list of affected targets
            if(!affectedTargets.ContainsKey(es))
            {
                //Start with 0 so it will get damged in next Update()
                affectedTargets.Add(es, 0);
            }
            else
            {
                if(targetToUnaffect.Contains(es))
                {
                    targetToUnaffect.Remove(es);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.TryGetComponent(out EnemyStats es))
        {
            if(affectedTargets.ContainsKey(es))
            {
                targetToUnaffect.Add(es);
            }
        }
    }
}

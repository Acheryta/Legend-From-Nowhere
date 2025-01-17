using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterRingWeapon : ProjectileWeapon
{
    List<EnemyStats> allSelectedEnemies = new List<EnemyStats>();

    protected override bool Attack(int attackCount = 1)
    {
        // If no hit effect prefab is assigned, leave warning
        if(!currentStats.hitEffect)
        {
            Debug.LogWarning(string.Format("Hit effect prefab not set for {}", name));
            ActivateCooldown(true);
            return false;
        }

        // If there is no projectile assigned, set the weapon on cooldown
        if(!CanAttack()) return false;

        // If this is the first time the attack, reset the selectedd enemies
        if(currentCooldown <= 0)
        {
            allSelectedEnemies = new List<EnemyStats>(FindObjectsOfType<EnemyStats>());
            ActivateCooldown(true);
            currentAttackCount = attackCount;
        }

        //Find an enemy in the map to strike with water
        EnemyStats target = PickEnemy();
        if(target)
        {
            DamageArea(target.transform.position, GetArea(), GetDamage());
            Instantiate(currentStats.hitEffect, target.transform.position, Quaternion.identity);
        }

        // If there is a proc effect
        if(currentStats.procEffect)
        {
            Destroy(Instantiate(currentStats.procEffect, owner.transform), 5f);
        }

        //If we have more than 1 attack
        if(attackCount > 0)
        {
            currentAttackCount = attackCount - 1;
            currentAttackInterval = currentStats.projectileInteval;
        }

        return true;
    }

    //Randomly pick an enemy on screen
    EnemyStats PickEnemy()
    {
        EnemyStats target = null;
        while(!target && allSelectedEnemies.Count > 0)
        {
            int idx = Random.Range(0, allSelectedEnemies.Count);
            target = allSelectedEnemies[idx];

            //If the target is already dead, remove it and skip
            if(!target)
            {
                allSelectedEnemies.RemoveAt(idx);
                continue;
            }

            //Check if the enemy is on screen 
            Renderer r = target.GetComponent<Renderer>();
            if(!r || !r.isVisible)
            {
                allSelectedEnemies.Remove(target);
                target = null;
                continue;
            }
        }

        allSelectedEnemies.Remove(target);
        return target;
    }

    //Deal damge in an area
    void DamageArea(Vector2 position, float radius, float damage)
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(position, radius);
        foreach (Collider2D t in targets)
        {
            EnemyStats es = t.GetComponent<EnemyStats>();
            if(es) es.TakeDamage(damage, transform.position);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordWeapon : ProjectileWeapon
{
    int currentSpawnCount; // How many time the sword has been attacking in this iteration
    float currentSpawnYOffset; //If there are more than 2 sword, we will start offsetting upwards

    protected override bool Attack(int attackCount = 1)
    {
        // If no projectile prefab is assigned, leave warning
        if(!currentStats.projectilePrefab)
        {
            Debug.LogWarning(string.Format("Projectile prefab not set for {}", name));
            currentCooldown = data.baseStats.cooldown;
            return false;
        }

        // If there is no projectile assigned, set the weapon on cooldown
        if(!CanAttack()) return false;

        // If this is the first time the attack, reset the currentSpawnCount
        if(currentCooldown <= 0)
        {
            currentSpawnCount = 0;
            currentSpawnYOffset = 0f;
        }

        // Otherwise, calculate the angle and offset
        // if <currentSpawnCount> is even mean there more than 1
        // flip the direction
        float spawnDir = Mathf.Sign(movement.lastMovedVector.x) * (currentSpawnCount % 2 != 0 ? -1 : 1);
        Vector2 spawnOffset = new Vector2(
            spawnDir * Random.Range(currentStats.spawnVariance.xMin, currentStats.spawnVariance.xMin),
            currentSpawnYOffset
        );

        // If there is a proc effect
        if(currentStats.procEffect)
        {
            Destroy(Instantiate(currentStats.procEffect, owner.transform), 5f);
        }

        // Spawn a copy of the projectile
        Projectile prefab = Instantiate(
            currentStats.projectilePrefab, 
            owner.transform.position + (Vector3)spawnOffset,
            Quaternion.identity
        );
        prefab.owner = owner;

        //Flip the projectile's sprite
        if(spawnDir < 0)
        {
            prefab.transform.localScale = new Vector3(
                -Mathf.Abs(prefab.transform.localScale.x),
                prefab.transform.localScale.y,
                prefab.transform.localScale.z
            );
            Debug.Log(spawnDir + " | " + prefab.transform.localScale);
        }

        // Assign the stats
        prefab.weapon = this;
        currentCooldown = data.baseStats.cooldown;
        attackCount--;

        //Determine where the next projectile spawn
        currentSpawnCount++;
        if(currentSpawnCount > 1 && currentSpawnCount % 2 == 0)
        {
            currentSpawnYOffset += 1;
        }
        // Perforn another attack if there are more
        if (attackCount > 0)
        {
            currentAttackCount = attackCount;
            currentAttackInterval = data.baseStats.projectileInteval;
        }

        return true;
    }

}

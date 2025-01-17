using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeapon : Weapon
{   
    protected float currentAttackInterval;
    protected int currentAttackCount; // Number of times this attack will happen


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // If the attack interval goes from above 0 or below, also call attack
        if (currentAttackInterval > 0)
        {
            currentAttackInterval -= Time.deltaTime;
            if (currentAttackInterval <= 0) Attack(currentAttackCount);
        }
    }

    public override bool CanAttack()
    {
        if(currentAttackCount > 0) return true;
        return base.CanAttack();
    }

    protected override bool Attack(int attackCount = 1)
    {
        // If no projectile prefab is assigned, get warning message
        if (!currentStats.projectilePrefab)
        {
            Debug.LogWarning(string.Format("Projectile prefab has not been set for {0}", name));
            currentCooldown = data.baseStats.cooldown;
            return false;
        }

        if(!CanAttack()) return false;

        // Otherwise caculate the angle and offset of spawned projectile
        float spawnAngle = GetSpawnAngle();

        // If there is a proc effect
        if(currentStats.procEffect)
        {
            Destroy(Instantiate(currentStats.procEffect, owner.transform), 5f);
        }

        // Spawn a copy of the projectile
        Projectile prefab = Instantiate(
            currentStats.projectilePrefab, 
            owner.transform.position + (Vector3)GetSpawnOffset(spawnAngle),
            Quaternion.Euler(0, 0, spawnAngle)
        );

        prefab.weapon = this;
        prefab.owner = owner;

        // Reset the cooldown only if this attack was triggered by cooldown
        if (currentCooldown <= 0)
        {
            currentCooldown += currentStats.cooldown;
        }
        attackCount--;
        // Perforn another attack if there are more
        if (attackCount > 0)
        {
            currentAttackCount = attackCount;
            currentAttackInterval = data.baseStats.projectileInteval;
        }

        return true;
    }

    // Get which direction the projectile should face
    protected virtual float GetSpawnAngle()
    {
        return Mathf.Atan2(movement.lastMovedVector.y, movement.lastMovedVector.x) * Mathf.Rad2Deg;
    }

    // Generate a random point to spawn the projectile
    // and rotate the facing of the point by spawnAngle
    protected virtual Vector2 GetSpawnOffset(float spawnAngle = 0)
    {
        return Quaternion.Euler(0, 0, spawnAngle) * new Vector2(
            Random.Range(currentStats.spawnVariance.xMin, currentStats.spawnVariance.xMax),
            Random.Range(currentStats.spawnVariance.yMin, currentStats.spawnVariance.yMax)
        );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Item
{
    [System.Serializable]
    public class Stats : LevelData
    {
        [Header("Visuals")]
        public Projectile projectilePrefab;
        public Aura auraPrefab;
        public ParticleSystem hitEffect, procEffect;
        public Rect spawnVariance;

        [Header("Values")]
        public float lifespan;
        public float damage, damageVariance, area, speed, cooldown, projectileInteval, knockback;
        public int number, piercing, maxInstances;

        // Operator + to add 2 Stats together
        public static Stats operator +(Stats s1, Stats s2)
        {
            Stats result = new Stats();
            result.name = s2.name ?? s1.name;
            result.description = s2.description ?? s1.description;
            result.projectilePrefab = s2.projectilePrefab ?? s1.projectilePrefab;
            result.auraPrefab = s2.auraPrefab ?? s1.auraPrefab;
            result.hitEffect = s2.hitEffect == null ? s1.hitEffect : s2.hitEffect;
            result.procEffect = s2.procEffect == null ? s1.procEffect : s2.procEffect;
            result.spawnVariance = s2.spawnVariance;
            result.lifespan = s1.lifespan + s2.lifespan;
            result.damage = s1.damage + s2.damage;
            result.area = s1.area + s2.area;
            result.speed = s1.speed + s2.speed;
            result.cooldown = s1.cooldown + s2.cooldown;
            result.number = s1.number + s2.number;
            result.piercing = s1.piercing + s2.piercing;
            result.projectileInteval = s1.projectileInteval + s2.projectileInteval;
            result.knockback = s1.knockback + s2.knockback;
            return result;
        }

        // Get damage dealt
        public float GetDamage()
        {
            return damage + Random.Range(0, damageVariance);
        }
    }

    protected Stats currentStats;
    public WeaponData data;
    protected float currentCooldown;
    protected PlayerMovement movement;

    // For dynamically created weapons
    public virtual void Initialise(WeaponData data)
    {
        base.Initialise(data);
        this.data = data;
        currentStats = data.baseStats;
        movement = GetComponentInParent<PlayerMovement>();
        ActivateCooldown();
    }

    protected virtual void Update()
    {
        currentCooldown -= Time.deltaTime;
        if(currentCooldown <= 0f) // Do attack if 0
        {
            Attack(currentStats.number + owner.Stats.amount);
        }
    }

    public override bool DoLevelUp()
    {
        base.DoLevelUp();
        // Prevent level up if max level
        if(!CanLevelUp())
        {
            Debug.LogWarning(string.Format("Cannot level up {0} to Level {1}, max level of {2} already reached.", name, currentLevel, data.maxLevel));
            return false;
        }

        // Otherwise add stats of the next level to our weapon
        currentStats += (Stats)data.GetLevelData(++currentLevel);
        return true;
    }

    // Check if can attack now
    public virtual bool CanAttack()
    {
        return currentCooldown <= 0;
    }

    // Do Attack with the weapon
    protected virtual bool Attack(int attackCount = 1)
    {
        if(CanAttack())
        {
            ActivateCooldown();
            return true;
        }
        return false;
    }

    // Get the amount of damage that the weapon deal
    // Factoring in the weapon's stats as well as the character's Might
    public virtual float GetDamage()
    {
        return currentStats.GetDamage() * owner.Stats.might;
    }

    // Get the area, including from the player stats
    public virtual float GetArea()
    {
        return currentStats.area + owner.Stats.area;
    }

    // For retrieving the weapon's stats
    public virtual Stats GetStats() {return currentStats;}

    //Refresh the cooldown of the weapon
    // refresh only when the currentCooldown <0
    public virtual bool ActivateCooldown(bool strict = false)
    {
        //When strict is enabled and the cooldown is not yet finished, dont refresh
        if(strict && currentCooldown > 0) return false;

        //Caculate cooldown going to be
        float actualCooldown = currentStats.cooldown * Owner.Stats.cooldown;
        
        //Limit the maximum cooldown to actual cooldown so the cooldown can't go above the cooldown stat
        currentCooldown = Mathf.Min(actualCooldown, currentCooldown + actualCooldown);
        return true;
    }
}

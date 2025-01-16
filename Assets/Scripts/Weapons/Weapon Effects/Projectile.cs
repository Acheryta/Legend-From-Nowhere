using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : WeaponEffect
{
    public enum DamageSource {projectile, owner}
    public DamageSource damageSource = DamageSource.projectile;
    public bool hasAutoAim = false;
    public Vector3 rotationSpeed = new Vector3(0, 0, 0);

    protected Rigidbody2D rb;
    protected int piercing;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Weapon.Stats stats = weapon.GetStats();
        if(rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.angularVelocity = rotationSpeed.z;
            rb.velocity = transform.right * stats.speed;
        }

        // Prevent the area from being 0
        float area = stats.area == 0 ? 1 : stats.area;
        transform.localScale = new Vector3(
            area * Mathf.Sign(transform.localScale.x),
            area * Mathf.Sign(transform.localScale.y), 1
        );

        // Set how much piercing this object has
        piercing = stats.piercing;

        // Destroy the project after its lifespan expires
        if(stats.lifespan > 0) Destroy(gameObject, stats.lifespan);

        // If the projectile is auto-aiming, automatically find a suitable enemy
        if(hasAutoAim) AcquireAutoAimFacing();
    }

    // If the project is homing, it will automatically find a suitable target
    public virtual void AcquireAutoAimFacing()
    {   
        float aimAngle; // Determine where to aim

        // Find all enemies on screen
        EnemyStats[] targets = FindObjectsOfType<EnemyStats>();

        // Select a random enemy (if have at least 1)
        // else pick random angle
        if(targets.Length > 0)
        {
            EnemyStats selectedTarget = targets[Random.Range(0, targets.Length)];
            Vector2 difference = selectedTarget.transform.position -  transform.position;
            aimAngle = Mathf.Atan2(difference.y, difference.x);
        }
        else
        {
            aimAngle = Random.Range(0f, 360f);
        }

        // Point the projectile towards where we are aiming at
        transform.rotation = Quaternion.Euler(0, 0, aimAngle);
    }

    protected virtual void FixedUpdate()
    {
        // Only drive movement ourselves if this is a kinematic
        if(rb.bodyType == RigidbodyType2D.Kinematic)
        {
            Weapon.Stats stats = weapon.GetStats();
            transform.position += transform.right * stats.speed *  Time.fixedDeltaTime;
            rb.MovePosition(transform.position);
            transform.Rotate(rotationSpeed * Time.fixedDeltaTime);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        EnemyStats es = other.GetComponent<EnemyStats>();
        BreakableProps p = other.GetComponent<BreakableProps>();

        // Only collide with enemies or breakable stuff
        if(es)
        {
            // If damage source is owner
            // use knockback the owner instead of the projectile
            Vector3 source = damageSource == DamageSource.owner && owner ? owner.transform.position : transform.position;
            // Deals damage and destroy the projectile
            es.TakeDamage(GetDamge(), source);

            Weapon.Stats stats = weapon.GetStats();
            piercing--;
            if(stats.hitEffect)
            {
                Destroy(Instantiate(stats.hitEffect, transform.position, Quaternion.identity), 5f);
            }
        }
        else if(p)
        {
            p.TakeDamage(GetDamge());
            piercing--;

            Weapon.Stats stats = weapon.GetStats();
            if(stats.hitEffect)
            {
                Destroy(Instantiate(stats.hitEffect, transform.position, Quaternion.identity), 5f);
            }
        }

        // Destroy this object if it has run out of health
        if(piercing <= 0) Destroy(gameObject);
    }
}

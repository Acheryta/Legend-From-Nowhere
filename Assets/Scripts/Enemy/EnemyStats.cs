using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStats : MonoBehaviour
{
    public EnemyScriptableObject enemyData;
    
    //Current stats
    [HideInInspector]
    public float currentMoveSpeed;
    [HideInInspector]
    public float currentHealth;
    [HideInInspector]
    public float currentDamage;

    public float despawnDistance = 20f;
    Transform player;

    [Header("Damage Feedback")]
    public Color damageColor = new Color(1,0,0,1);
    public float damageFlashDuration = 0.2f;
    public float deathFadeTime = 0.6f;
    Color originalColor;
    SpriteRenderer sr;
    EnemyMovement movement;
    public bool isDead = false;
    public bool isBoss = false;
    void Awake()
    {
        currentMoveSpeed = enemyData.MoveSpeed;
        currentDamage = enemyData.Damage;
        currentHealth = enemyData.MaxHealth;
    }

    void Start()
    {
        player = FindObjectOfType<PlayerStats>().transform;
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;

        movement = GetComponent<EnemyMovement>();
    }

    void Update()
    {
        if(Vector2.Distance(transform.position, player.position) >= despawnDistance)
        {
            ReturnEnemy();
        }
    }
    public void TakeDamage(float dmg, Vector2 sourcePosition, float knockbackForce = 5f, float knockbackDuration = 0.2f)
    {
        if(!isDead)
        {
            currentHealth -= dmg;
            StartCoroutine(DamageFlash());

            //Create text popup when enemy takes damage
            if(dmg > 0)
            {
                GameManager.GenerateFloatingText(Mathf.FloorToInt(dmg).ToString(),transform);
            }

            //Apply knockback
            if(knockbackForce > 0)
            {
                Vector2 dir = (Vector2)transform.position - sourcePosition;
                movement.Knockback(dir.normalized * knockbackForce, knockbackDuration);
            }

            if(currentHealth <= 0)
            {
                Kill();
            }
        }
    }

    //This is a Coroutine function that make the enemy flash when taking damagae
    IEnumerator DamageFlash()
    {
        sr.color = damageColor;
        yield return new WaitForSeconds(damageFlashDuration);
        sr.color = originalColor;
    }

    public void Kill()
    {
        isDead = true;
        EnemySpawner es = FindObjectOfType<EnemySpawner>();
        es.OnEnemyKilled();
        StartCoroutine(KillFade());
        if (isBoss && !GameManager.instance.isGameOver)
        {
            // Call Kill() from PlayerStats so that show the result
            PlayerStats player = FindObjectOfType<PlayerStats>();
            if (player != null)
            {
                player.Kill();
            }
        }
    }

    IEnumerator KillFade()
    {
        // Wait for a single frame
        WaitForEndOfFrame w = new WaitForEndOfFrame();
        float t = 0, origAlpha = sr.color.a;

        while(t < deathFadeTime)
        {
            yield return w;
            t += Time.deltaTime;
            //Set color for this frame
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, (1 - t / deathFadeTime) * origAlpha);
        }
        Destroy(gameObject);
    }

    public void OnCollisionStay2D(Collision2D col)
    {
        if(!isDead)
        {
            //Reference the script from collided collider and deal damage using TakeDamage()
            if(col.gameObject.CompareTag("Player"))
            {
                PlayerStats player = col.gameObject.GetComponent<PlayerStats>();
                player.TakeDamage(currentDamage);
            }
        }
    }

    void ReturnEnemy()
    {
        EnemySpawner es = FindObjectOfType<EnemySpawner>();
        transform.position = player.position + es.relativeSpawnPoints[Random.Range(0, es.relativeSpawnPoints.Count)].position;
    }
}

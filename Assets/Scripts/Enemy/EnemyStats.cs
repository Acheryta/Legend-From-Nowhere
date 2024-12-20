using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

    void Awake()
    {
        currentMoveSpeed = enemyData.MoveSpeed;
        currentDamage = enemyData.Damage;
        currentHealth = enemyData.MaxHealth;
    }

    void Start()
    {
        player = FindObjectOfType<PlayerStats>().transform;
    }

    void Update()
    {
        if(Vector2.Distance(transform.position, player.position) >= despawnDistance)
        {
            ReturnEnemy();
        }
    }
    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        if(currentHealth <= 0)
        {
            Kill();
        }
    }

    public void Kill()
    {
        EnemySpawner es = FindObjectOfType<EnemySpawner>();
        es.OnEnemyKilled();
        Destroy(gameObject);
    }

    public void OnCollisionStay2D(Collision2D col)
    {
        //Reference the script from collided collider and deal damage using TakeDamage()
        if(col.gameObject.CompareTag("Player"))
        {
            PlayerStats player = col.gameObject.GetComponent<PlayerStats>();
            player.TakeDamage(currentDamage);
        }
    }

    void ReturnEnemy()
    {
        EnemySpawner es = FindObjectOfType<EnemySpawner>();
        transform.position = player.position + es.relativeSpawnPoints[Random.Range(0, es.relativeSpawnPoints.Count)].position;
    }
}

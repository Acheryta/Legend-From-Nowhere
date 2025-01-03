using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    EnemyStats enemy;
    Transform player;

    Vector2 knockbackVelocity;
    float knockbackDuration;

    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<EnemyStats>();
        player = FindObjectOfType<PlayerMovement>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        //If currently being knockback
        if(knockbackDuration > 0)
        {
            transform.position += (Vector3)knockbackVelocity * Time.deltaTime;
            knockbackDuration -= Time.deltaTime;
        }
        else
        {
            //Otherwise, move to the player
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, enemy.currentMoveSpeed * Time.deltaTime); //move to player
        }
    }

    public void Knockback(Vector2 velocity, float duration)
    {
        if(knockbackDuration > 0)
        {
            return;
        }

        //Begin knockback
        knockbackVelocity = velocity;
        knockbackDuration = duration;
    }
}

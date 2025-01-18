using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    EnemyStats enemy;
    Transform player;
    Animator animator;

    Vector2 knockbackVelocity;
    float knockbackDuration;

    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<EnemyStats>();
        player = FindObjectOfType<PlayerMovement>().transform;
        if(enemy.isBoss)
        {
            animator = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!enemy.isDead)
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
                if(enemy.isBoss)
                {
                    Vector2 direction = (player.transform.position - transform.position).normalized;
                    if (direction.x > 0)
                    {
                        animator.SetBool("isMoveL", false);
                    }
                    else
                    {
                        animator.SetBool("isMoveL", true);
                    }
                }
            }
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

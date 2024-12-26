using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public List<GameObject> terrainChunk;
    public GameObject player;
    public float checkerRadius;
    public LayerMask terrainMask;
    public GameObject currentChunk;
    Vector3 playerLastPosition;

    [Header("Optimization")] //Chunk del
    public List<GameObject> spawnedChunks;
    GameObject latesChunk;
    public float maxOpDist; //Must be greater than the length and width of the tilemap
    float opDist;
    float optimizerCooldown;
    public float optimizerCooldownDur;

    // Start is called before the first frame update
    void Start()
    {
        playerLastPosition = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        ChunkChecker();
        ChunkOptimizer();
    }

    void ChunkChecker()
    {
        if(!currentChunk)
        {
            return;
        }
        Vector3 moveDir = player.transform.position - playerLastPosition;
        playerLastPosition = player.transform.position;

        string directionName = GetDirectionName(moveDir);

        //Check addition adjacent directions for diagonal chunk
        if (directionName.Equals("Up") || directionName.Equals("Down"))
        {
            CheckAndSpawnChunk(directionName);
            CheckAndSpawnChunk("Left " + directionName);
            CheckAndSpawnChunk("Right " + directionName);
        }
        else if (directionName.Equals("Left") || directionName.Equals("Right"))
        {
            CheckAndSpawnChunk(directionName);
            CheckAndSpawnChunk(directionName + " Up");
            CheckAndSpawnChunk(directionName + " Down");
        }
        else
        {
            string[] subDirectionNames = directionName.Split(' ');
            CheckAndSpawnChunk(directionName);
            CheckAndSpawnChunk(subDirectionNames[0]);
            CheckAndSpawnChunk(subDirectionNames[1]);
        }
    }

    void CheckAndSpawnChunk(string direction)
    {
        if(!Physics2D.OverlapCircle(currentChunk.transform.Find(direction).position, checkerRadius, terrainMask))
        {
            SpawnChunk(currentChunk.transform.Find(direction).position);
        }
    }
    string GetDirectionName(Vector3 direction)
    {
        direction = direction.normalized;

        if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            //Moving horizontally more than vertically
            if(direction.y > 0.5f)
            {
                //Also moving upwards
                return direction.x > 0 ? "Right Up" : "Left Up";
            }
            else if(direction.y < -0.5f)
            {
                //Also moving downwards
                return direction.x > 0 ? "Right Down" : "Left Down";
            }
            else
            {   //Moving only horizontally
                return direction.x > 0 ? "Right" : "Left";
            }
        }
        else
        {
            //Moving vertically more than horizontally
            if(direction.x > 0.5f)
            {
                //Also moving right
                return direction.y > 0 ? "Right Up" : "Right Down";
            }
            else if(direction.x < -0.5f)
            {
                //Also moving left
                return direction.y > 0 ? "Left Up" : "Left Down";
            }
            else
            {
                //Moving only vertically
                return direction.y > 0 ? "Up" : "Down";
            }
        }
    }

    void SpawnChunk(Vector3 spawnPosition)
    {
        int rand = Random.Range(0, terrainChunk.Count);
        latesChunk =  Instantiate(terrainChunk[rand], spawnPosition, Quaternion.identity);
        spawnedChunks.Add(latesChunk);
    }

    void ChunkOptimizer()
    {
        optimizerCooldown -= Time.deltaTime;
        if(optimizerCooldown <= 0f)
        {
            optimizerCooldown = optimizerCooldownDur;
        }
        else
        {
            return;
        }
        foreach(GameObject chunk in spawnedChunks)
        {
            opDist = Vector3.Distance(player.transform.position, chunk.transform.position);
            if(opDist > maxOpDist)
            {
                chunk.SetActive(false);
            }
            else
            {
                chunk.SetActive(true);
            }

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public List<EnemyGroup> enemyGroups; //List of group enemy to be spawned in this wave
        public int waveQuota;   //The total number of enemies to spawn in this wave
        public float spawnInterval; //The interval at which to spawn enemies
        public int spawnCount;  //The number of enemies already spawned in this wave
    }

    [System.Serializable]
    public class EnemyGroup
    {
        public string enemyName;
        public int enemyCount;  //The number of enemies to spawn
        public int spawnCount;  //The number of enemies if this type already spawned
        public GameObject enemyPrefab;
    }

    public List<Wave> waves; //List of all wave
    public int currentWaveCount; //The index of the current wave

    [Header("Spawner Attributes")]
    float spawnTimer; //Timer use to determine when to spawn the next enemy
    public int enemiesAlive;
    public int maxEnemiesAllowed; //The maximum of enemies on map
    public bool maxEnemiesReached = false; //Flag for reach maximum of enemies
    public float waveInterval; //The interval between each wave
    bool isWaveActive = false;

    [Header("Spawn Positions")]
    public List<Transform> relativeSpawnPoints; //A list relative to spawn point of enemies

    Transform player;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerStats>().transform;
        CaculateWaveQuota();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentWaveCount < waves.Count && waves[currentWaveCount].spawnCount == 0 && !isWaveActive) //Check if the wave has ended and the next wave should start
        {
            StartCoroutine(BeginNextWave());
        }
        spawnTimer += Time.deltaTime;
        //Check if it's time for next spawn
        if(spawnTimer >= waves[currentWaveCount].spawnInterval)
        {
            spawnTimer = 0f;
            SpawnEnemies();
        }
    }

    IEnumerator BeginNextWave()
    {
        isWaveActive = true;
        //Wait for "waveInterval" seconds before starting the next wave
        yield return new WaitForSeconds(waveInterval);

        //If there are more waves to start after the current wave, move on to the next wave
        if(currentWaveCount < waves.Count - 1)
        {
            isWaveActive = false;
            currentWaveCount++;
            CaculateWaveQuota();
        }
    }
    void CaculateWaveQuota()
    {
        int currentWaveQuota = 0;
        foreach (var enemyGroup in waves[currentWaveCount].enemyGroups)
        {
            currentWaveQuota += enemyGroup.enemyCount;
            
        }

        waves[currentWaveCount].waveQuota = currentWaveQuota;
    }

    void SpawnEnemies()
    {
        //Check if the minimum number of enemies in the wave has been spawned
        if(waves[currentWaveCount].spawnCount < waves[currentWaveCount].waveQuota && !maxEnemiesReached)
        {
            //Spawnn each type of enemy untill the quota is filled
            foreach(var enemyGroup in waves[currentWaveCount].enemyGroups)
            {
                //Check if the minimum number of enemies of this type have been spawned
                if(enemyGroup.spawnCount < enemyGroup.enemyCount)
                {
                    //Spawn randomly enemy close to player position
                    Instantiate(enemyGroup.enemyPrefab, player.position + relativeSpawnPoints[Random.Range(0, relativeSpawnPoints.Count)].position,Quaternion.identity);

                    enemyGroup.spawnCount++;
                    waves[currentWaveCount].spawnCount++;
                    enemiesAlive++;

                    //Limit the number of enemies
                    if(enemiesAlive >= maxEnemiesAllowed)
                    {
                        maxEnemiesReached = true;
                        return;
                    }
                }

            }
        }
    }

    public void OnEnemyKilled()
    {
        enemiesAlive--;
        
        //Reset the maxEnemiesReached
        if(enemiesAlive < maxEnemiesAllowed)
        {
            maxEnemiesReached = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    CharacterScriptableObject characterData;

    //Current stats
    [HideInInspector]
    public float currentRecovery;
    [HideInInspector]
    public float currentMoveSpeed;
    [HideInInspector]
    public float currentMight;
    [HideInInspector]
    public float currentProjectileSpeed;
    [HideInInspector]
    public float currentHealth;
    [HideInInspector]
    public float currentMagnet;

    //Experience and level of the player
    [Header("Experience/Level")]
    public int experience = 0;
    public int level = 1;
    public int experienceCap;

    //Class for defining a level range and the corresponding experience cap increase for that range
    [System.Serializable]
    public class LevelRange
    {
        public int startLevel;
        public int endLevel;
        public int experienceCapIncrease;
    }

    //I-Frames
    [Header("I-Frame")]
    public float invincibilityDuration;
    float invincibilityTimer;
    bool isInvincible;
    public List<LevelRange> levelRanges;


    InventoryManager inventory;
    public int weaponIndex;
    public int passiveItemIndex;

    public GameObject secondWeaponTest;
    public GameObject firstPassiveItemTest, secondPassiveItemTest;
    void Awake()
    {
        characterData = CharacterSelector.GetData();
        CharacterSelector.instance.DestroySingleton();

        inventory = GetComponent<InventoryManager>();
        //Assign the variables
        currentHealth = characterData.MaxHealth;
        currentMight = characterData.Might;
        currentMoveSpeed = characterData.MoveSpeed;
        currentProjectileSpeed =  characterData.ProjectileSpeed;
        currentRecovery = characterData.Recovery;
        currentMagnet = characterData.Magnet;

        SpawnedWeapon(characterData.StartingWeapon);
        SpawnedWeapon(secondWeaponTest);
        SpawnedPassiveItem(firstPassiveItemTest);
        SpawnedPassiveItem(secondPassiveItemTest);
    }

    void Start()
    {
        //Initialize the experience cap as the first experience cap increase
        experienceCap = levelRanges[0].experienceCapIncrease;
    }

    void Update()
    {
        if(invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
        //The invincibility Timer has reached 0, set the invisibility to false
        else if(isInvincible)
        {
            isInvincible = false;
        }

        Recover();
    }
    public void IncreaseExperience(int amount)
    {
        experience += amount;

        LevelUpChecker();
    }

    void LevelUpChecker()
    {
        if(experience >= experienceCap)
        {
            level++;
            experience -= experienceCap;

            int experienceCapIncrease = 0;
            foreach(LevelRange range in levelRanges)
            {
                if(level >= range.startLevel && level <= range.endLevel)
                {
                    experienceCapIncrease = range.experienceCapIncrease;
                    break;
                }
            }
            experienceCap += experienceCapIncrease;
        }
    }

    public void TakeDamage(float dmg)
    {
        if(!isInvincible)
        {

            invincibilityTimer = invincibilityDuration;
            isInvincible = true;

            currentHealth -= dmg;
            if(currentHealth <= 0)
            {
                Kill();
            }
        }
    }

    public void Kill()
    {
        Debug.Log("Player is DEAD");
    }

    public void RestoreHealth(float amount)
    {
        //Only heal the player when current health less than max health
        if(currentHealth < characterData.MaxHealth)
        {
            currentHealth += amount;
            if(currentHealth > characterData.MaxHealth)
            {
                currentHealth = characterData.MaxHealth;
            }
        }
    }

    void Recover()
    {
        if(currentHealth < characterData.MaxHealth)
        {
            currentHealth += currentRecovery * Time.deltaTime;
            if(currentHealth > characterData.MaxHealth)
            {
                currentHealth = characterData.MaxHealth;
            }
        }
    }

    public void SpawnedWeapon(GameObject weapon)
    {
        //Check if Inventory is full
        if(weaponIndex >= inventory.weaponSlots.Count - 1)
        {
            Debug.Log("Inventory is full");
            return;
        }
        //Spawn the starting weapon
        GameObject spawnedWeapon = Instantiate(weapon, transform.position, Quaternion.identity);
        spawnedWeapon.transform.SetParent(transform); //Set weapon as a child of the player
        inventory.AddWeapon(weaponIndex, spawnedWeapon.GetComponent<WeaponController>()); //Add weapon to it's inventory slot

        weaponIndex++;
    }

    public void SpawnedPassiveItem(GameObject passiveItem)
    {
        //Check if Inventory is full
        if(passiveItemIndex >= inventory.passiveItemSlots.Count - 1)
        {
            Debug.Log("Inventory is full");
            return;
        }
        //Spawn the passvie item
        GameObject spawnedPassiveItemm = Instantiate(passiveItem, transform.position, Quaternion.identity);
        spawnedPassiveItemm.transform.SetParent(transform); //Set passive item as a child of the player
        inventory.AddPassiveItem(passiveItemIndex, spawnedPassiveItemm.GetComponent<PassiveItem>()); //Add passive item to it's inventory slot

        passiveItemIndex++;
    }
}

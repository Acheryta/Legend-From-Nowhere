using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
public class PlayerStats : MonoBehaviour
{
    CharacterData characterData;
    public CharacterData.Stats baseStats;
    [SerializeField] CharacterData.Stats actualStats;

    float health;

    #region Current Stats Properties
    public float CurrentHealth
    {
        get { return health; }
        set
        {
            //Check if the value has changed
            if(health != value)
            {
                health = value;
                if(GameManager.instance != null)
                {
                    GameManager.instance.currentHealthDisplay.text = string.Format(
                        "Health: {0}/{1}", health, actualStats.maxHealth
                    );
                }
            }
        }
    }

    public float MaxHealth
    {
        get { return actualStats.maxHealth; }
        set
        {
            //Check if the value has changed
            if(actualStats.maxHealth != value)
            {
                actualStats.maxHealth = value;
                if(GameManager.instance != null)
                {
                    GameManager.instance.currentHealthDisplay.text = string.Format(
                        "Health: {0}/{1}", health, actualStats.maxHealth
                    );
                }
            }
        }
    }

    public float CurrentRecovery
    {
        get {return Recovery;}
        set {Recovery = value;}
    }
    public float Recovery
    {
        get { return actualStats.recovery; }
        set
        {
            //Check if the value has changed
            if(actualStats.recovery != value)
            {
                actualStats.recovery = value;
                if(GameManager.instance != null)
                {
                    GameManager.instance.currentRecoveryDisplay.text = "Recovery: " + actualStats.recovery;
                }
            }
        }
    }

    public float CurrentMoveSpeed
    {
        get {return MoveSpeed;}
        set {MoveSpeed = value;}
    }
    public float MoveSpeed
    {
        get { return actualStats.moveSpeed; }
        set
        {
            //Check if the value has changed
            if(actualStats.moveSpeed != value)
            {
                actualStats.moveSpeed = value;
                if(GameManager.instance != null)
                {
                    GameManager.instance.currentMoveSpeedDisplay.text = "Move Speed: " + actualStats.moveSpeed;
                }
            }
        }
    }

    public float CurrentMight
    {
        get {return Might;}
        set {Might = value;}
    }
    public float Might
    {
        get { return actualStats.might; }
        set
        {
            //Check if the value has changed
            if(actualStats.might != value)
            {
                actualStats.might = value;
                if(GameManager.instance != null)
                {
                    GameManager.instance.currentMightDisplay.text = "Might: " + actualStats.might;
                }
            }
        }
    }

    public float CurrentProjectileSpeed
    {
        get {return Speed;}
        set {Speed = value;}
    }
    public float Speed
    {
        get { return actualStats.speed; }
        set
        {
            //Check if the value has changed
            if(actualStats.speed != value)
            {
                actualStats.speed = value;
                if(GameManager.instance != null)
                {
                    GameManager.instance.currentProjectileSpeedDisplay.text = "Projectile Speed: " + actualStats.speed;
                }
            }
        }
    }

    public float CurrentMagnet
    {
        get {return Magnet;}
        set {Magnet = value;}
    }
    public float Magnet
    {
        get { return actualStats.magnet; }
        set
        {
            //Check if the value has changed
            if(actualStats.magnet != value)
            {
                actualStats.magnet = value;
                if(GameManager.instance != null)
                {
                    GameManager.instance.currentMagnetDisplay.text = "Magnet: " + actualStats.magnet;
                }
            }
        }
    }
    #endregion

    public ParticleSystem damageEffect;
    
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

    PlayerCollector collector;
    PlayerInventory inventory;
    public int weaponIndex;
    public int passiveItemIndex;

    [Header("UI")]
    public Image healthBar;
    public Image expBar;
    public TMP_Text levelText;

    PlayerAnimator playerAnimator;
    void Awake()
    {
        characterData = CharacterSelector.GetData();

        if(CharacterSelector.instance)
            CharacterSelector.instance.DestroySingleton();

        inventory = GetComponent<PlayerInventory>();
        collector = GetComponentInChildren<PlayerCollector>();

        //Assign the variables
        baseStats = actualStats = characterData.stats;
        health = actualStats.maxHealth;
        collector.SetRadius(actualStats.magnet);

        playerAnimator = GetComponent<PlayerAnimator>();
        if(characterData.controller)
        {
            playerAnimator.SetAnimatorController(characterData.controller);
        }
    }

    void Start()
    {
        //Spawn the starting weapon
        inventory.Add(characterData.StartingWeapon);

        //Initialize the experience cap as the first experience cap increase
        experienceCap = levelRanges[0].experienceCapIncrease;

        //Set the current stats display
        GameManager.instance.currentHealthDisplay.text = "Health: " + CurrentHealth;
        GameManager.instance.currentRecoveryDisplay.text = "Recovery: " + CurrentRecovery;
        GameManager.instance.currentMoveSpeedDisplay.text = "Move Speed: " + CurrentMoveSpeed;
        GameManager.instance.currentMightDisplay.text = "Might: " + CurrentMight;
        GameManager.instance.currentProjectileSpeedDisplay.text = "Projectile Speed: " + CurrentProjectileSpeed;
        GameManager.instance.currentMagnetDisplay.text = "Magnet: " + CurrentMagnet;

        GameManager.instance.AssignChosenCharacterUI(characterData);

        UpdateHealthBar();
        UpdateExpBar();
        UpdateLevelText();
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

    public void RecalculateStats()
    {
        actualStats = baseStats;
        foreach(PlayerInventory.Slot s in inventory.passiveSlots)
        {
            Passive p = s.item as Passive;
            if(p)
            {
                actualStats += p.GetBoosts();
            }
        }
        collector.SetRadius(actualStats.magnet);
    }
    public void IncreaseExperience(int amount)
    {
        experience += amount;
        LevelUpChecker();
        UpdateExpBar();
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
            UpdateLevelText();
            GameManager.instance.StartLevelUp();
        }
    }

    void UpdateExpBar()
    {
        //Update Exp bar
        expBar.fillAmount = (float)experience / experienceCap;
    }

    void UpdateLevelText()
    {
        //Update Level text
        levelText.text = "LV " + level.ToString();
    }
    public void TakeDamage(float dmg)
    {
        if(!isInvincible)
        {

            invincibilityTimer = invincibilityDuration;
            isInvincible = true;

            CurrentHealth -= dmg;

            if(damageEffect)
            {
                Destroy(Instantiate(damageEffect, transform.position, Quaternion.identity), 5f);
            }

            if(CurrentHealth <= 0)
            {
                Kill();
            }
            UpdateHealthBar();
        }
    }

    void UpdateHealthBar()
    {
        //Update the health bar
        healthBar.fillAmount = CurrentHealth / actualStats.maxHealth;
    }
    public void Kill()
    {
        if(!GameManager.instance.isGameOver)
        {
            GameManager.instance.AssignLevelReachedUI(level);
            GameManager.instance.AssignChosenWeaponsAndPassiveItemsUI(inventory.weaponSlots, inventory.passiveSlots);
            GameManager.instance.GameOver();
        }
    }

    public void RestoreHealth(float amount)
    {
        //Only heal the player when current health less than max health
        if(CurrentHealth < actualStats.maxHealth)
        {
            CurrentHealth += amount;
            if(CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }
            UpdateHealthBar();
        }
    }

    void Recover()
    {
        if(CurrentHealth < actualStats.maxHealth)
        {
            CurrentHealth += CurrentRecovery * Time.deltaTime;
            if(CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }
            UpdateHealthBar();
        }
    }

    [System.Obsolete("Old function that is kept to maintain compatibility")]
    public void SpawnWeapon(GameObject weapon)
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
        //inventory.AddWeapon(weaponIndex, spawnedWeapon.GetComponent<WeaponController>()); //Add weapon to it's inventory slot

        weaponIndex++;
    }
    
    [System.Obsolete("No need to spawn directly now")]
    public void SpawnPassiveItem(GameObject passiveItem)
    {
        //Check if Inventory is full
        if(passiveItemIndex >= inventory.passiveSlots.Count - 1)
        {
            Debug.Log("Inventory is full");
            return;
        }
        //Spawn the passvie item
        GameObject spawnedPassiveItemm = Instantiate(passiveItem, transform.position, Quaternion.identity);
        spawnedPassiveItemm.transform.SetParent(transform); //Set passive item as a child of the player
        //inventory.AddPassiveItem(passiveItemIndex, spawnedPassiveItemm.GetComponent<PassiveItem>()); //Add passive item to it's inventory slot

        passiveItemIndex++;
    }
}

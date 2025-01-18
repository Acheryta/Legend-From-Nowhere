using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
public class PlayerStats : MonoBehaviour
{
    CharacterData characterData;
    public CharacterData.Stats baseStats;
    [SerializeField] CharacterData.Stats actualStats;

    public CharacterData.Stats Stats
    {
        get {return actualStats;}
        set {actualStats = value;}
    }
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
                UpdateHealthBar();
            }
        }
    }

    #endregion

    [Header("Visuals")]
    public ParticleSystem damageEffect;
    public ParticleSystem blockedEffect; //If armor completely block damage
    
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

        //This is just button cheat for testing and demo
        if(Input.GetKey(KeyCode.Comma)) IncreaseExperience(100);
        
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

            if(experience >= experienceCap)
                LevelUpChecker();
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
            //Take armor into account before dealing the damage
            dmg -= actualStats.armor;

            if(dmg > 0)
            {
                //Deal damage
                CurrentHealth -= dmg;
                if(damageEffect)
                {
                    Destroy(Instantiate(damageEffect, transform.position, Quaternion.identity), 5f);
                }

                if(CurrentHealth <= 0)
                {
                    Kill();
                }
            }
            else
            {
                //If blocked effect assigned
                if(blockedEffect) Destroy(Instantiate(blockedEffect, transform.position, Quaternion.identity), 5f);
            }

            invincibilityTimer = invincibilityDuration;
            isInvincible = true;
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
            CurrentHealth += Stats.recovery * Time.deltaTime;
            if(CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }
            UpdateHealthBar();
        }
    }

}

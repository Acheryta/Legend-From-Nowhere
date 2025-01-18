using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(VerticalLayoutGroup))] // Need this to make sure the button are evenly spaced out
public class UIUpgradeWindow : MonoBehaviour
{
    // Need to access the padding / spacing
    VerticalLayoutGroup verticalLayout;

    // Button and tooltip template object
    public RectTransform upgradeOptionTemplate;
    public TextMeshProUGUI tooltipTemplate;

    [Header("Settings")]
    public int maxOptions = 4; //We cannot show more option than this
    public string newText = "New!"; // The text that shows when a new upgrade is shown

    // Color the "New!" and regular text
    public Color newTextColor = Color.yellow, levelTextColor = Color.white;

    //New paths to the different UI elements in the <upgradeOptionTemplates>
    [Header("Paths")]
    public string iconPath = "Icon/Item Icon";
    public string namePath = "Name", descriptionPath = "Description", buttonPath = "Button", levelPath = "Level";

    //Used by the function to track the status
    RectTransform rectTransform;
    float optionHeight;
    int activeOptions;

    //List of all the upgrade button
    List<RectTransform> upgradeOptions = new List<RectTransform>();

    Vector2 lastScreen;

    // This is the main function that will be calling on this script.
    // 1. specify which <inventory> to add the item to, and a list of all
    //          <possibleUpgrades> to show. It will select <pick> number of upgrades and show them
    // 2 if specify a <tooltip>, then some text will appear at the bottom of the Window

    public void SetUpgrades (PlayerInventory inventory, List<ItemData> possibleUpgrades, int pick = 3, string tooltip = "")
    {
        pick = Mathf.Min(maxOptions, pick);

        // If don't have enough upgrade option boxes, create them.
        if (maxOptions > upgradeOptions.Count)
        {
            for (int i = upgradeOptions.Count; i < pick; i++)
            {
                GameObject go = Instantiate(upgradeOptionTemplate.gameObject, transform); 
                upgradeOptions.Add((RectTransform)go.transform);
            }
        }

        // If a string is provided, turn on the tooltip.
        tooltipTemplate.text = tooltip;
        tooltipTemplate.gameObject.SetActive(tooltip.Trim() != "");

        // Activate only the number of upgrade options we need, and arm the buttons and the 
        // different attributes like descriptions, etc.
        activeOptions = 0;
        int totalPossibleUpgrades = possibleUpgrades.Count; // How many upgrades have to choose from?
        foreach(RectTransform r in upgradeOptions)
        {
            if (activeOptions < pick && activeOptions < totalPossibleUpgrades)
            {
                r.gameObject.SetActive(true);

                // Select one of the possible upgrades, then remove it from the list.
                ItemData selected = possibleUpgrades[Random.Range(0, possibleUpgrades.Count)]; 
                possibleUpgrades.Remove(selected);
                Item item = inventory.Get(selected);

                // Insert the name of the item.
                TextMeshProUGUI name = r.Find(namePath).GetComponent<TextMeshProUGUI>();
                if (name)
                {
                    name.text = selected.name;
                }

                //Insert the current level of the item, or a "New!" text if it is a new weapon/passive
                TextMeshProUGUI level = r.Find(levelPath).GetComponent<TextMeshProUGUI>();
                if(level)
                {
                    if(item)
                    {
                        if(item.currentLevel >= item.maxLevel)
                        {
                            level.text = "New!";
                            level.color = newTextColor;
                        }
                        else
                        {
                            level.text = selected.GetLevelData(item.currentLevel + 1).name;
                            level.color = levelTextColor;
                        }
                    }
                    else
                    {
                        level.text = newText;
                        level.color = newTextColor;
                    }
                }

                //Insert the description of the item
                TextMeshProUGUI desc = r.Find(descriptionPath).GetComponent<TextMeshProUGUI>();
                if(desc)
                {
                    if(item)
                    {
                        desc.text = selected.GetLevelData(item.currentLevel + 1).description;
                    }
                    else
                    {
                        desc.text = selected.GetLevelData(1).description;
                    }
                }

                //Insert the icon
                Image icon = r.Find(iconPath).GetComponent<Image>();
                if(icon)
                {
                    icon.sprite = selected.icon;
                }

                //Insert the button action
                Button b = r.Find(buttonPath).GetComponent<Button>();
                if(b)
                {
                    b.onClick.RemoveAllListeners();
                    if(item)
                        b.onClick.AddListener(() => inventory.LevelUp(item));
                    else
                        b.onClick.AddListener(() => inventory.Add(selected));
                }

                activeOptions++;
            }
            else r.gameObject.SetActive(false);
        }

        //Sizes all the elements so they do not exceed the size of the box
        RecalculateLayout();
    }

    // Recalculates the height of all elements
    // this function make the window upgrade more responsively
    // whenever the size of the window changes
    void RecalculateLayout()
    {
        //Calculate the toltal for all option, then divide it by the  number of options
        optionHeight = (rectTransform.rect.height - verticalLayout.padding.top - verticalLayout.padding.bottom
                            - (maxOptions - 1) * verticalLayout.spacing);
        
        if(activeOptions == maxOptions && tooltipTemplate.gameObject.activeSelf)
        {
            optionHeight /= maxOptions + 1;
        }
        else
        {
            optionHeight /= maxOptions;
        }

        // Recalculates the height of the tooltip as well if it is currently active
        if (tooltipTemplate.gameObject.activeSelf)
        {
            RectTransform tooltipRect = (RectTransform)tooltipTemplate.transform; 
            tooltipTemplate.gameObject.SetActive(true);
            tooltipRect.sizeDelta = new Vector2(tooltipRect.sizeDelta.x, optionHeight); 
            tooltipTemplate.transform.SetAsLastSibling();
        }
        // Sets the height of every active Upgrade Option button.
        foreach (RectTransform r in upgradeOptions)
        {
            if (!r.gameObject.activeSelf) continue;
            r.sizeDelta = new Vector2(r.sizeDelta.x, optionHeight);
        }
    }
    
    void Update()
    {
        //Redraw the boxed in this element if the screen size change
        if(lastScreen.x != Screen.width || lastScreen.y != Screen.height)
        {
            RecalculateLayout();
            lastScreen = new Vector2(Screen.width, Screen.height);
        }
    }

    void Awake()
    {
        // Populates all important variable
        verticalLayout = GetComponentInChildren<VerticalLayoutGroup>();
        if(tooltipTemplate) tooltipTemplate.gameObject.SetActive(false);
        if(upgradeOptionTemplate) upgradeOptions.Add(upgradeOptionTemplate);

        rectTransform = (RectTransform)transform;
    }

    void Reset()
    {
        upgradeOptionTemplate = (RectTransform)transform.Find("Upgrade Option");
        tooltipTemplate = transform.Find("Tooltip").GetComponentInChildren<TextMeshProUGUI>();
    }
}



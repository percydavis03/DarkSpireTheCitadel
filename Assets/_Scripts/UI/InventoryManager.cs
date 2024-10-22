using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEditor.Rendering;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<Item> Items = new List<Item>();
    public int listCount;
    public AudioSource grab;
    //popup
    public GameObject popupBox;
    public TextMeshProUGUI popupText;
    public DOTweenVisualManager visualManager;
    public DOTweenAnimation anim;

    //dialogue
    public GameObject dialoguePopup;
    public bool isDialogue;
    public TextMeshProUGUI dialogueText;
    private int popupWaitTime;

    //weapons
    public Transform weaponsLocation;
    public GameObject weaponIventoryItem;

    public TextMeshProUGUI weaponItemDescription;
    public Image weaponItemImage;
    public TextMeshProUGUI weaponItemTitle;
    //anatomy
    public Transform anatomyLocation;
    public GameObject anatomyIventoryItem;

    public TextMeshProUGUI anatomyItemDescription;
    public Image anatomyItemImage;
    public TextMeshProUGUI anatomyItemTitle;
    //lore
    public Transform loreLocation;
    public GameObject loreIventoryItem;

    public TextMeshProUGUI loreItemDescription;
    public Image loreItemImage;
    public TextMeshProUGUI loreItemTitle;
    public TextMeshProUGUI loreItemSubtitle; 
    //quests
    public Transform questLocation;
    public GameObject questIventoryItem;

    public TextMeshProUGUI questItemDescription;
    public TextMeshProUGUI questItemTitle;
    public TextMeshProUGUI questItemSubtitle;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
      
    }

    public void Add(Item item) 
    {
        Items.Add(item);
        grab.Play();
    }

    public void Remove(Item item)
    {
        Items.Remove(item);
    }

    public void Popup(Item item)
    {
        popupText.text = item.itemPopup.ToString();

        popupBox.SetActive(true);
        popupWaitTime = 5;
        StartCoroutine(Wait());

        /*if (visualManager != null)
        {
            anim.enabled = true;
            visualManager.enabled = true;
            Invoke("Fade", 3);
        }*/
        
    }

    public void DialoguePopup(string dialogue)
    {
        dialogueText.text = dialogue;

        dialoguePopup.SetActive(true);
        StartCoroutine(Wait());
    }

    public void SetWait(int waitTime)
    {
        waitTime = popupWaitTime;
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(5);
        popupBox.SetActive(false);
        dialoguePopup.SetActive(false);
    }


    void Fade()
    {
        popupBox.SetActive(false);
        visualManager.enabled = false;
        anim.enabled = false;
    }
    public void ListItems()
    {
        print("elo");

        foreach (Transform item in weaponsLocation)
        {
            Destroy(item.gameObject);
        }
        foreach (Transform item in anatomyLocation)
        {
            Destroy(item.gameObject);
        }
        foreach (Transform item in loreLocation)
        {
            Destroy(item.gameObject);
        }
        foreach (Transform item in questLocation)
        {
            Destroy(item.gameObject);
        }

        foreach (var item in Items) 
        {
            if (item.itemType == 1)
            {
                GameObject obj = Instantiate(weaponIventoryItem, weaponsLocation);
              
                obj.GetComponent<ShowInfo>().thisOption = item.assignItem;

                var itemIcon = obj.transform.Find("Weapon_Icon").GetComponent<Image>();
                itemIcon.sprite = item.icon;
            }

            else if (item.itemType == 2)
            {
                GameObject obj = Instantiate(anatomyIventoryItem, anatomyLocation);
               
                obj.GetComponent<ShowInfo>().thisOption = item.assignItem;

                var itemIcon = obj.transform.Find("Anatomy_Icon").GetComponent<Image>();
                itemIcon.sprite = item.icon;
            }

            else if (item.itemType == 3)
            {
                GameObject obj = Instantiate(loreIventoryItem, loreLocation);

                obj.GetComponent<ShowInfo>().thisOption = item.assignItem;

                var itemName = obj.transform.Find("Lore_Name").GetComponent<TextMeshProUGUI>();
                itemName.text = item.itemName;
                var itemSubtitle = obj.transform.Find("Lore_Subname").GetComponent<TextMeshProUGUI>();
                itemSubtitle.text = item.subtitle;
            }

            else if (item.itemType == 4)
            {
                GameObject obj = Instantiate(questIventoryItem, questLocation);

                obj.GetComponent<ShowInfo>().thisOption = item.assignItem;

                var itemName = obj.transform.Find("Quest_Name").GetComponent<TextMeshProUGUI>();
                itemName.text = item.itemName;
                var itemSubtitle = obj.transform.Find("Quest_Subname").GetComponent<TextMeshProUGUI>();
                itemSubtitle.text = item.subtitle;
            }
            
        }

    }
}

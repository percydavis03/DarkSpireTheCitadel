using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using FMODUnity;


public class ShowInfo : MonoBehaviour
{
    public static ShowInfo Instance;
    public Item thisOption;
    //public GameObject infoLocation;
    public StudioEventEmitter buttonSound;
    //weapons
    private TextMeshProUGUI weaponDescription;
    private Image weaponRightImage;
    private TextMeshProUGUI weaponRightTitle;
    //anatomy
    private TextMeshProUGUI anatomyDescription;
    private Image anatomyRightImage;
    private TextMeshProUGUI anatomyRightTitle;
    //lore
    private TextMeshProUGUI loreDescription;
    private Image loreRightImage;
    private TextMeshProUGUI loreRightTitle;
    private TextMeshProUGUI loreSubtitle;
    //quests
    private TextMeshProUGUI questDescription;
    private TextMeshProUGUI questRightTitle;
    private TextMeshProUGUI questSubtitle;
    private void Awake()
    {
        Instance = this;

        weaponDescription = InventoryManager.Instance.weaponItemDescription;
        weaponRightImage = InventoryManager.Instance.weaponItemImage;
        weaponRightTitle = InventoryManager.Instance.weaponItemTitle;

        anatomyDescription = InventoryManager.Instance.anatomyItemDescription;
        anatomyRightImage = InventoryManager.Instance.anatomyItemImage;
        anatomyRightTitle = InventoryManager.Instance.anatomyItemTitle;

        loreDescription = InventoryManager.Instance.loreItemDescription;
        loreRightImage = InventoryManager.Instance.loreItemImage;
        loreRightTitle = InventoryManager.Instance.loreItemTitle;
        loreSubtitle = InventoryManager.Instance.loreItemSubtitle;

        questDescription = InventoryManager.Instance.questItemDescription;
        questRightTitle = InventoryManager.Instance.questItemTitle;
        questSubtitle = InventoryManager.Instance.questItemSubtitle;
    }

  
    void Start()
    {
        
        
    }
   
    public void ShowWeapon()
    {
        buttonSound.Play();
        
        weaponDescription.text = thisOption.description.ToString();
        weaponRightImage.sprite = thisOption.rightImage;
        weaponRightTitle.text = thisOption.itemName.ToString();
        
    }
    public void ShowAnatomy()
    {
        buttonSound.Play();
        
        anatomyDescription.text = thisOption.description.ToString();
        anatomyRightImage.sprite = thisOption.rightImage;
        anatomyRightTitle.text = thisOption.itemName.ToString();
        
    }

    public void ShowLore()
    {
        buttonSound.Play();
       
        loreDescription.text = thisOption.description.ToString();
        loreRightImage.sprite = thisOption.rightImage;
        loreRightTitle.text = thisOption.itemName.ToString();
        loreSubtitle.text = thisOption.subtitle.ToString();
    }

    public void ShowQuest()
    {
        buttonSound.Play();
       
        questDescription.text = thisOption.description.ToString();
        questRightTitle.text = thisOption.itemName.ToString();
        questSubtitle.text = thisOption.subtitle.ToString();
    }
    
}

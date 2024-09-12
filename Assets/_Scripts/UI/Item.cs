using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New item",menuName = "Item/Create New Item")]

public class Item : ScriptableObject 
{
    public Item assignItem;
    public int itemType;
    public string itemName;
    public string subtitle;
    public Sprite icon;
    public Sprite rightImage;
    public string description;
    public string itemPopup;

    //public int value;
    public int id;
}

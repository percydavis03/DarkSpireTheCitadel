using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    public string dialogueText;
    public int waitTime;
    public bool destroy;
    public bool notDoor;
    public GameObject doorKey;
   

    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        print("dialogue go");
        if (other.gameObject.CompareTag("Player") && notDoor)
        {
            InventoryManager.Instance.SetWait(waitTime);
            InventoryManager.Instance.DialoguePopup(dialogueText);
        }
        if (destroy)
        {
            Destroy(gameObject);
        }
        if (!notDoor)
        {
            if (!doorKey.activeInHierarchy)
            {
                InventoryManager.Instance.SetWait(waitTime);
                InventoryManager.Instance.DialoguePopup(dialogueText);
            }
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}

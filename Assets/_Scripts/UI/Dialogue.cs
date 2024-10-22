using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    public string dialogueText;
    public int waitTime;
    public bool destroy;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        print("dialogue go");
        if (other.gameObject.CompareTag("Player"))
        {
            InventoryManager.Instance.SetWait(waitTime);
            InventoryManager.Instance.DialoguePopup(dialogueText);
        }
        if (destroy)
        {
            Destroy(gameObject);
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}

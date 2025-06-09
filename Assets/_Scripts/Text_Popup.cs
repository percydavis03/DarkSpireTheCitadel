using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Text_Popup : MonoBehaviour
{
    public GameObject textBox;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }



    private void OnTriggerEnter(Collider other)
    {
        print("text");
        if (other.gameObject.CompareTag("Player"))
        {
            textBox.SetActive(true);
            StartCoroutine(Wait());
        }
        
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(3);
        textBox.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

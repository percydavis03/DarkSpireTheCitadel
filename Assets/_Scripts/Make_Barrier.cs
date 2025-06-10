using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Make_Barrier : MonoBehaviour
{
    public GameObject barrier;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.CompareTag("Player"))
        {
            barrier.SetActive(true);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}

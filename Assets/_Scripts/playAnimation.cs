using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playAnimation : MonoBehaviour
{
    public Animator animator;
    public string animBool;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        print("play");
        if (other.gameObject.CompareTag("Player"))
        {
            animator.SetBool(animBool, true);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

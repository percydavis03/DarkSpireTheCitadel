using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class simple_lock : MonoBehaviour
{
    public int index;
    public GameObject doorKey;
    public bool pit;
    public Animator anim;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && doorKey.activeInHierarchy )
        {
            if (!pit)
            {
                SceneManager.LoadScene(index);
            }
            else if (pit)
            {
                anim.Play("gate");
            }

           
        }
    }
}

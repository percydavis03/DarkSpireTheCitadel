using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelplessPerson : MonoBehaviour
{
    public float health;
    public GameObject drop;
    private bool dead;

    public List<GameObject> bloodSplats = new List<GameObject>();
    public int randomListObject;
    

    // Start is called before the first frame update
    void Start()
    {
        dead = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        print("collide");
        if (other.gameObject.CompareTag("Weapon"))
        {
            health--; 
            Bleed();
        }
    }
    void Bleed()
    {
        if (!dead)
        {
            GameObject b = Instantiate(bloodSplats[randomListObject]);
            b.transform.position = transform.position;
            b.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
            b.transform.localScale = new Vector3(1, 1, 1);
        }
    }
    public void Die()
    {
        if (health == 0)
        {
            dead = true;
           
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

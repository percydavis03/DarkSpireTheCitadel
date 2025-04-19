using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmGuy : MonoBehaviour
{
    public float health;
    public GameObject hisArm;
    public GameObject yourNewArm;
    private bool dead;
    public GameObject bloodspawn;

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
        if (other.gameObject.CompareTag("Weapon") && !dead)
        {
            health--;
            Bleed();
            print("stab");
        }
    }
    void Bleed()
    {
        if (!dead)
        {
            GameObject b = Instantiate(bloodSplats[randomListObject]);
            b.transform.position = bloodspawn.transform.position;
            b.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
            b.transform.localScale = new Vector3(1, 1, 1);
        }
    }
    public void Die()
    {
        
            dead = true;
            hisArm.SetActive(false);
            yourNewArm.SetActive(true);
        
    }
    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Die();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    public string attackTarget;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("attackTarget"))
        {
            print("attackTarget");
            if (other.gameObject.TryGetComponent(out IKnockbackable knockbackable))
            {
                
                knockbackable.GetKnockedBack(new Vector3(10,10,10));
            }
        
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

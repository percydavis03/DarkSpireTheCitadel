using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{
    public static projectile Instance;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        rb = GetComponent<Rigidbody>();
    }

    public void Fire(float speed, Vector3 direction)
    {
        rb.velocity = direction * speed;
        Invoke("OnDestroy", 5);
    }

    private void OnDestroy()
    {
        Destroy(this);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Player_Health_Manager.GetInstance().DamangePlayer(10f);
            Destroy(this);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawForwardRayCast : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        Debug.DrawRay(transform.position, forward * 10, Color.red);
    }
}

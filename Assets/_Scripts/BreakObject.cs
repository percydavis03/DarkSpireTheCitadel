using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;


public class BreakObject : MonoBehaviour
{
    public Rigidbody rb;
    public bool isBroken;
    public MeshCollider mCollider;
    public StudioEventEmitter breakSound;
    public bool theSoundOne;
    private bool isPlaying;
    // Start is called before the first frame update
    void Start()
    {
        rb.isKinematic = true;
        isPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isBroken)
        {
            rb.isKinematic = false;
            Invoke("DestroyPiece", 3);
            //mCollider.isTrigger = true;
        }
    }
    void DestroyPiece()
    {
        Destroy(this.gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Weapon")
        {
            if (theSoundOne && !isPlaying)
            {
                breakSound.Play();
                isPlaying = true;
            }
            isBroken = true;
        }
    }
}

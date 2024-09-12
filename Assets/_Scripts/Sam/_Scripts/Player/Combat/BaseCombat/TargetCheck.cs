using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
public class TargetCheck : MonoBehaviour
{
    public List<GameObject> targets = new List<GameObject>();
    public string targetName; 
    public GameObject targetTransform;
    public bool isTargeting;
    public string TargetTag;
    public float checkRadius;
    public bool lookForTarget;
    public SphereCollider targetTrigger;
    [SerializeField] private Image aimIcon;
    [SerializeField] Vector3 tagertOffset;
    public Camera mainCamera;
    public int targetCount;
    public int targetIndex;
    public void Update()
    {

        if (targetCount <= 0 && isTargeting)
        {
            print("PEnis");
            isTargeting = false;
            targets.Clear();
            targetName = null;
            targetTransform = null;
        }



        targetCount = targets.Count;
        InvokeRepeating("RemoveMissingObjects", 0f, 2f);

        if (Input.GetButton("Target") && isTargeting == false)
        {
           lookForTarget = true;
           targetTrigger.enabled = true;

        }
        
        else
        {
            targetTrigger.enabled = false;
            lookForTarget= false;

        } 
        
        if (Input.GetButtonUp("Target") && isTargeting == true)
        {
            isTargeting = false;
            targets.Clear();
            targetName = null;
            targetTransform = null;
        }

        if (aimIcon)
        {
            aimIcon.gameObject.SetActive(isTargeting);
        }



        if (isTargeting)
        {
            aimIcon.transform.position = mainCamera.WorldToScreenPoint(targetTransform.transform.position + tagertOffset);
           
        }

       


        if (lookForTarget)
        {
            
            
        
            for (int i = 0; i < targets.Count; i++)
                {
                    Vector3 targetDist = targets[i].transform.position;
                float td = Vector3.Distance(targetDist, transform.position);
                    if ((td < (checkRadius)))
                    {
                        targetName = targets[i].name;
                        targetTransform = targets[i];
                        isTargeting = true;
                        lookForTarget = false;
                        targetTrigger.enabled = false;
                    targetIndex = i;
                    }

                }
        } else
        {
            targetIndex = 0;
        }


       
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (lookForTarget && other.gameObject.tag == TargetTag)
        {
            targets.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        targets.Remove(other.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = UnityEngine.Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }

    void RemoveMissingObjects()
    {
        List<GameObject> validObjects = new List<GameObject>();

        foreach (GameObject obj in targets)
        {
            if (obj != null)
            {
                validObjects.Add(obj);
            }
            else
            {
                Debug.Log("Removed object from list");
            }
        }

       targets = validObjects;
    }

}

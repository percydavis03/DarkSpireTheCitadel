using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public List<GameObject> taggedObjects = new List<GameObject>();
    public string targetTag;

    public GameObject[] walls;
    public bool spawnEnmines;
    public GameObject[] objectstoApear;
    private void Start()
    {
        HideBarriers();
        HideObjects();
    }

    private void Update()
    {
        InvokeRepeating("RemoveMissingObjects", 0f, 2f);

        if (taggedObjects.Count == 0)
        {
            HideBarriers();
            showObjects();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag) && spawnEnmines == false)
        {
            //  PopulateTaggedObjectsList();
            taggedObjects.Add(other.gameObject);
            HideTaggedObjects();
        }

        if (other.CompareTag("Player"))
        {
            spawnEnmines = true;
            ShowTaggedObjects();
            ShowBarriers();
        }
    }

    void RemoveMissingObjects()
    {
        List<GameObject> validObjects = new List<GameObject>();

        foreach (GameObject obj in taggedObjects)
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

        taggedObjects = validObjects;
    }

    void HideTaggedObjects()
    {
        foreach (GameObject obj in taggedObjects)
        {
            // Deactivate (hide) the tagged objects
            obj.SetActive(false);
        }
    }

    void ShowTaggedObjects()
    {
        foreach (GameObject obj in taggedObjects)
        {
            // Deactivate (hide) the tagged objects
            obj.SetActive(true);
        }
    }

    void HideBarriers()
    {
        for (int i = 0; i < walls.Length; i++)
        {

            walls[i].SetActive(false);
        }


    }

    void ShowBarriers()
    {
        for (int i = 0; i < walls.Length; i++)
        {

            walls[i].SetActive(true);
        }



    }

    void HideObjects()
    {
        if (objectstoApear != null)
        {
            for (int i = 0; i < objectstoApear.Length; i++)
            {
                objectstoApear[i].SetActive(false);
            }

        }
        else
        {
            Debug.Log("No Objects to hide");
            return;
        }
    }

    void showObjects()
    {
        if (objectstoApear != null)
        {
            for (int i = 0; i < objectstoApear.Length; i++)
            {
                objectstoApear[i].SetActive(true);
            }
        }
    }
}

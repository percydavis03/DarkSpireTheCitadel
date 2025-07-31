using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ArmActivate : MonoBehaviour
{
    public static ArmActivate instance;

    public CanvasGroup deathScreen;
    public CanvasGroup newdeathScreen;
    public float fadeDuration = 2f;
    private bool isFaded;
    public StudioEventEmitter pickupSound;

    public GameObject theArm;
    public GameObject nyxWthArm;
    public GameObject oldNyx;
    public GameObject block;
    public PlayerSaveState thisGameSave;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public void AddArm()
    {
        pickupSound.Play();
        StartCoroutine(FadeCanvasGroupIn(deathScreen.alpha, 1, 3f));
    }
    private IEnumerator FadeCanvasGroupIn(float start, float end, float duration)
    {
        end = 1;
        float elapsedTime = 0.0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            deathScreen.alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            yield return null;
        }
        deathScreen.alpha = end;
        yield return new WaitForSeconds(0.5f);
        //theArm.SetActive(false);
        //nyxWthArm.SetActive(true);
        //oldNyx.SetActive(false);
        //block.SetActive(false);
        // Temporarily disable NyxUpgrades to prevent prefab switching
        NyxUpgrades nyxUpgrades = FindObjectOfType<NyxUpgrades>();
        bool wasEnabled = false;
        if (nyxUpgrades != null)
        {
            wasEnabled = nyxUpgrades.enabled;
            nyxUpgrades.enabled = false;
        }
        
        thisGameSave.hasArm = true;
        
        // Re-enable NyxUpgrades after a frame
        if (nyxUpgrades != null)
        {
            nyxUpgrades.enabled = wasEnabled;
        }
        yield return new WaitForSeconds(3f);
        StartCoroutine(FadeCanvasGroupOut(deathScreen.alpha, 0, 3f));
    }

    private IEnumerator FadeCanvasGroupOut(float start, float end, float duration)
    {
        end = 0;
        float elapsedTime = 0.0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            deathScreen.alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            yield return null;
        }
        deathScreen.alpha = end;

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

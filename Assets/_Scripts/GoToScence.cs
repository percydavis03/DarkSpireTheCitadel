using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using FMOD;
public class GoToScence : MonoBehaviour
{
    public int index;
    public CanvasGroup deathScreen;
    public float fadeDuration = 2f;
    private bool isFaded;

    private void Awake()
    {
        FadeOut();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(FadeCanvasGroupAndLeave(deathScreen.alpha, 1, fadeDuration));

        }
    }
    void FadeIn()
    {
        StartCoroutine(FadeCanvasGroup(deathScreen.alpha, 1, fadeDuration));
    }
    void FadeOut()
    {
        StartCoroutine(FadeCanvasGroup(deathScreen.alpha, 0, fadeDuration));
    }
    public void Fade()
    {

        if (isFaded)
        {
            deathScreen.DOFade(1, 0.3f);
            print("dofadein");
            StartCoroutine(FadeWait(0.5f));
        }
    }
    private IEnumerator FadeCanvasGroupAndLeave(float start, float end, float duration )
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            deathScreen.alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            yield return null;
        }
        deathScreen.alpha = end;
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(index);
    }
    private IEnumerator FadeCanvasGroup(float start, float end, float duration)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            deathScreen.alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            yield return null;
        }
        deathScreen.alpha = end;
       
    }
    IEnumerator FadeWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(index);
        //deathScreen.DOFade(0, 0.3f);
        
        print("dofadeout");
        isFaded = true;
    }
}

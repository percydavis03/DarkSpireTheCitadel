using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartBossAnims : MonoBehaviour
{
    public int index;
    public CanvasGroup blackScreen;
    public float fadeDuration = 2f;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>(); 
    }

    public void PushAway() //push player back
    {

    }

    public void ResetAttack() //back to attack stance
    {
        anim.SetBool("isAttacking", true);
        anim.SetBool("isBigAttack", false);
        anim.SetBool("wasHit", false );
    }

    public void Sheild() // no damage 
    {

    }

    public void RingOfFireOn()
    {
        Boss.instance.ringOfFire.SetActive(true);
    }

    public void RingOfFireOff()
    {
        Boss.instance.isFire = false;
        Boss.instance.ringOfFire.SetActive(false);
    }
    
    public void FireAttack()
    {
        Boss.instance.FireRingAttack();
    }

    public void AfterDeath()
    {
        StartCoroutine(FadeCanvasGroupAndLeave(blackScreen.alpha, 1, fadeDuration));
    }

    private IEnumerator FadeCanvasGroupAndLeave(float start, float end, float duration)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            blackScreen.alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            yield return null;
        }
        blackScreen.alpha = end;
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(index);
    }
}

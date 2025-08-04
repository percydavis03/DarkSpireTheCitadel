using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
public class Player_Health_Manager : MonoBehaviour
{
    public static Player_Health_Manager instance;
    [Header("Values")]
    public float maxHealth;
    public float currentHealth;
    public float maxHP;
    [Space]
    [Header("Tags")]
    public string[] hurtTags;
    [Space]
    [Header("Bools")]
    public bool isHurt;
    [Space]
    [Header("UI")]
    //public Slider healthSlider;
    public Animator anim;
    public string[] animNames;
    // fills
    public Image healthFill;
    public Image damageGlow;
    
    [Header("Health Bar Animation")]
    [Tooltip("Duration for health bar fill animation")]
    public float healthBarAnimDuration = 0.3f;
    
    [Header("Damage Glow Animation")]
    [Tooltip("Duration for damage glow opacity fade")]
    public float glowFadeDuration = 1.2f;
    [Tooltip("Delay before glow starts fading")]
    public float glowFadeDelay = 0.2f;
    [Tooltip("Maximum glow opacity when taking damage")]
    [Range(0f, 1f)]
    public float maxGlowOpacity = 0.8f;
    [Tooltip("Easing curve for glow fade")]
    public Ease glowFadeEase = Ease.OutExpo;
    
    // Private variables for smooth animation
    private float targetHealthFill;
    private Tween healthFillTween;
    private Tween damageGlowTween;

    public PlayerSaveState thisGameSave;
  

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("Found more than one Player Health Manager - destroying duplicate");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Don't override instance here - it's already set in Awake()
        // if (instance != this) { Debug.LogWarning("Found more than one Player Health Manager"); }
        // instance = this; // REMOVED - was causing singleton issues

        // Initialize health bar values
        if (thisGameSave != null)
        {
            float initialFill = thisGameSave.hitpoints / thisGameSave.maxHP;
            targetHealthFill = initialFill;
            
            if (healthFill != null) healthFill.fillAmount = initialFill;
            if (damageGlow != null) 
            {
                damageGlow.fillAmount = initialFill;
                // Initialize glow with no opacity
                var glowColor = damageGlow.color;
                glowColor.a = 0f;
                damageGlow.color = glowColor;
            }
        }
        
        //healthSlider.maxValue = maxHealth;
    }

    public static Player_Health_Manager GetInstance()
    {
        return instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (thisGameSave == null) return;
        
        currentHealth = thisGameSave.hitpoints;
        maxHP = thisGameSave.maxHP;
        
        // Calculate target fill amount
        float newTargetFill = currentHealth / maxHP;
        
        // Only animate if the target value has changed
        if (Mathf.Abs(newTargetFill - targetHealthFill) > 0.001f)
        {
            AnimateHealthBar(newTargetFill);
        }

        if (currentHealth <= 0)
        {
            // print("Dead"); // DISABLED - was causing spam
            //thisGameSave.Init();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            //StartCoroutine(DeathScreen());
        }
    }

    private void AnimateHealthBar(float newTargetFill)
    {
        // Kill any existing tweens to prevent overlapping animations
        healthFillTween?.Kill();
        damageGlowTween?.Kill();
        
        float previousHealthFill = targetHealthFill;
        targetHealthFill = newTargetFill;
        
        // Always animate the main health bar smoothly
        if (healthFill != null)
        {
            healthFillTween = healthFill.DOFillAmount(targetHealthFill, healthBarAnimDuration)
                .SetEase(Ease.OutCubic);
        }
        
        // Handle damage glow effect
        if (damageGlow != null)
        {
            if (newTargetFill < previousHealthFill) // Taking damage
            {
                // Update glow fill to match health immediately, then trigger opacity glow effect
                damageGlow.fillAmount = newTargetFill;
                
                // Flash the glow with opacity, then fade it out
                damageGlowTween = DOTween.Sequence()
                    .Append(damageGlow.DOFade(maxGlowOpacity, 0.1f)) // Quick flash to max opacity
                    .AppendInterval(glowFadeDelay) // Hold the glow
                    .Append(damageGlow.DOFade(0f, glowFadeDuration).SetEase(glowFadeEase)); // Fade out with custom easing
            }
            else // Healing
            {
                // Update glow fill to match health and ensure no glow opacity
                damageGlow.fillAmount = newTargetFill;
                damageGlowTween = damageGlow.DOFade(0f, 0.2f); // Quick fade out any existing glow
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up tweens when object is destroyed
        healthFillTween?.Kill();
        damageGlowTween?.Kill();
    }
    
    /// <summary>
    /// Manually trigger a damage glow effect (useful for testing or special effects)
    /// </summary>
    public void TriggerDamageGlow()
    {
        if (damageGlow != null)
        {
            damageGlowTween?.Kill();
            damageGlowTween = DOTween.Sequence()
                .Append(damageGlow.DOFade(maxGlowOpacity, 0.1f))
                .AppendInterval(glowFadeDelay)
                .Append(damageGlow.DOFade(0f, glowFadeDuration).SetEase(glowFadeEase));
        }
    }

    private IEnumerator DeathScreen()
    {
        anim.SetBool(animNames[0], true);
        Player_Movement.instance.canMove = false;
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

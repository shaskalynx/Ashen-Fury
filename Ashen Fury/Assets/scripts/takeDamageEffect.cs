using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class takeDamageEffect : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private HealthSystem healthSystem;
    private float lastFrameHealth;
    public Volume postProcessingVolume;
    private Vignette _vignette;
    private float currentIntensity;

    void Start()
    {
        healthSystem = player.GetComponent<HealthSystem>();
        lastFrameHealth = healthSystem.health;
        postProcessingVolume.profile.TryGet(out _vignette);

        _vignette.intensity.Override(0f);
    }

    void Update()
    {
        if(healthSystem.health < lastFrameHealth)
        {
            StartCoroutine(TakeDamageEffectCoroutine()); 
            Debug.Log("Damage effect triggered");
        }
        lastFrameHealth = healthSystem.health;
    }

    private IEnumerator TakeDamageEffectCoroutine()
    {
        currentIntensity = 0.3f;
        _vignette.intensity.Override(currentIntensity);
        _vignette.active = true;
        yield return new WaitForSeconds(0.4f);
        
        Debug.Log("Starting fade out"); 
        while (currentIntensity > 0)
        {
            currentIntensity -= Time.deltaTime * 0.5f;
            _vignette.intensity.Override(currentIntensity);
            yield return null;
        }
        _vignette.active = false;
        Debug.Log("Damage effect completed");
    }
}
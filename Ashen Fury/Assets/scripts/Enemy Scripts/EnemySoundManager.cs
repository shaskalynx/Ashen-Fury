using UnityEngine;

public class EnemySoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource combatSource;    // For combat sounds
    [SerializeField] private AudioSource movementSource;  // For movement sounds
    [SerializeField] private AudioSource vocalizationSource; // For enemy vocalizations

    [Header("Combat Sounds")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;

    [Header("Movement Sounds")]
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip alertedSound;      // When enemy spots player

    [Header("Vocalization Sounds")]
    [SerializeField] private AudioClip[] idleSounds;      // Random sounds when idle
    [SerializeField] private AudioClip[] aggroSounds;     // Sounds when chasing player
    
    [Header("Sound Settings")]
    [SerializeField] private float minTimeBetweenVocalizations = 5f;
    [SerializeField] private float maxTimeBetweenVocalizations = 15f;
    [SerializeField] private float vocalizationVolume = 0.7f;
    
    private float nextVocalizationTime;
    private bool isAggro = false;

    private void Start()
    {
        // Create audio sources if not assigned
        if (combatSource == null)
        {
            combatSource = gameObject.AddComponent<AudioSource>();
            combatSource.playOnAwake = false;
        }

        if (movementSource == null)
        {
            movementSource = gameObject.AddComponent<AudioSource>();
            movementSource.playOnAwake = false;
        }
        
        if (vocalizationSource == null)
        {
            vocalizationSource = gameObject.AddComponent<AudioSource>();
            vocalizationSource.playOnAwake = false;
            vocalizationSource.volume = vocalizationVolume;
        }
        
        // Set initial vocalization time
        SetNextVocalizationTime();
    }
    
    private void Update()
    {
        // Handle random vocalizations
        if (Time.time >= nextVocalizationTime && (idleSounds.Length > 0 || aggroSounds.Length > 0))
        {
            PlayRandomVocalization();
            SetNextVocalizationTime();
        }
    }
    
    private void SetNextVocalizationTime()
    {
        nextVocalizationTime = Time.time + Random.Range(minTimeBetweenVocalizations, maxTimeBetweenVocalizations);
    }
    
    private void PlayRandomVocalization()
    {
        AudioClip[] soundArray = isAggro ? aggroSounds : idleSounds;
        
        if (soundArray.Length > 0)
        {
            AudioClip randomSound = soundArray[Random.Range(0, soundArray.Length)];
            if (randomSound != null)
            {
                vocalizationSource.PlayOneShot(randomSound);
            }
        }
    }

    public void PlayAttack()
    {
        if (attackSound != null)
        {
            combatSource.PlayOneShot(attackSound);
        }
    }

    public void PlayDamage()
    {
        if (damageSound != null)
        {
            combatSource.PlayOneShot(damageSound);
        }
    }

    public void PlayDeath()
    {
        if (deathSound != null)
        {
            combatSource.PlayOneShot(deathSound);
        }
    }

    public void PlayFootstep()
    {
        if (footstepSound != null)
        {
            movementSource.PlayOneShot(footstepSound);
        }
    }
    
    public void PlayAlerted()
    {
        if (alertedSound != null)
        {
            movementSource.PlayOneShot(alertedSound);
        }
    }
    
    public void SetAggroState(bool aggro)
    {
        isAggro = aggro;
        
        // Play alerted sound when first becoming aggro
        if (aggro && alertedSound != null)
        {
            PlayAlerted();
        }
    }

    // Stop all sounds (useful when enemy dies)
    public void StopAllSounds()
    {
        combatSource.Stop();
        movementSource.Stop();
        vocalizationSource.Stop();
    }
}
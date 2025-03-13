using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource combatSource;    // For combat sounds
    [SerializeField] private AudioSource movementSource;  // For movement sounds

    [Header("Combat Sounds")]
    [SerializeField] private AudioClip attackSound1;
    [SerializeField] private AudioClip attackSound2;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip swordSheathSound;
    [SerializeField] private AudioClip swordUnsheathSound;
    [SerializeField] private AudioClip deathSound;

    [Header("Movement Sounds")]
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip jumpSound;

    private void Start()
    {
        // Create combat audio source if not assigned
        if (combatSource == null)
        {
            combatSource = gameObject.AddComponent<AudioSource>();
            combatSource.playOnAwake = false;
        }

        // Create movement audio source if not assigned
        if (movementSource == null)
        {
            movementSource = gameObject.AddComponent<AudioSource>();
            movementSource.playOnAwake = false;
        }
    }

    public void PlayAttack1()
    {
        if (attackSound1 != null)
        {
            combatSource.PlayOneShot(attackSound1);
        }
    }

    public void PlayAttack2()
    {
        if (attackSound2 != null)
        {
            combatSource.PlayOneShot(attackSound2);
        }
    }

    public void PlayDamage()
    {
        if (damageSound != null)
        {
            combatSource.PlayOneShot(damageSound);
        }
    }

    public void PlaySwordSheath()
    {
        if (swordSheathSound != null)
        {
            combatSource.PlayOneShot(swordSheathSound);
        }
    }

    public void PlaySwordUnsheath()
    {
        if (swordUnsheathSound != null)
        {
            combatSource.PlayOneShot(swordUnsheathSound);
        }
    }

    public void PlayJump()
    {
        if (jumpSound != null)
        {
            movementSource.PlayOneShot(jumpSound);
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

    // Optional: Stop all sounds (useful when player dies)
    public void StopAllSounds()
    {
        combatSource.Stop();
        movementSource.Stop();
    }
}
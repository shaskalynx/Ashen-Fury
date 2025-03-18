using UnityEngine;
using System.Collections;

public class potionPickup : MonoBehaviour
{
    private GameObject player;
    private GameObject weapon;
    private float playerHealth;
    private float playerMaxHealth;
    public float healAmount = 10; // Amount of health restored by the heal potion
    public float buffDuration = 10; // Duration of the power-up effect
    public float buffAmount = 20; // Amount of damage increase during the power-up
    public float respawnTime = 5; // Time before the potion respawns after being picked up
    [SerializeField] private potionTypes type;
    private UIController uiController;

    private GameObject coroutineManager; // Separate GameObject to manage the coroutine

    enum potionTypes
    {
        Heal,
        PowerUp,
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerMaxHealth = player.GetComponent<HealthSystem>().health;
        uiController = FindObjectOfType<UIController>();

        // Create a separate GameObject to manage the coroutine
        coroutineManager = new GameObject("CoroutineManager");
        DontDestroyOnLoad(coroutineManager); // Ensure it persists across scene changes
    }

    private void Update()
    {
        playerHealth = player.GetComponent<HealthSystem>().health;
        weapon = player.GetComponent<EquipmentSystem>().currentWeaponInHand;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            Debug.Log("Potion picked up by player.");
            if (type == potionTypes.Heal)
            {
                heal();
            }
            else if (type == potionTypes.PowerUp)
            {
                powerUp();
            }
        }
        else
        {
            return;
        }
    }

    private void heal()
    {
        if (playerHealth < playerMaxHealth)
        {
            player.GetComponent<HealthSystem>().Heal(healAmount);
            if (uiController != null)
            {
                uiController.ShowPickupMessage($"Health restored: +{healAmount}");
            }
            StartRespawnCoroutine(); // Start the respawn coroutine
            gameObject.SetActive(false); // Disable the potion instead of destroying it
        }
        else
        {
            if (uiController != null)
            {
                uiController.ShowPickupMessage("Health is already full!");
            }
            return;
        }
    }

    private void powerUp()
    {
        if (weapon != null) // Check if the player has a weapon equipped
        {
            DamageDealer damageDealer = weapon.GetComponentInChildren<DamageDealer>();
            if (damageDealer != null)
            {
                StartCoroutine(buffUp(damageDealer));
                if (uiController != null)
                {
                    uiController.ShowPickupMessage($"Power Up! Damage increased by {buffAmount} for {buffDuration} seconds");
                }
                StartRespawnCoroutine(); // Start the respawn coroutine
                gameObject.SetActive(false); // Disable the potion instead of destroying it
            }
            else
            {
                if (uiController != null)
                {
                    uiController.ShowPickupMessage("No weapon with DamageDealer found!");
                }
            }
        }
        else
        {
            if (uiController != null)
            {
                uiController.ShowPickupMessage("No weapon equipped!");
            }
        }
    }

    private IEnumerator buffUp(DamageDealer damageDealer)
    {
        float originalDamage = damageDealer.weaponDamage; // Store the original damage
        damageDealer.weaponDamage = buffAmount; // Apply the buff
        yield return new WaitForSeconds(buffDuration);
        damageDealer.weaponDamage = originalDamage; // Revert to the original damage
    }

    private void StartRespawnCoroutine()
    {
        // Start the coroutine on the separate GameObject
        coroutineManager.AddComponent<CoroutineHelper>().StartRespawnCoroutine(this, respawnTime);
    }

    public void Respawn()
    {
        gameObject.SetActive(true); // Re-enable the potion
        Debug.Log("Potion re-enabled.");
    }
}

// Helper class to manage the coroutine
public class CoroutineHelper : MonoBehaviour
{
    public void StartRespawnCoroutine(potionPickup potion, float respawnTime)
    {
        StartCoroutine(RespawnPotion(potion, respawnTime));
    }

    private IEnumerator RespawnPotion(potionPickup potion, float respawnTime)
    {
        Debug.Log("RespawnPotion coroutine started. Waiting for respawn time: " + respawnTime + " seconds.");
        yield return new WaitForSeconds(respawnTime);
        potion.Respawn(); // Call the Respawn method on the potion
    }
}
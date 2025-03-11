using UnityEngine;
using System.Collections;

public class healPickup : MonoBehaviour
{
    private GameObject player;
    private GameObject weapon;
    private float playerHealth;
    private float playerMaxHealth;
    public float healAmount = 10;
    public float buffDuration = 10;
    [SerializeField] private potionTypes type;
    private UIController uiController;

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
    }

    private void Update() {
        playerHealth = player.GetComponent<HealthSystem>().health;
        weapon = player.GetComponent<EquipmentSystem>().currentWeaponInHand;
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject == player){
            if(type == potionTypes.Heal){
                heal();
            }
            else if(type == potionTypes.PowerUp){
                powerUp();
            }
        }else{
            return;
        }
    }

    private void heal(){
        //Debug.Log("the player should be healed" + "    Player max health is " + playerMaxHealth);
        if(playerHealth < playerMaxHealth){
            player.GetComponent<HealthSystem>().Heal(healAmount);
            if (uiController != null)
            {
                uiController.ShowPickupMessage($"Health restored: +{healAmount}");
            }
            Destroy(gameObject);
            //Debug.Log("Player healed for " + healAmount + " health");
            //Debug.Log("Player health is now " + player.GetComponent<HealthSystem>().health);
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

    private void powerUp(){
        StartCoroutine(buffUp());
        if (uiController != null)
        {
            uiController.ShowPickupMessage($"Power Up! Damage increased for {buffDuration} seconds");
        }
        Destroy(gameObject);
    }

    private IEnumerator buffUp(){
        weapon.GetComponentInChildren<DamageDealer>().weaponDamage = 20;
        yield return new WaitForSeconds(buffDuration);
        weapon.GetComponentInChildren<DamageDealer>().weaponDamage = 10;
    }
}
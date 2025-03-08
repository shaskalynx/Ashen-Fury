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
            player.GetComponent<HealthSystem>().health += healAmount;

            if(player.GetComponent<HealthSystem>().health > playerMaxHealth){
                player.GetComponent<HealthSystem>().health = playerMaxHealth;
            }
            Destroy(gameObject);
            //Debug.Log("Player healed for " + healAmount + " health");
            //Debug.Log("Player health is now " + player.GetComponent<HealthSystem>().health);
        }else{
            return;
        }
    }

    private void powerUp(){
        StartCoroutine(buffUp());
        Destroy(gameObject);
    }

    private IEnumerator buffUp(){
        weapon.GetComponentInChildren<DamageDealer>().weaponDamage = 20;
        yield return new WaitForSeconds(buffDuration);
        weapon.GetComponentInChildren<DamageDealer>().weaponDamage = 10;
    }
}
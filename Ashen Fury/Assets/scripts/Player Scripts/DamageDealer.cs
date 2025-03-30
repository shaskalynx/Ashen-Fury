using System.Collections.Generic;
using UnityEngine;
 
public class DamageDealer : MonoBehaviour
{
    public bool canDealDamage;
    List<GameObject> hasDealtDamage;

    [SerializeField] float weaponLength;
    [SerializeField] public float weaponDamage;

    [Header("Camera Shake Properties")]
    [SerializeField] cameraShake _cameraShake;
    [SerializeField] float shakeIntensity = 1f;
    [SerializeField] float shakeTime = 0.2f;

    void Start()
    {
        canDealDamage = false;
        hasDealtDamage = new List<GameObject>();
        _cameraShake = FindObjectOfType<cameraShake>();
    }
 
    void Update()
    {
        if (canDealDamage)
        {
            RaycastHit hit;
 
            int layerMask = 1 << 9;
            if (Physics.Raycast(transform.position, -transform.up, out hit, weaponLength, layerMask))
            {
                if (hit.transform.TryGetComponent(out enemyHealthSystem enemy) && !hasDealtDamage.Contains(hit.transform.gameObject))
                {
                    enemy.TakeDamage(weaponDamage);
                    enemy.HitVFX(hit.point);
                    //Debug.Log("Damaged the enemy");
                    hasDealtDamage.Add(hit.transform.gameObject);
                }
                _cameraShake.ShakeCamera(shakeIntensity, shakeTime);
            }
        }
    }
    public void StartDealDamage()
    {
        canDealDamage = true;
        hasDealtDamage.Clear();
    }
    public void EndDealDamage()
    {
        canDealDamage = false;
    }
 
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * weaponLength);
    }
}
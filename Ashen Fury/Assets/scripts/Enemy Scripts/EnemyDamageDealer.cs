using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class EnemyDamageDealer : MonoBehaviour
{
    bool canDealDamage;
    bool hasDealtDamage;
    int layerMask;
 
    [SerializeField] float weaponLength;
    [SerializeField] float weaponDamage;

    void Start()
    {
        canDealDamage = false;
        hasDealtDamage = false;
        layerMask = 1 << 6;
    }
 
    // Update is called once per frame
    void Update()
    {
        if (canDealDamage && !hasDealtDamage)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, -transform.up, out hit, weaponLength, layerMask))
            {
                if (hit.transform.TryGetComponent(out HealthSystem health))
                {
                    print("it damaged you");
                    health.TakeDamage(weaponDamage);
                    health.HitVFX(hit.point);
                    hasDealtDamage = true;
                }
            }
        }
    }
    public void StartDealDamage()
    {
        print("the enemy should damage you");
        canDealDamage = true;
        hasDealtDamage = false;
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
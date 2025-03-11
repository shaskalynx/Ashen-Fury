using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TutorialStageHandler : MonoBehaviour
{
    [SerializeField] private bool destroyOnTriggerEnter;
    [SerializeField] private string tagFiller;
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private UnityEvent<TMP_Text>  onTriggerEnter;
    [SerializeField] private UnityEvent<TMP_Text>  onTriggerExit;

    void OnTriggerEnter(Collider other)
    {
        if(!String.IsNullOrEmpty(tagFiller) && !other.CompareTag(tagFiller)) return;
        onTriggerEnter?.Invoke(targetText);
        
        if(destroyOnTriggerEnter){
            Destroy(gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(!String.IsNullOrEmpty(tagFiller) && !other.CompareTag(tagFiller)) return;
        onTriggerExit?.Invoke(targetText);
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class EnemyStateUI : MonoBehaviour
{
    [SerializeField] private enemy targetEnemy;
    [SerializeField] private GameObject stateEntryPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private Color highlightColor = Color.yellow; // Add this field
    private Color defaultColor = Color.white; // Add this field
    
    private Dictionary<string, TextMeshProUGUI> stateTexts = new Dictionary<string, TextMeshProUGUI>();

    private void Awake()
    {
        if (targetEnemy == null)
        {
            targetEnemy = FindObjectOfType<enemy>();
            if (targetEnemy == null)
            {
                Debug.LogError("No enemy found in scene! Please assign an enemy in inspector.");
                return;
            }
        }

        if (stateEntryPrefab == null)
        {
            Debug.LogError("State entry prefab is not assigned!");
            return;
        }

        if (contentParent == null)
        {
            Debug.LogError("Content parent transform is not assigned!");
            return;
        }
    }

    private void Start()
    {
        // Wait one frame to ensure enemy is initialized
        StartCoroutine(DelayedInitialization());
    }

    private System.Collections.IEnumerator DelayedInitialization()
    {
        yield return new WaitForSeconds(0.2f); // Wait for enemy to initialize
        InitializeStateUI();
    }

    private void InitializeStateUI()
    {
        if (targetEnemy == null)
        {
            Debug.LogError("Target enemy is null!");
            return;
        }

        if (targetEnemy.wolves == null)
        {
            Debug.LogError($"Wolves list is null on enemy {targetEnemy.name}!");
            return;
        }

        Debug.Log($"Initializing UI for {targetEnemy.wolves.Count} states on enemy {targetEnemy.name}");
        
        // Clear existing entries
        stateTexts.Clear();
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var node in targetEnemy.wolves)
        {
            if (node == null)
            {
                Debug.LogError("Found null node in wolves list!");
                continue;
            }

            GameObject entry = Instantiate(stateEntryPrefab, contentParent);
            TextMeshProUGUI text = entry.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"{node.name}: 0.00";
                stateTexts[node.name] = text;
                Debug.Log($"Created UI entry for state: {node.name}");
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component not found on prefab!");
            }
        }
    }

    private void Update()
    {
        var weightedBehaviors = targetEnemy.gwo.GetWeightedBehaviors();
        string currentState = targetEnemy.stateName;  // Use enemy's stateName directly

        foreach (var behavior in weightedBehaviors)
        {
            if (stateTexts.TryGetValue(behavior.node.name, out TextMeshProUGUI text))
            {
                text.text = $"{behavior.node.name}: {behavior.weight:F2}";
                
                // Highlight current state using enemy's stateName
                text.color = behavior.node.name == currentState ? highlightColor : defaultColor;
            }
        }
    }
}
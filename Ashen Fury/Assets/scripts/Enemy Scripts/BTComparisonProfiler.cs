using UnityEngine;
using Unity.Profiling;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

public class BTComparisonProfiler : MonoBehaviour
{
    private ProfilerRecorder totalMemoryRecorder;
    private ProfilerRecorder gcMemoryRecorder;
    private StringBuilder statsText;

    private enemy gwoEnemy;
    private SimplifiedEnemyAI btEnemy;

    private Dictionary<string, float> memoryUsage = new Dictionary<string, float>();
    private Dictionary<string, float> executionTimes = new Dictionary<string, float>();
    private Dictionary<string, float> effectiveness = new Dictionary<string, float>();
    private Dictionary<string, float> totalEffectiveness = new Dictionary<string, float>();
    private Dictionary<string, int> measurementCount = new Dictionary<string, int>();
    private const int MAX_MEASUREMENTS = 100;

    private Stopwatch stopwatch;
    private float updateInterval = 0.5f;
    private float nextUpdate = 0.0f;

    // GUI styling
    private GUIStyle guiStyle;
    private Rect windowRect = new Rect(20, 340, 500, 350); // Increased width and height
    private bool showWindow = true;

    void OnEnable()
    {
        totalMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
        gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        statsText = new StringBuilder(500);
        stopwatch = new Stopwatch();

        gwoEnemy = Object.FindAnyObjectByType<enemy>();
        btEnemy = Object.FindAnyObjectByType<SimplifiedEnemyAI>();

        executionTimes["BT+GWO"] = 0f;
        executionTimes["BT"] = 0f;
        effectiveness["BT+GWO"] = 0f;
        effectiveness["BT"] = 0f;
        totalEffectiveness["BT+GWO"] = 0f;
        totalEffectiveness["BT"] = 0f;
        measurementCount["BT+GWO"] = 0;
        measurementCount["BT"] = 0;

        guiStyle = new GUIStyle();
        guiStyle.normal.textColor = Color.white;
        guiStyle.fontSize = 11;
        guiStyle.padding = new RectOffset(5, 5, 2, 2);
        guiStyle.wordWrap = true;
        guiStyle.richText = true;
        guiStyle.alignment = TextAnchor.UpperLeft;
    }

    void OnDisable()
    {
        totalMemoryRecorder.Dispose();
        gcMemoryRecorder.Dispose();
    }

    void Update()
    {
        if (Time.time >= nextUpdate)
        {
            nextUpdate = Time.time + updateInterval;
            UpdateStats();
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            showWindow = !showWindow;
        }
    }

    void UpdateStats()
    {
        statsText.Clear();
        statsText.AppendLine("\n=== BT vs BT+GWO Comparison ===\n");
        statsText.AppendLine($"Total Memory: {totalMemoryRecorder.LastValue / (1024 * 1024):F2} MB");
        statsText.AppendLine($"GC Memory: {gcMemoryRecorder.LastValue / (1024 * 1024):F2} MB");
        statsText.AppendLine("\n-------------------------\n");

        // BT+GWO
        if (gwoEnemy != null && measurementCount["BT+GWO"] < MAX_MEASUREMENTS)
        {
            memoryUsage["BT+GWO"] = CalculateInstanceMemory(gwoEnemy);
            MeasureExecutionTime(gwoEnemy, "BT+GWO");
            MeasureEffectiveness(gwoEnemy, "BT+GWO");
        }
        // BT
        if (btEnemy != null && measurementCount["BT"] < MAX_MEASUREMENTS)
        {
            memoryUsage["BT"] = CalculateInstanceMemory(btEnemy);
            MeasureExecutionTime(btEnemy, "BT");
            MeasureEffectiveness(btEnemy, "BT");
        }

        foreach (var algorithm in memoryUsage.Keys)
        {
            statsText.AppendLine($"\n{algorithm}:");
            statsText.AppendLine($"  Memory: {memoryUsage[algorithm]:F2} KB");
            statsText.AppendLine($"  Execution: {executionTimes[algorithm]:F3} ms");
            statsText.AppendLine($"  Effectiveness: {effectiveness[algorithm]:F3}");
            statsText.AppendLine($"  Measurements: {measurementCount[algorithm]}/{MAX_MEASUREMENTS}");
        }

        statsText.AppendLine("\n-------------------------");
        statsText.AppendLine("Press ` (backquote) to toggle window");
    }

    private float CalculateInstanceMemory(MonoBehaviour instance)
    {
        if (instance == null) return 0f;
        float memory = 0;
        if (instance is enemy gwo)
        {
            memory += gwo.GetMemoryUsage();
            memory += gwo.GetComponent<UnityEngine.AI.NavMeshAgent>().path.corners.Length * sizeof(float) * 3;
        }
        else if (instance is SimplifiedEnemyAI bt)
        {
            memory += sizeof(float) * 5; // aggro, attack, cooldowns, etc.
            memory += sizeof(float) * 3; // state vector
            memory += 32; // estimate for string currentBehavior
            memory += bt.GetComponent<UnityEngine.AI.NavMeshAgent>().path.corners.Length * sizeof(float) * 3;
        }
        memory += sizeof(float) * 3;
        memory += sizeof(float) * 2;
        return memory / 1024f;
    }

    private void MeasureExecutionTime(MonoBehaviour instance, string algorithmName)
    {
        if (measurementCount[algorithmName] >= MAX_MEASUREMENTS) return;

        const int ITERATIONS = 100;
        float totalTime = 0;

        for (int i = 0; i < ITERATIONS; i++)
        {
            stopwatch.Reset();
            stopwatch.Start();

            if (instance is enemy gwo)
            {
                Vector3 state = GetCurrentState(gwo);
                gwo.gwo.Optimize(10, state);
            }
            else if (instance is SimplifiedEnemyAI bt)
            {
                Vector3 state = GetCurrentState(bt);
                // Simulate one BT decision cycle
                foreach (var behavior in bt.GetBehaviors())
                {
                    if (behavior.Evaluate(state) != SimplifiedNode.NodeState.fail)
                        break;
                }
            }

            stopwatch.Stop();
            totalTime += stopwatch.ElapsedTicks / (float)System.TimeSpan.TicksPerMillisecond;
        }

        executionTimes[algorithmName] = totalTime / ITERATIONS;
    }

    private void MeasureEffectiveness(MonoBehaviour instance, string algorithmName)
    {
        if (measurementCount[algorithmName] >= MAX_MEASUREMENTS) return;

        const int SAMPLES = 100;
        float totalScore = 0f;

        for (int i = 0; i < SAMPLES; i++)
        {
            Vector3 state = GetCurrentState(instance);
            float score = 0f;

            if (instance is enemy gwo)
            {
                var behaviors = gwo.gwo.GetWeightedBehaviors();
                if (behaviors != null && behaviors.Count > 0)
                {
                    score = EvaluateDecisionQuality(behaviors[0].node, state);
                }
            }
            else if (instance is SimplifiedEnemyAI bt)
            {
                var behaviors = bt.GetBehaviors();
                if (behaviors != null && behaviors.Count > 0)
                {
                    score = EvaluateDecisionQuality(behaviors[0], state);
                }
            }

            totalScore += score;
        }

        float currentEffectiveness = totalScore / SAMPLES;
        totalEffectiveness[algorithmName] += currentEffectiveness;
        measurementCount[algorithmName]++;
        effectiveness[algorithmName] = totalEffectiveness[algorithmName] / measurementCount[algorithmName];
    }

    private float EvaluateDecisionQuality(object selectedBehavior, Vector3 state)
    {
        float score = 0f;
        float distanceToPlayer = state.x;
        float healthPercentage = state.y;

        string behaviorName = selectedBehavior is Node n ? n.name :
                              selectedBehavior is SimplifiedNode sn ? sn.name : "";

        if (behaviorName.Contains("Attack"))
        {
            if (distanceToPlayer <= 2f && healthPercentage > 0.5f)
                score = 1.0f;
            else if (distanceToPlayer <= 2f)
                score = 0.5f;
            else
                score = 0.2f;
        }
        else if (behaviorName.Contains("Chase"))
        {
            if (distanceToPlayer > 2f && distanceToPlayer < 10f && healthPercentage > 0.3f)
                score = 1.0f;
            else if (distanceToPlayer < 10f)
                score = 0.6f;
            else
                score = 0.3f;
        }
        else if (behaviorName.Contains("Patrol"))
        {
            if (distanceToPlayer > 10f || healthPercentage < 0.3f)
                score = 1.0f;
            else if (healthPercentage < 0.5f)
                score = 0.7f;
            else
                score = 0.4f;
        }
        return score;
    }

    private Vector3 GetCurrentState(MonoBehaviour enemy)
    {
        GameObject player = GameObject.FindWithTag("Player");
        float distanceToPlayer = player ?
            Vector3.Distance(enemy.transform.position, player.transform.position) :
            float.MaxValue;

        float healthPercentage = 1.0f;
        if (enemy is enemy gwo)
        {
            var healthSystem = gwo.GetComponent<enemyHealthSystem>();
            if (healthSystem != null)
                healthPercentage = healthSystem.health / 100f;
        }
        else if (enemy is SimplifiedEnemyAI bt)
        {
            var healthSystem = bt.GetComponent<enemyHealthSystem>();
            if (healthSystem != null)
                healthPercentage = healthSystem.health / 100f;
        }

        float distanceToPatrolPoint = 0f;
        if (enemy is enemy gwo2)
        {
            distanceToPatrolPoint = Vector3.Distance(
                enemy.transform.position,
                gwo2.target ? gwo2.target.position : enemy.transform.position
            );
        }
        else if (enemy is SimplifiedEnemyAI bt2)
        {
            distanceToPatrolPoint = Vector3.Distance(
                enemy.transform.position,
                bt2.target ? bt2.target.position : enemy.transform.position
            );
        }

        return new Vector3(distanceToPlayer, healthPercentage, distanceToPatrolPoint);
    }

    void OnGUI()
    {
        if (!showWindow) return;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.window);
        titleStyle.normal.textColor = Color.white;
        titleStyle.fontSize = 11;
        titleStyle.alignment = TextAnchor.UpperCenter;

        windowRect = GUI.Window(2, windowRect, (id) =>
        {
            GUILayout.BeginVertical();
            GUILayout.Label(statsText.ToString(), guiStyle);
            GUILayout.EndVertical();
            GUI.DragWindow();
        }, "BT vs BT+GWO Profiler", titleStyle);

        windowRect.x = Mathf.Clamp(windowRect.x, 0, Screen.width - windowRect.width);
        windowRect.y = Mathf.Clamp(windowRect.y, 0, Screen.height - windowRect.height);
    }
}

// Extension for SimplifiedEnemyAI to expose behaviors for profiling
public static class SimplifiedEnemyAIProfilerExt
{
    public static List<SimplifiedNode> GetBehaviors(this SimplifiedEnemyAI ai)
    {
        var field = typeof(SimplifiedEnemyAI).GetField("behaviors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(ai) as List<SimplifiedNode>;
    }
}

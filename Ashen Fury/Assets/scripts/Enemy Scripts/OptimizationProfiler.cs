using UnityEngine;
using System.Collections.Generic;
using Unity.Profiling;
using System.Text;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class OptimizationProfiler : MonoBehaviour
{
    private ProfilerRecorder totalMemoryRecorder;
    private ProfilerRecorder gcMemoryRecorder;
    private StringBuilder statsText;
    
    private enemy gwoEnemy;
    private enemyPSO psoEnemy;
    private enemyFA faEnemy;
    
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
    private Rect windowRect = new Rect(20, 20, 300, 300);
    private bool showWindow = true;
    private Color backgroundColor = new Color(0, 0, 0, 0.8f);
    private bool isResizing = false;
    private Rect resizeHandle = new Rect(0, 0, 15, 15);

    private bool showSensitivityWindow = false;
    private Rect sensitivityWindowRect = new Rect(340, 20, 400, 300);
    private StringBuilder sensitivityResults = new StringBuilder();
    private bool analysisComplete = false;

    private struct Criterion
    {
        public string name;
        public float importance;
        public float threshold;
        public bool isMaxCriterion;
    }

    void OnEnable()
    {
        totalMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
        gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        statsText = new StringBuilder(500);
        stopwatch = new Stopwatch();
        
        gwoEnemy = Object.FindAnyObjectByType<enemy>();
        psoEnemy = Object.FindAnyObjectByType<enemyPSO>();
        faEnemy = Object.FindAnyObjectByType<enemyFA>();

        // Initialize dictionaries
        executionTimes["Grey Wolf Optimizer"] = 0f;
        executionTimes["Particle Swarm"] = 0f;
        executionTimes["Firefly Algorithm"] = 0f;
        
        effectiveness["Grey Wolf Optimizer"] = 0f;
        effectiveness["Particle Swarm"] = 0f;
        effectiveness["Firefly Algorithm"] = 0f;
        
        totalEffectiveness["Grey Wolf Optimizer"] = 0f;
        totalEffectiveness["Particle Swarm"] = 0f;
        totalEffectiveness["Firefly Algorithm"] = 0f;
        
        measurementCount["Grey Wolf Optimizer"] = 0;
        measurementCount["Particle Swarm"] = 0;
        measurementCount["Firefly Algorithm"] = 0;

        // Initialize GUI style
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

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showWindow = !showWindow;
        }
    }

    void UpdateStats()
    {
        statsText.Clear();
        statsText.AppendLine("\n=== Performance Analysis ===\n");
        statsText.AppendLine($"Total Memory: {totalMemoryRecorder.LastValue / (1024 * 1024):F2} MB");
        statsText.AppendLine($"GC Memory: {gcMemoryRecorder.LastValue / (1024 * 1024):F2} MB");
        statsText.AppendLine("\n-------------------------\n");
        
        // Calculate metrics for each algorithm only if measurements are not complete
        if (gwoEnemy != null && measurementCount["Grey Wolf Optimizer"] < MAX_MEASUREMENTS)
        {
            memoryUsage["Grey Wolf Optimizer"] = CalculateInstanceMemory(gwoEnemy);
            MeasureExecutionTime(gwoEnemy, "Grey Wolf Optimizer");
            MeasureEffectiveness(gwoEnemy, "Grey Wolf Optimizer");
        }
        if (psoEnemy != null && measurementCount["Particle Swarm"] < MAX_MEASUREMENTS)
        {
            memoryUsage["Particle Swarm"] = CalculateInstanceMemory(psoEnemy);
            MeasureExecutionTime(psoEnemy, "Particle Swarm");
            MeasureEffectiveness(psoEnemy, "Particle Swarm");
        }
        if (faEnemy != null && measurementCount["Firefly Algorithm"] < MAX_MEASUREMENTS)
        {
            memoryUsage["Firefly Algorithm"] = CalculateInstanceMemory(faEnemy);
            MeasureExecutionTime(faEnemy, "Firefly Algorithm");
            MeasureEffectiveness(faEnemy, "Firefly Algorithm");
        }
            
        // Display metrics with better formatting
        foreach (var algorithm in memoryUsage.Keys)
        {
            statsText.AppendLine($"\n{algorithm}:");
            statsText.AppendLine($"  Memory: {memoryUsage[algorithm]:F2} KB");
            statsText.AppendLine($"  Execution: {executionTimes[algorithm]:F3} ms");
            statsText.AppendLine($"  Effectiveness: {effectiveness[algorithm]:F3}");
            statsText.AppendLine($"  Measurements: {measurementCount[algorithm]}/{MAX_MEASUREMENTS}");
        }
        
        statsText.AppendLine("\n-------------------------");
        statsText.AppendLine("Press TAB to toggle window");
    }
    
    private float CalculateInstanceMemory(MonoBehaviour instance)
    {
        if (instance == null) return 0f;

        float memory = 0;
        
        if (instance is enemy gwo)
        {
            memory += sizeof(float) * 3;
            memory += gwo.GetComponent<UnityEngine.AI.NavMeshAgent>().path.corners.Length * sizeof(float) * 3;
            memory += gwo.GetMemoryUsage();
        }
        else if (instance is enemyPSO pso)
        {
            memory += sizeof(float) * 4;
            memory += sizeof(float) * 3;
            memory += pso.GetMemoryUsage();
        }
        else if (instance is enemyFA fa)
        {
            memory += sizeof(float) * 3;
            memory += sizeof(float) * 3;
            memory += fa.GetMemoryUsage();
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
            else if (instance is enemyPSO pso)
            {
                Vector3 state = GetCurrentState(pso);
                pso.pso.Optimize(10, state);
            }
            else if (instance is enemyFA fa)
            {
                Vector3 state = GetCurrentState(fa);
                fa.fa.Optimize(10, state);
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
            else if (instance is enemyPSO pso)
            {
                var behaviors = pso.pso.GetWeightedBehaviors();
                if (behaviors != null && behaviors.Count > 0)
                {
                    score = EvaluateDecisionQuality(behaviors[0].node, state);
                }
            }
            else if (instance is enemyFA fa)
            {
                var behaviors = fa.fa.GetWeightedBehaviors();
                if (behaviors != null && behaviors.Count > 0)
                {
                    score = EvaluateDecisionQuality(behaviors[0].node, state);
                }
            }
            
            totalScore += score;
        }
        
        float currentEffectiveness = totalScore / SAMPLES;
        
        totalEffectiveness[algorithmName] += currentEffectiveness;
        measurementCount[algorithmName]++;
        effectiveness[algorithmName] = totalEffectiveness[algorithmName] / measurementCount[algorithmName];
    }

    private float EvaluateDecisionQuality(Node selectedBehavior, Vector3 state)
    {
        float score = 0f;
        float distanceToPlayer = state.x;
        float healthPercentage = state.y;
        
        if (selectedBehavior is AttackPlayer)
        {
            // Good choice if close to player and healthy
            if (distanceToPlayer <= 2f && healthPercentage > 0.5f)
                score = 1.0f;
            else if (distanceToPlayer <= 2f)
                score = 0.5f;
            else
                score = 0.2f;
        }
        else if (selectedBehavior is ChasePlayer)
        {
            // Good choice if medium distance and reasonable health
            if (distanceToPlayer > 2f && distanceToPlayer < 10f && healthPercentage > 0.3f)
                score = 1.0f;
            else if (distanceToPlayer < 10f)
                score = 0.6f;
            else
                score = 0.3f;
        }
        else if (selectedBehavior is Patrol)
        {
            // Good choice if far from player or low health
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
        // Use enemyHealthSystem to get health information
        var healthSystem = gwo.GetComponent<enemyHealthSystem>();
        if (healthSystem != null)
        {
            healthPercentage = healthSystem.health / 100f; // Assuming max health is 100
        }
    }
    else if (enemy is enemyPSO pso)
    {
        // Use enemyHealthSystem to get health information
        var healthSystem = pso.GetComponent<enemyHealthSystem>();
        if (healthSystem != null)
        {
            healthPercentage = healthSystem.health / 100f; // Assuming max health is 100
        }
    }
    else if (enemy is enemyFA fa)
    {
        // Use enemyHealthSystem to get health information
        var healthSystem = fa.GetComponent<enemyHealthSystem>();
        if (healthSystem != null)
        {
            healthPercentage = healthSystem.health / 100f; // Assuming max health is 100
        }
    }

    float distanceToPatrolPoint = Vector3.Distance(
        enemy.transform.position, 
        enemy.GetComponent<UnityEngine.AI.NavMeshAgent>().destination
    );

    return new Vector3(distanceToPlayer, healthPercentage, distanceToPatrolPoint);
}

    void OnGUI()
    {
        if (!showWindow) return;

        // Create a style for the window title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.window);
        titleStyle.normal.textColor = Color.white;
        titleStyle.fontSize = 11;
        titleStyle.alignment = TextAnchor.UpperCenter;

        // Draw the window with a dark background
        windowRect = GUI.Window(0, windowRect, (id) =>
        {
            // Draw the content
            GUILayout.BeginVertical();
            GUILayout.Label(statsText.ToString(), guiStyle);
            GUILayout.EndVertical();

            // Draw resize handle in bottom-right corner
            resizeHandle.x = windowRect.width - resizeHandle.width;
            resizeHandle.y = windowRect.height - resizeHandle.height;
            GUI.Box(resizeHandle, "↘");

            // Handle resizing
            Event e = Event.current;
            if (e.type == EventType.MouseDown && resizeHandle.Contains(e.mousePosition))
            {
                isResizing = true;
                e.Use(); // Consume the event
            }
            else if (e.type == EventType.MouseUp)
            {
                isResizing = false;
                e.Use(); // Consume the event
            }
            else if (e.type == EventType.MouseDrag && isResizing)
            {
                // Only resize when dragging while the mouse is held down
                windowRect.width = Mathf.Max(200, e.mousePosition.x + 5);
                windowRect.height = Mathf.Max(200, e.mousePosition.y + 5);
                GUI.changed = true;
                e.Use(); // Consume the event
            }

            // Make window draggable
            GUI.DragWindow();
        }, 
        "Algorithm Performance Profiler", 
        titleStyle);

        // Keep window within screen bounds
        windowRect.x = Mathf.Clamp(windowRect.x, 0, Screen.width - windowRect.width);
        windowRect.y = Mathf.Clamp(windowRect.y, 0, Screen.height - windowRect.height);

        // Add this at the end of OnGUI
        if (showWindow)
        {
            if (GUI.Button(new Rect(windowRect.x, windowRect.y + windowRect.height + 5, 150, 25), "Run Sensitivity Analysis"))
            {
                RunSensitivityAnalysis();
            }
        }

        if (showSensitivityWindow && analysisComplete)
        {
            sensitivityWindowRect = GUI.Window(1, sensitivityWindowRect, (id) =>
            {
                GUILayout.BeginVertical();
                GUILayout.Label(sensitivityResults.ToString(), guiStyle);
                GUILayout.EndVertical();

                if (GUI.Button(new Rect(5, sensitivityWindowRect.height - 30, 100, 25), "Close"))
                {
                    showSensitivityWindow = false;
                }

                GUI.DragWindow();
            }, "Sensitivity Analysis Results", titleStyle);

            // Keep sensitivity window within screen bounds
            sensitivityWindowRect.x = Mathf.Clamp(sensitivityWindowRect.x, 0, Screen.width - sensitivityWindowRect.width);
            sensitivityWindowRect.y = Mathf.Clamp(sensitivityWindowRect.y, 0, Screen.height - sensitivityWindowRect.height);
        }
    }

    // Add this method after your existing methods
    private void RunSensitivityAnalysis()
{
    var criteria = new List<Criterion>
    {
        new Criterion { 
            name = "Speed MIN (ms)", 
            importance = 0.25f,        // 5% importance
            threshold = 100f,          // baseline for normalization
            isMaxCriterion = false     // lower is better
        },
        new Criterion { 
            name = "Effectiveness MAX", 
            importance = 0.5f,        // 90% importance
            threshold = 1.0f,          // effectiveness is already 0-1
            isMaxCriterion = true      // higher is better
        },
        new Criterion { 
            name = "Memory MIN (MiB)", 
            importance = 0.25f,        // 5% importance
            threshold = 500f,          // baseline for normalization
            isMaxCriterion = false     // lower is better
        }
    };

    sensitivityResults.Clear();
    sensitivityResults.AppendLine("Sensitivity Analysis Results\n");
    sensitivityResults.AppendLine("| Constraints | Criterion's Importance | GWO | PSO | FA |");
    sensitivityResults.AppendLine("| ----------- | --------------------- | --- | --- | --- |");

    var algorithms = new[] { "Grey Wolf Optimizer", "Particle Swarm", "Firefly Algorithm" };
    var finalScores = new Dictionary<string, float>();

    foreach (var criterion in criteria)
    {
        sensitivityResults.Append($"| {criterion.name} | {criterion.importance:F2} |");

        foreach (var algorithm in algorithms)
        {
            float rawValue = GetMeasuredValue(algorithm, criterion.name);
            float score = NormalizeToScale(rawValue, criterion);
            float weightedScore = score * criterion.importance;

            if (!finalScores.ContainsKey(algorithm))
                finalScores[algorithm] = 0;
            finalScores[algorithm] += weightedScore;

            sensitivityResults.Append($" {score:F2} |");
        }
        sensitivityResults.AppendLine();
    }

    // Add final weighted scores
    sensitivityResults.AppendLine("| Final Score | - |");
    foreach (var algorithm in algorithms)
    {
        sensitivityResults.Append($" {finalScores[algorithm]:F2} |");
    }
    
    analysisComplete = true;
    showSensitivityWindow = true;
}

private float NormalizeToScale(float value, Criterion criterion)
{
    if (criterion.isMaxCriterion)
    {
        // For maximization criteria (like effectiveness)
        // Higher raw values get higher scores (1-10)
        return Mathf.Lerp(1f, 10f, value / criterion.threshold);
    }
    else
    {
        // For minimization criteria (like speed and memory)
        // Lower raw values get higher scores (1-10)
        return Mathf.Lerp(10f, 1f, value / criterion.threshold);
    }
}

    private float GetMeasuredValue(string algorithm, string criterionName)
    {
        switch (criterionName)
        {
            case "Speed MIN (ms)":
                return executionTimes[algorithm];
            case "Effectiveness MAX":
                return effectiveness[algorithm];
            case "Memory MIN (MiB)":
                return memoryUsage[algorithm] / 1024f;
            default:
                return 0f;
        }
    }

    private float NormalizeScore(float value, Criterion criterion)
    {
        if (criterion.isMaxCriterion)
        {
            return Mathf.Lerp(1f, 10f, value / criterion.threshold);
        }
        else
        {
            return Mathf.Lerp(10f, 1f, value / criterion.threshold);
        }
    }
}
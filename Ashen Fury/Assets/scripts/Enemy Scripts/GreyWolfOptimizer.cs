using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GreyWolfOptimizer
{
    private List<Node> wolves;
    private Node alpha, beta, delta;
    private float a = 2f;
    private const float MIN_POSITION = 0.1f;
    private Vector3 currentState;
    
    // Cached values
    private float cachedAdaptiveA;
    private float cachedHealthFactor;
    private float cachedDistanceFactor;

    public struct WeightedBehavior
    {
        public Node node;
        public float weight;
    }

    public GreyWolfOptimizer(List<Node> nodes)
    {
        if (nodes == null || nodes.Count < 3)
        {
            Debug.LogError("GWO requires at least 3 nodes!");
            return;
        }

        this.wolves = nodes;
        
        // Initialize positions
        foreach (var wolf in wolves)
        {
            wolf.position = Random.Range(MIN_POSITION, 1f);
        }

        // Initial hierarchy setup
        UpdateWolfHierarchy();
    }

    private void UpdateWolfHierarchy()
    {
        if (wolves == null || wolves.Count < 3) return;

        // Calculate fitness for all wolves once
        foreach (var wolf in wolves)
        {
            wolf.CalculateFitness(currentState);
            // Apply position modifier to fitness
            wolf.fitness *= (0.5f + wolf.position * 0.5f);
        }
        
        // Sort and assign hierarchy
        wolves.Sort((a, b) => b.fitness.CompareTo(a.fitness));
        alpha = wolves[0];
        beta = wolves[1];
        delta = wolves[2];
    }

    public void Optimize(int iterations, Vector3 state)
    {
        if (wolves == null || wolves.Count < 3) return;

        this.currentState = state;
        UpdateCachedValues();
        
        for (int i = 0; i < iterations; i++)
        {
            // Linear decrease of a
            a = 2f * (1f - (float)i / iterations);
            cachedAdaptiveA = a * (1f + (cachedHealthFactor + cachedDistanceFactor));
            
            UpdatePositions();
            UpdateWolfHierarchy();
        }
    }

    private void UpdateCachedValues()
    {
        cachedHealthFactor = 1.0f - currentState.y;
        cachedDistanceFactor = Mathf.Abs(currentState.x - 5f) / 5f;
    }

    private void UpdatePositions()
    {
        if (alpha == null || beta == null || delta == null) return;

        float meanLeaderPosition = (alpha.position + beta.position + delta.position) / 3f;
        float r = Random.value; // Single random value for all calculations
        float A = 2f * cachedAdaptiveA * r - cachedAdaptiveA;

        foreach (var wolf in wolves)
        {
            float newPosition = CalculateNewPosition(wolf, meanLeaderPosition, A);
            wolf.position = Mathf.Clamp(newPosition, MIN_POSITION, 1f);
        }
    }

    private float CalculateNewPosition(Node wolf, float meanLeaderPosition, float A)
    {
        // Simplified distance calculation
        float D = Mathf.Max(0.1f, Mathf.Abs(meanLeaderPosition - wolf.position));
        float X = meanLeaderPosition - A * D;
        
        // Small random movement to prevent stagnation
        return X + Random.Range(-0.05f, 0.05f);
    }

    public List<WeightedBehavior> GetWeightedBehaviors()
    {
        if (wolves == null || wolves.Count == 0)
        {
            return new List<WeightedBehavior>();
        }

        float totalFitness = wolves.Sum(w => w.fitness);
        
        if (totalFitness <= float.Epsilon)
        {
            return wolves.Select(w => new WeightedBehavior 
            { 
                node = w,
                weight = 1.0f / wolves.Count
            })
            .OrderByDescending(wb => wb.weight)
            .ToList();
        }
        
        return wolves.Select(w => new WeightedBehavior 
        { 
            node = w,
            weight = w.fitness / totalFitness
        })
        .OrderByDescending(wb => wb.weight)
        .ToList();
    }
}
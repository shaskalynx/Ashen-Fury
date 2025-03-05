using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FireflyOptimizer
{
    private List<Node> fireflies;
    private float[] intensity;
    private float[] attractiveness;
    private float beta0 = 1.0f;
    private float gamma = 1.0f;
    private float alpha = 0.2f;
    private int populationSize;
    private Vector3 currentState;
    
    // Additional arrays for intensive computation
    private float[] lightAbsorption;
    private float[] movementHistory;
    private float[] energyLevels;

    public struct WeightedBehavior
    {
        public Node node;
        public float weight;
        public float brightness;
    }

    public FireflyOptimizer(List<Node> nodes)
    {
        this.fireflies = nodes;
        this.populationSize = nodes.Count;
        intensity = new float[populationSize];
        attractiveness = new float[populationSize];
        lightAbsorption = new float[populationSize];
        movementHistory = new float[populationSize];
        energyLevels = new float[populationSize];
        
        for (int i = 0; i < populationSize; i++)
        {
            fireflies[i].position = Random.value;
            lightAbsorption[i] = Random.value;
            energyLevels[i] = 1f;
            movementHistory[i] = 0f;
        }
    }

    public void Optimize(int maxGeneration, Vector3 state)
    {
        currentState = state;
        
        // Intensive computation loop
        for (int t = 0; t < maxGeneration; t++)
        {
            UpdateLightIntensity(state);
            
            for (int i = 0; i < populationSize; i++)
            {
                for (int j = 0; j < populationSize; j++)
                {
                    if (intensity[j] > intensity[i])
                    {
                        // Intensive attractiveness calculation
                        float r = Mathf.Abs(fireflies[i].position - fireflies[j].position);
                        float absorptionFactor = Mathf.Exp(-lightAbsorption[i] * r);
                        attractiveness[i] = beta0 * absorptionFactor * energyLevels[i];
                        
                        MoveFirefly(i, j);
                        
                        // Update energy levels with intensive computation
                        energyLevels[i] *= Mathf.Exp(-0.1f * Mathf.Abs(movementHistory[i]));
                    }
                }
            }

            // Intensive parameter updates
            UpdateParameters(t, maxGeneration);
        }

        RankFireflies();
    }

    private void UpdateLightIntensity(Vector3 state)
    {
        float[] tempIntensity = new float[populationSize];
        
        for (int i = 0; i < populationSize; i++)
        {
            fireflies[i].CalculateFitness(state);
            
            // Intensive intensity calculation
            float positionFactor = Mathf.Sqrt(fireflies[i].position);
            float energyFactor = Mathf.Pow(energyLevels[i], 2);
            float absorptionFactor = 1f / (1f + lightAbsorption[i]);
            
            tempIntensity[i] = fireflies[i].fitness * 
                              positionFactor * 
                              energyFactor * 
                              absorptionFactor;
        }
        
        // Update intensities
        for (int i = 0; i < populationSize; i++)
        {
            intensity[i] = tempIntensity[i];
        }
    }

    private void MoveFirefly(int i, int j)
    {
        // Store previous position for history
        float previousPosition = fireflies[i].position;
        
        // Intensive movement calculation
        float r = Mathf.Abs(fireflies[i].position - fireflies[j].position);
        float absorptionEffect = Mathf.Exp(-gamma * r * r * lightAbsorption[i]);
        float energyEffect = Mathf.Sqrt(energyLevels[i]);
        
        float movement = attractiveness[i] * (fireflies[j].position - fireflies[i].position) * absorptionEffect;
        float randomization = alpha * (Random.value - 0.5f) * energyEffect;
        
        float newPosition = fireflies[i].position + movement + randomization;
        fireflies[i].position = Mathf.Clamp01(newPosition);
        
        // Update movement history with intensive computation
        movementHistory[i] = Mathf.Lerp(movementHistory[i], 
            Mathf.Abs(fireflies[i].position - previousPosition),
            0.3f);
    }

    private void UpdateParameters(int generation, int maxGeneration)
    {
        float progress = (float)generation / maxGeneration;
        
        // Intensive parameter updates
        alpha *= Mathf.Exp(-0.1f * progress);
        gamma = Mathf.Lerp(gamma, 2f, progress * 0.1f);
        
        for (int i = 0; i < populationSize; i++)
        {
            lightAbsorption[i] = Mathf.Lerp(lightAbsorption[i],
                Mathf.Clamp01(lightAbsorption[i] + (Random.value - 0.5f) * 0.1f),
                0.2f);
            
            energyLevels[i] = Mathf.Clamp01(energyLevels[i] + 
                (1f - progress) * (Random.value - 0.5f) * 0.1f);
        }
    }

    private void RankFireflies()
    {
        // Intensive ranking with multiple criteria
        for (int i = 0; i < populationSize - 1; i++)
        {
            for (int j = 0; j < populationSize - i - 1; j++)
            {
                float score1 = intensity[j] * energyLevels[j] * (1f - lightAbsorption[j]);
                float score2 = intensity[j + 1] * energyLevels[j + 1] * (1f - lightAbsorption[j + 1]);
                
                if (score1 < score2)
                {
                    SwapFireflies(j, j + 1);
                }
            }
        }
    }

    private void SwapFireflies(int i, int j)
    {
        (fireflies[i], fireflies[j]) = (fireflies[j], fireflies[i]);
        (intensity[i], intensity[j]) = (intensity[j], intensity[i]);
        (attractiveness[i], attractiveness[j]) = (attractiveness[j], attractiveness[i]);
        (lightAbsorption[i], lightAbsorption[j]) = (lightAbsorption[j], lightAbsorption[i]);
        (movementHistory[i], movementHistory[j]) = (movementHistory[j], movementHistory[i]);
        (energyLevels[i], energyLevels[j]) = (energyLevels[j], energyLevels[i]);
    }

    public List<WeightedBehavior> GetWeightedBehaviors()
    {
        // Intensive weight calculation
        float[] weights = new float[populationSize];
        float totalWeight = 0f;
        
        for(int i = 0; i < populationSize; i++)
        {
            weights[i] = intensity[i] * 
                        energyLevels[i] * 
                        (1f - lightAbsorption[i]) * 
                        (1f + Mathf.Abs(movementHistory[i]));
            totalWeight += weights[i];
        }
        
        return fireflies.Select((f, i) => new WeightedBehavior 
        { 
            node = f,
            weight = weights[i] / totalWeight,
            brightness = intensity[i]
        })
        .OrderByDescending(wb => wb.weight)
        .ToList();
    }
}
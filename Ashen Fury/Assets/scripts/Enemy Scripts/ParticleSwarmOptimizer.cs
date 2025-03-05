using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ParticleSwarmOptimizer
{
    private List<Node> particles;
    private float[] personalBestPositions;
    private float[] personalBestFitness;
    private float globalBestPosition;
    private float globalBestFitness;
    private Vector3 currentState;
    
    private float baseInertia = 0.729f;
    private float cognitive = 1.49f;
    private float social = 1.49f;
    private float[] velocities;
    
    // Additional arrays for intensive computation
    private float[] momentumFactors;
    private float[] velocityHistory;

    public struct WeightedBehavior
    {
        public Node node;
        public float weight;
        public float velocity;
    }

    public ParticleSwarmOptimizer(List<Node> nodes)
    {
        this.particles = nodes;
        int numParticles = nodes.Count;
        
        personalBestPositions = new float[numParticles];
        personalBestFitness = new float[numParticles];
        velocities = new float[numParticles];
        momentumFactors = new float[numParticles];
        velocityHistory = new float[numParticles];

        for (int i = 0; i < numParticles; i++)
        {
            particles[i].position = Random.value;
            personalBestPositions[i] = particles[i].position;
            personalBestFitness[i] = float.MinValue;
            velocities[i] = Random.Range(-0.5f, 0.5f);
            momentumFactors[i] = Random.value;
            velocityHistory[i] = 0f;
        }

        globalBestFitness = float.MinValue;
    }

    public void Optimize(int iterations, Vector3 state)
    {
        currentState = state;
        
        for (int i = 0; i < iterations; i++)
        {
            // Intensive computation loop
            for(int j = 0; j < 5; j++)
            {
                UpdateFitness();
                UpdateVelocitiesAndPositions();
            }
        }
    }

    private void UpdateFitness()
    {
        float[] tempFitness = new float[particles.Count];
        
        for (int i = 0; i < particles.Count; i++)
        {
            Node particle = particles[i];
            particle.CalculateFitness(currentState);
            
            // Intensive fitness calculation
            float velocityInfluence = Mathf.Exp(-Mathf.Abs(velocities[i]));
            float momentumInfluence = Mathf.Sqrt(momentumFactors[i]);
            float historicalInfluence = 1f + Mathf.Abs(velocityHistory[i]) * 0.1f;
            
            tempFitness[i] = particle.fitness * velocityInfluence * momentumInfluence * historicalInfluence;
            
            if (tempFitness[i] > personalBestFitness[i])
            {
                personalBestFitness[i] = tempFitness[i];
                personalBestPositions[i] = particle.position;
            }

            if (tempFitness[i] > globalBestFitness)
            {
                globalBestFitness = tempFitness[i];
                globalBestPosition = particle.position;
            }
        }
        
        // Update particle fitness with computed values
        for (int i = 0; i < particles.Count; i++)
        {
            particles[i].fitness = tempFitness[i];
        }
    }

    private void UpdateVelocitiesAndPositions()
    {
        float inertia = CalculateAdaptiveInertia();
        
        for (int i = 0; i < particles.Count; i++)
        {
            // Store previous velocity for history
            velocityHistory[i] = velocities[i];
            
            // Intensive random calculations
            float r1 = Random.value;
            float r2 = Random.value;
            float r3 = Random.value;

            // Update momentum factor with intensive computation
            momentumFactors[i] = Mathf.Clamp01(
                momentumFactors[i] + 
                (r3 - 0.5f) * 0.1f + 
                Mathf.Sin(particles[i].position * Mathf.PI) * 0.05f
            );

            // Complex velocity update
            velocities[i] = inertia * velocities[i] * (1f + momentumFactors[i]) +
                           cognitive * r1 * (personalBestPositions[i] - particles[i].position) +
                           social * r2 * (globalBestPosition - particles[i].position);

            // Intensive boundary calculations
            float maxVelocity = Mathf.Lerp(0.5f, 1f, momentumFactors[i]);
            velocities[i] = Mathf.Clamp(velocities[i], -maxVelocity, maxVelocity);
            
            // Position update with intensive computation
            float newPosition = particles[i].position + 
                              velocities[i] * momentumFactors[i] + 
                              velocityHistory[i] * 0.1f;
            
            particles[i].position = Mathf.Clamp01(newPosition);
        }
    }

    private float CalculateAdaptiveInertia()
    {
        // Intensive inertia calculation
        float healthPercentage = currentState.y;
        float distanceToPlayer = currentState.x;
        
        float healthFactor = Mathf.Pow(1.0f - healthPercentage, 2);
        float distanceFactor = Mathf.Exp(-Mathf.Abs(distanceToPlayer - 5f) / 5f);
        
        return baseInertia * (0.5f + (healthFactor + distanceFactor) * 0.25f);
    }

    public List<WeightedBehavior> GetWeightedBehaviors()
    {
        // Intensive weight calculation
        float[] weights = new float[particles.Count];
        float totalWeight = 0f;
        
        for(int i = 0; i < particles.Count; i++)
        {
            weights[i] = particles[i].fitness * 
                        (1.0f + particles[i].position) * 
                        (1.0f + Mathf.Abs(velocities[i])) *
                        momentumFactors[i];
            totalWeight += weights[i];
        }
        
        return particles.Select((p, i) => new WeightedBehavior 
        { 
            node = p,
            weight = weights[i] / totalWeight,
            velocity = velocities[i]
        })
        .OrderByDescending(wb => wb.weight)
        .ToList();
    }
}
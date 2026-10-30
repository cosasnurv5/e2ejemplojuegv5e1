using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class BoidSensor : MonoBehaviour
    {
        public List<BoidAgent> GetNeighbors(float radius)
        {
            List<BoidAgent> neighbors = new List<BoidAgent>();
            float sqrRadius = radius * radius;

            
            foreach (var agent in BoidAgent.allBoids)
            {
                if (agent != null && agent != this && !agent.IsDestroyed)
                {
                    float sqrDist = (transform.position - agent.transform.position).sqrMagnitude;
                    if (sqrDist < sqrRadius)
                    {
                        neighbors.Add(agent);
                    }
                }
            }
            return neighbors;
        }

        public Transform GetNearestHunter(float radius)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            foreach (var col in colliders)
            {
                if (col.TryGetComponent<Hunter.HunterNPC>(out var hunter))
                {
                    return hunter.transform;
                }
            }
            return null;
        }

        public Transform GetNearestInterestObject(float radius)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            Transform nearest = null;
            float minDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                if (col.TryGetComponent<Environment.InterestObject>(out _))
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        nearest = col.transform;
                    }
                }
            }
            return nearest;
        }
    }
}
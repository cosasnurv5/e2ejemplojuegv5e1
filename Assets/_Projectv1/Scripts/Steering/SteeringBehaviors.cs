using UnityEngine;

namespace Steering
{
    public static class SteeringBehaviors
    {
        public static Vector3 Seek(SteeringAgent agent, Vector3 targetPosition)
        {
            Vector3 desired = targetPosition - agent.transform.position;
            desired.y = 0f; 

            if (desired.sqrMagnitude < 0.001f) return Vector3.zero;

            Vector3 desiredVelocity = desired.normalized * agent.maxSpeed;
            return desiredVelocity - agent.Velocity;
        }

        public static Vector3 Flee(SteeringAgent agent, Vector3 targetPosition)
        {
            Vector3 desired = agent.transform.position - targetPosition;
            desired.y = 0f;

            if (desired.sqrMagnitude < 0.001f) return agent.maxSpeed * agent.transform.forward;

            Vector3 desiredVelocity = desired.normalized * agent.maxSpeed;
            return desiredVelocity - agent.Velocity;
        }

        public static Vector3 Arrive(SteeringAgent agent, Vector3 targetPosition, float slowingRadius)
        {
            Vector3 desired = targetPosition - agent.transform.position;
            desired.y = 0f;
            float distance = desired.magnitude;

            if (distance < 0.001f) return -agent.Velocity;

            Vector3 desiredVelocity;
            if (distance < slowingRadius)
            {
                desiredVelocity = desired.normalized * agent.maxSpeed * (distance / slowingRadius);
            }
            else
            {
                desiredVelocity = desired.normalized * agent.maxSpeed;
            }

            return desiredVelocity - agent.Velocity;
        }

        public static Vector3 Evade(SteeringAgent agent, SteeringAgent pursuerAgent)
        {
            Vector3 distance = pursuerAgent.transform.position - agent.transform.position;
            float updatesEsti = distance.magnitude / Mathf.Max(agent.maxSpeed, 0.1f);
            Vector3 estiPosition = pursuerAgent.transform.position + pursuerAgent.Velocity * updatesEsti;

            return Flee(agent, estiPosition);
        }

        public static Vector3 Pursuit(SteeringAgent agent, SteeringAgent targetAgent)
        {
            Vector3 distance = targetAgent.transform.position - agent.transform.position;
            float updatesEsti = distance.magnitude / Mathf.Max(agent.maxSpeed, 0.1f);
            Vector3 estiPosition = targetAgent.transform.position + targetAgent.Velocity * updatesEsti;

            return Seek(agent, estiPosition);
        }
    }
}
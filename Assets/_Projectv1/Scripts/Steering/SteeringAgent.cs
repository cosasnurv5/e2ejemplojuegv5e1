using UnityEngine;

namespace Steering
{
    public class SteeringAgent : MonoBehaviour
    {
        [Header("Configuración.")]
        public float maxSpeed = 5f;
        public float maxForce = 10f;
        public float mass = 1f;

        [Header("GroundY.")]

        public float groundY = 0f; 

        public Vector3 Velocity { get; protected set; }
        public Vector3 SteeringForce { get; private set; }

        protected virtual void Update()
        {
        
            SteeringForce = Vector3.ClampMagnitude(SteeringForce, maxForce);
            Vector3 acceleration = SteeringForce / mass;

     
            Velocity = Vector3.ClampMagnitude(Velocity + acceleration * Time.deltaTime, maxSpeed);
            Velocity = new Vector3(Velocity.x, 0f, Velocity.z);

           
            Vector3 newPosition = transform.position + Velocity * Time.deltaTime;
            newPosition.y = groundY;
            transform.position = newPosition;

            
            Vector3 flatVelocity = new Vector3(Velocity.x, 0f, Velocity.z);
            if (flatVelocity.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(flatVelocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 12f);
            }

       
            SteeringForce = Vector3.zero;
        }

        public void AddForce(Vector3 force)
        {
            SteeringForce += force;
        }
    }
}
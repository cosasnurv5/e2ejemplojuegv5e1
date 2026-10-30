using UnityEngine;
using Hunter;
using Boids;

namespace UI_Debug
{
    public class DebugVisualizer : MonoBehaviour
    {
        [Header("Referencias.")]
        public HunterNPC hunter;
        public BoidAgent[] boids;

        [Header("Opciones.")]
        public bool showHunterRanges = true;
        public bool showBoidRanges = true;

        private void OnDrawGizmos()
        {
            if (showHunterRanges && hunter != null)
            {
              
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(hunter.transform.position, hunter.visionRadius);

                
                Gizmos.color = new Color(1f, 0.5f, 0f);
                Gizmos.DrawWireSphere(hunter.transform.position, hunter.RangeAttackRadius);

            
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hunter.transform.position, hunter.MeleeAttackRadius);
            }

            if (showBoidRanges && boids != null)
            {
                foreach (var boid in boids)
                {
                    if (boid == null) continue;

                    
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(boid.transform.position, boid.separationRadius);

                   
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(boid.transform.position, boid.neighborRadius);

                    
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(boid.transform.position, boid.visionRadius);
                }
            }
        }
    }
}
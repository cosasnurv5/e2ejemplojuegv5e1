using UnityEngine;

namespace Hunter
{
    public class WaypointPath : MonoBehaviour
    {
        public Transform[] waypoints;
        public bool reverseOnEnd = true;

        private int currentIndex = 0;
        private int direction = 1;
         
        public Vector3 GetCurrentWaypoint()
        {
            if (waypoints == null || waypoints.Length == 0) return transform.position;
            return waypoints[currentIndex].position;
        }

        public void AdvanceToNext()
        {
            if (waypoints == null || waypoints.Length <= 1) return;

            if (reverseOnEnd)
            {
                if (currentIndex + direction >= waypoints.Length || currentIndex + direction < 0)
                {
                    direction *= -1;
                }
                currentIndex += direction;
            }
            else
            {
                currentIndex = (currentIndex + 1) % waypoints.Length;
            }
        }
    }
}
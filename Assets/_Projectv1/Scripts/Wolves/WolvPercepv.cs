using UnityEngine;
using Hunter;

namespace Wolves
{
    public class WolvPercepv : MonoBehaviour
    {
        [Header("Configuración.")]
        public float viewRadius = 12f;
        [Range(0f, 360f)] public float viewAngle = 120f;

        [HideInInspector] public WolvAgent wolfAgent;

        private void Awake()
        {
            wolfAgent = GetComponent<WolvAgent>();
        }

        public bool CanSeeHunter(out HunterNPC hunter)
        {
            hunter = null;
            Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius);

            foreach (var hit in hits)
            {
                HunterNPC h = hit.GetComponentInParent<HunterNPC>();
                if (h != null)
                {
            
                    if (h.IsInSafeZone() || h.FSM.CurrentState == h.DestrydState) continue;

                    Vector3 dirToHunter = (h.transform.position - transform.position);
                    float dist = dirToHunter.magnitude;
                    dirToHunter.Normalize();

                    float angle = Vector3.Angle(transform.forward, dirToHunter);

                    bool isVeryClose = dist <= 4.0f;
                    bool isInViewCone = angle <= viewAngle / 2f;

                    if (isVeryClose || isInViewCone)
                    {
                        if (!isVeryClose)
                        {
                            Vector3 origin = transform.position + Vector3.up * 0.5f;
                            Vector3 targetPos = h.transform.position + Vector3.up * 0.5f;
                            float rayDist = Vector3.Distance(origin, targetPos);


                            LayerMask layerToUse = wolfAgent.wallLayer;

                            if (Physics.Raycast(origin, (targetPos - origin).normalized, out RaycastHit hitInfo, rayDist, layerToUse))
                            {
                                if (!hitInfo.collider.transform.IsChildOf(h.transform))
                                {
                                    continue; 
                                }
                            }
                        }

                        hunter = h;
                        return true;
                    }
                }
            }
            return false;
        }

 
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, viewRadius);

            Vector3 forward = transform.forward;
            Quaternion leftRot = Quaternion.Euler(0, -viewAngle / 2f, 0);
            Quaternion rightRot = Quaternion.Euler(0, viewAngle / 2f, 0);

            Vector3 leftDir = leftRot * forward;
            Vector3 rightDir = rightRot * forward;

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, leftDir * viewRadius);
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, rightDir * viewRadius);
        }
    }
}
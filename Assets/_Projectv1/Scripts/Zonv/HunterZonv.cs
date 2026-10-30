using UnityEngine;

namespace Zones
{
    public class HunterZonv : MonoBehaviour
    {
        public static HunterZonv Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}
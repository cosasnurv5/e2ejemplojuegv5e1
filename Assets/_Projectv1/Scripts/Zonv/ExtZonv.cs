using UnityEngine;

namespace Zones
{
    public class ExtZonv : MonoBehaviour
    {
        public static ExtZonv Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}
using UnityEngine;

namespace Zones
{
    public class InitialZonv : MonoBehaviour
    {
        public static InitialZonv Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
           
        }

        private void OnEnable()
        {
            Instance = this;
        }
    }
}
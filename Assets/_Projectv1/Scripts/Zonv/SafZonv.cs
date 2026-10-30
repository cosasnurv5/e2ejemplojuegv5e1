using UnityEngine;
using System.Collections.Generic;

namespace Zones
{
    public class SafeZonv : MonoBehaviour
    {
        public static List<SafeZonv> allSafZonv = new List<SafeZonv>();

        private void Awake()
        {
            if (!allSafZonv.Contains(this))
            {
                allSafZonv.Add(this);
            }
        }

        private void OnDestroy()
        {
            if (allSafZonv.Contains(this))
            {
                allSafZonv.Remove(this);
            }
        }
    }
}
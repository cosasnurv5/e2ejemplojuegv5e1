using System.Collections.Generic;
using UnityEngine;

namespace Environment
{
    public class ObjectSpawner : MonoBehaviour
    {
        [Header("Configuración.")]
        public GameObject interestObjectPrefab;
        [Tooltip("Máximo de objetos activos.")]
        public int maxActiveObjects = 5;

        private readonly List<InterestObject> activeObjects = new List<InterestObject>();

        public bool CanSpawn()
        {
            activeObjects.RemoveAll(item => item == null);
            return activeObjects.Count < maxActiveObjects;
        }

        public void SpawnInterestObject(Vector3 position)
        {
            if (!CanSpawn() || interestObjectPrefab == null) return;

            GameObject obj = Instantiate(interestObjectPrefab, position, Quaternion.identity);
            if (obj.TryGetComponent<InterestObject>(out var interestObj))
            {
                interestObj.Initialize(this);
                activeObjects.Add(interestObj);
            }
        }

        public void UnregisterObject(InterestObject obj)
        {
            if (activeObjects.Contains(obj))
            {
                activeObjects.Remove(obj);
            }
        }
    }
}
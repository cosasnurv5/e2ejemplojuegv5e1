using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boids;
using Zones; 
 
namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
          
        [Header("Configuración.")]
        public GameObject boidPrefab;
        public int initialBoidCount = 10;
        public float respawnDelay = 3f;

        [Header("HunterZonv.")]
        public LayerMask huntZoneLayer; 
        public Vector3 huntZoneSize = new Vector3(15f, 5f, 15f);

        private readonly List<BoidAgent> boids = new List<BoidAgent>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            SpawnInitialBoids();
        }

        public Vector3 FindHuntZoneCenter()
        {
            if (HunterZonv.Instance != null)
            {
                return HunterZonv.Instance.transform.position;
            }
            return Vector3.zero;
        }
         
        private void SpawnInitialBoids()
        {
            for (int i = 0; i < initialBoidCount; i++)
            {
                Vector3 spawnPos = GetRandomPositionInHuntZone();
                GameObject obj = Instantiate(boidPrefab, spawnPos, Quaternion.identity);

                if (obj.TryGetComponent<BoidAgent>(out var boid))
                {
                    boid.huntZoneSize = huntZoneSize;
                    boids.Add(boid);
                }
            }
        }

        public void CollectAndRespawnBoid(BoidAgent boid)
        {
            boid.gameObject.SetActive(false);
            StartCoroutine(RespawnRoutine(boid));
        }

        private IEnumerator RespawnRoutine(BoidAgent boid)
        {
            yield return new WaitForSeconds(respawnDelay);

            boid.transform.position = GetRandomPositionInHuntZone();
            boid.ResetAgent();
        }
           
        public Vector3 GetRandomPositionInHuntZone()
        {
            Vector3 center = FindHuntZoneCenter();
            if (center != Vector3.zero)
            {
                float randomX = Random.Range(-huntZoneSize.x / 2f + 1f, huntZoneSize.x / 2f - 1f);
                float randomZ = Random.Range(-huntZoneSize.z / 2f + 1f, huntZoneSize.z / 2f - 1f);

                Vector3 worldPoint = center + new Vector3(randomX, 0f, randomZ);
                return worldPoint;
            }

         
            return Vector3.zero;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = FindHuntZoneCenter();
            if (center != Vector3.zero)
            {
                Gizmos.color = Color.orange;
                Gizmos.DrawWireCube(center, huntZoneSize);
            }
        }
    }
}
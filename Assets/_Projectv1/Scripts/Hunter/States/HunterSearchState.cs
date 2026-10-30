using UnityEngine;
using FSM;
using Steering;
using System.Collections.Generic;

namespace Hunter.States
{
    public class HunterSearchState : IState
    {
        private readonly HunterNPC hunter;
        private float spawnTimer = 0f;
        private const float SPAWN_INTERVAL = 4f;

        public HunterSearchState(HunterNPC hunter) => this.hunter = hunter;

        public void Enter()
        {
            spawnTimer = 0f;
        }

        public void Update()
        {
            
            var destroyedBoid = hunter.FindDestroyedBoidInVision();
            if (destroyedBoid != null)
            {
                hunter.DestroyedTarget = destroyedBoid;
                hunter.FSM.ChangeState(hunter.GatherState);
                return;
            }

            
            if (hunter.TBATimer <= 0f)
            {
                var liveBoid = hunter.FindValidBoidInVision();
                if (liveBoid != null)
                {
                    hunter.CurrentTarget = liveBoid;
                    hunter.FSM.ChangeState(hunter.AttackState);
                    return;
                }
            }

          
            Vector3 huntPos = hunter.GetPositionByLayer(hunter.huntZoneLayer);
            bool inHuntZone = hunter.IsInHuntZone();

          
            if (!inHuntZone && huntPos != Vector3.zero)
            {
                if (hunter.currentPath != null && hunter.currentPathIndex < hunter.currentPath.Count)
                {
                    Vector3 targetWaypoint = hunter.GetCurrentPathTarget();
                    hunter.AddForce(SteeringBehaviors.Seek(hunter, targetWaypoint));

                    if (Vector3.Distance(hunter.transform.position, targetWaypoint) < 1.5f)
                    {
                        hunter.AdvancePath();
                    }
                }
                else
                {
                    hunter.AddForce(SteeringBehaviors.Seek(hunter, huntPos));
                }
                return;
            }

           
            hunter.Stop(); 

      
            if (hunter.spawner != null)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= SPAWN_INTERVAL)
                {
                    spawnTimer = 0f;
                    if (hunter.spawner.CanSpawn())
                    {
                        Vector3 randomSpawnPos = Managers.GameManager.Instance != null
                            ? Managers.GameManager.Instance.GetRandomPositionInHuntZone()
                            : hunter.transform.position + Random.insideUnitSphere * 1.5f;

                        hunter.spawner.SpawnInterestObject(randomSpawnPos);
                    }
                }
            }

  
            if (hunter.currentPath != null && hunter.currentPathIndex < hunter.currentPath.Count)
            {
                Vector3 targetNodePos = hunter.GetCurrentPathTarget();
                hunter.AddForce(SteeringBehaviors.Seek(hunter, targetNodePos));

                if (Vector3.Distance(hunter.transform.position, targetNodePos) < 1.0f)
                {
                    hunter.AdvancePath();
                }
            }
            else
            {
                GenerateRandomHuntNodePath();
            }
        }

        private void GenerateRandomHuntNodePath()
        {
            if (GraphManager.Instance != null && GraphManager.Instance.nodes.Count > 0)
            {
                GraphNode startNode = GraphManager.Instance.GetNearestNode(hunter.transform.position);
                Vector3 huntCenter = hunter.GetPositionByLayer(hunter.huntZoneLayer);
                Vector3 size = (Managers.GameManager.Instance != null) ? Managers.GameManager.Instance.huntZoneSize : new Vector3(4f, 1f, 4f);

                List<GraphNode> validNodes = new List<GraphNode>();
                float halfX = size.x / 2f;
                float halfZ = size.z / 2f;

                foreach (var node in GraphManager.Instance.nodes)
                {
                    if (node != null)
                    {
                        Vector3 pos = node.transform.position;
                        if (pos.x >= huntCenter.x - halfX && pos.x <= huntCenter.x + halfX &&
                            pos.z >= huntCenter.z - halfZ && pos.z <= huntCenter.z + halfZ)
                        {
                            validNodes.Add(node);
                        }
                    }
                }

                if (validNodes.Count > 0 && startNode != null)
                {
                    GraphNode targetNode = validNodes[Random.Range(0, validNodes.Count)];
                    var path = APathFinderv.FindPath(startNode, targetNode, false);
                    hunter.SetPath(path);
                }
            }
        }

        public void Exit()
        {
            hunter.SetPath(null);
        }
    }
}
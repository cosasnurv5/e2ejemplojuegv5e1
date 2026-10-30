using System.Collections.Generic;
using UnityEngine;

public class APathFinderv
{
    private class NodeRecord
    {
        public GraphNode node;
        public NodeRecord connection;
        public float gCost;
        public float hCost;
        public float fCost => gCost + hCost;

        public NodeRecord(GraphNode node)
        {
            this.node = node;
        }
    }

    public static List<GraphNode> FindPath(GraphNode startNode, GraphNode targetNode, bool excludeSafeZones = false)
    {
        if (startNode == null || targetNode == null) return null;

        List<NodeRecord> openSet = new List<NodeRecord>();
        HashSet<GraphNode> closedSet = new HashSet<GraphNode>();

        NodeRecord startRecord = new NodeRecord(startNode)
        {
            gCost = 0f,
            hCost = Vector3.Distance(startNode.transform.position, targetNode.transform.position)
        };

        openSet.Add(startRecord);

        while (openSet.Count > 0)
        {
           
            NodeRecord current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < current.fCost)
                    current = openSet[i];
            }

            if (current.node == targetNode)
            {
                return ReconstructPath(current);
            }

            openSet.Remove(current);
            closedSet.Add(current.node);

            foreach (var neighbor in current.node.neighbors)
            {
                if (neighbor == null || closedSet.Contains(neighbor)) continue;

               
                if (excludeSafeZones && neighbor.isSafeZone) continue;

                float tentativeG = current.gCost + Vector3.Distance(current.node.transform.position, neighbor.transform.position);

                NodeRecord neighborRecord = openSet.Find(r => r.node == neighbor);
                if (neighborRecord == null)
                {
                    neighborRecord = new NodeRecord(neighbor);
                    neighborRecord.gCost = tentativeG;
                    neighborRecord.hCost = Vector3.Distance(neighbor.transform.position, targetNode.transform.position);
                    neighborRecord.connection = current;
                    openSet.Add(neighborRecord);
                }
                else if (tentativeG < neighborRecord.gCost)
                {
                    neighborRecord.gCost = tentativeG;
                    neighborRecord.connection = current;
                }
            }
        }

        return null; 
    }

    private static List<GraphNode> ReconstructPath(NodeRecord endRecord)
    {
        List<GraphNode> path = new List<GraphNode>();
        NodeRecord current = endRecord;

        while (current != null)
        {
            path.Add(current.node);
            current = current.connection;
        }

        path.Reverse();
        return path;
    }
}
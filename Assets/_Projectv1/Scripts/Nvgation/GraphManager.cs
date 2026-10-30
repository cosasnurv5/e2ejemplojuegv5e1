using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GraphManager : MonoBehaviour
{
    public static GraphManager Instance { get; private set; }

    [Header("GraphManager")]
    public List<GraphNode> nodes = new List<GraphNode>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        
       
        if (nodes == null || nodes.Count == 0)
        {
            nodes = GetComponentsInChildren<GraphNode>().ToList();
            Debug.Log($"El GraphManager, {nodes.Count} nodos, detectados.");
        }
        else
        {
            Debug.Log($"El GraphManager, {nodes.Count} nodos, detectados.");
        }
    }
      
    public GraphNode GetNearestNode(Vector3 position)
    {
        if (nodes == null || nodes.Count == 0) return null;

        GraphNode nearest = null;
        float minDist = float.MaxValue;

        foreach (var node in nodes)
        {
            if (node == null) continue;
            float dist = Vector3.Distance(position, node.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = node;
            }
        }
        return nearest;
    }
}
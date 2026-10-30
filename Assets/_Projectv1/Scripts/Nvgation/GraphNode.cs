using System.Collections.Generic;
using UnityEngine;

public class GraphNode : MonoBehaviour
{
    [Header("GraphRv.")]
    public List<GraphNode> neighbors = new List<GraphNode>();
    public bool isSafeZone = false;

    private void OnDrawGizmos()
    {
       
        Gizmos.color = isSafeZone ? Color.blue : Color.gray;
        Gizmos.DrawSphere(transform.position, 0.35f);

       
        Gizmos.color = Color.white;
        foreach (var neighbor in neighbors)
        {
            if (neighbor != null)
            {
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }
}
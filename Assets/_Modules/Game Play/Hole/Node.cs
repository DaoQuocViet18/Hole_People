using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("Node Information")]
    public float gCost;
    public float hCost;

    public float FCost => gCost + hCost;

    public Node previousNode;

    public List<Node> neighbors = new List<Node>();

    public Vector3 position;
    public bool isObstacle = false;

    private void Start()
    {
        this.position = transform.position;
        neighbors = new List<Node>();

        // Định nghĩa các hướng (offset) theo 4 hướng chính
        Vector3[] directions = new Vector3[]
        {
        Vector3.forward,   // trước
        Vector3.back,      // sau
        Vector3.left,      // trái
        Vector3.right      // phải
        };

        float offsetDistance = 1f; // khoảng cách tìm kiếm

        foreach (var dir in directions)
        {
            Vector3 checkPos = transform.position + dir * offsetDistance;

            // Sử dụng OverlapSphere nhỏ để kiểm tra xem có Node tại vị trí này không
            Collider[] hits = Physics.OverlapSphere(checkPos, 0.1f); // bán kính nhỏ, chỉ kiểm tra gần đúng điểm đó

            foreach (var hit in hits)
            {
                Node node = hit.GetComponent<Node>();
                if (node != null && node != this && !neighbors.Contains(node))
                {
                    neighbors.Add(node);
                }
            }
        }
    }

}

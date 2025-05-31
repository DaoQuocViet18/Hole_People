using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EventDefine;

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

    private void OnEnable()
    {
        EventDispatcher.Add<OnClickHole>(OnClickHole);
    }

    private void OnDisable()
    {
        EventDispatcher.Remove<OnClickHole>(OnClickHole);
    }

    private void Start()
    {
        position = transform.position;
        neighbors.Clear();
        gCost = 0;
        hCost = 0;
        isObstacle = false;

        Vector3[] directions = {
                Vector3.forward,
                Vector3.back,
                Vector3.left,
                Vector3.right
            };

        float offsetDistance = 1f;
        Vector3 currentPos = transform.position;

        // Tìm các Node lân cận
        foreach (var dir in directions)
        {
            Vector3 checkPos = currentPos + dir * offsetDistance;

            foreach (var hit in Physics.OverlapSphere(checkPos, 0.1f))
            {
                Node node = hit.GetComponent<Node>();
                if (node != null && node != this && !neighbors.Contains(node))
                {
                    neighbors.Add(node);
                }
            }
        }
    }

    private void OnClickHole(IEventParam param)
    {
        if (param is OnClickHole clickHoleEvent)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.white;
            string tag = clickHoleEvent.tag;

            gCost = 0;
            hCost = 0;
            isObstacle = false;

            Vector3 currentPos = transform.position;

            // ❗ Chỉ kiểm tra vật thể phía trên node hiện tại
            if (IsBlockedAbove(currentPos, tag))
            {
                isObstacle = true;
            }
        }
    }

    private bool IsBlockedAbove(Vector3 position, string tag)
    {
        Vector3 origin = position + Vector3.up * 0.1f;
        Ray ray = new Ray(origin, Vector3.up);

        if (Physics.Raycast(ray, out RaycastHit hit, 0.5f))
        {
            return !hit.collider.CompareTag(tag);
        }

        return false;
    }
}

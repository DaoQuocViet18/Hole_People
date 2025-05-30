using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EventDefine;

public class PeopleFindHole : MonoBehaviour
{
    [Header("Lists in Gameplay")]
    public List<Node> resultPath = new List<Node>();
    public List<Node> frontierNodes = new List<Node>();
    public List<Node> exploredNodes = new List<Node>();

    [Header("Nodes in Gameplay")]
    public Node player;
    public Node target;
    public Node currentNode;

    [Header("Tag Group")]
    public string tagGroup;

    private void OnEnable()
    {
        EventDispatcher.Add<EventDefine.OnPeopleFindHole>(OnPeopleFindHole);
    }

    private void OnDisable()
    {
        EventDispatcher.Remove<EventDefine.OnPeopleFindHole>(OnPeopleFindHole);
    }

    private void OnPeopleFindHole(IEventParam param)
    {
        if (param is OnPeopleFindHole peopleFindHoleEvent)
        {
            this.target = peopleFindHoleEvent.target;
            string tag = peopleFindHoleEvent.tag;

            if (target != null && tagGroup == tag)
            {
                Setup();
            }
        }
    }

    void Setup()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 5f, LayerMask.GetMask("Block")))
        {
            player = hit.collider.GetComponent<Node>();
        }

        if (player == null || target == null)
        {
            Debug.LogError("Player hoặc Target bị null. Kiểm tra lại thiết lập.");
            return;
        }

        currentNode = player;
        frontierNodes.Clear();
        exploredNodes.Clear();
        frontierNodes.AddRange(currentNode.neighbors.Where(n => !n.isObstacle));

        foreach (var node in frontierNodes)
        {
            node.gCost = Vector3.Distance(player.transform.position, node.transform.position);
            node.hCost = Vector3.Distance(node.transform.position, target.transform.position);
            node.previousNode = currentNode;
        }

        exploredNodes.Add(currentNode);

        if (FindPath())
        {
            Debug.Log("Đã tìm thấy đường đi");
            HighlightPath();
        }
    }

    void HighlightPath()
    {
        Node node = target;

        int i = 0;
        while (node != null && node != player)
        {
            HighlightNode(node, Color.red);
            node = node.previousNode;

            i++;
            if (i > 1000)
            {
                Debug.LogWarning("Path quá dài, dừng tô màu.");
                break;
            }
        }

        // Tô màu node bắt đầu (player)
        if (node == player)
        {
            HighlightNode(node, Color.green);
        }
    }

    void HighlightNode(Node node, Color color)
    {
        var renderer = node.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    bool FindPath()
    {
        if (frontierNodes.Count <= 0)
        {
            Debug.LogWarning("Cout frontier by zero");
            return false;
        }

        if (target == player)
        {
            Debug.LogWarning("One Block");
            return true;
        }

        if (frontierNodes.Contains(target))
        {
            Debug.LogWarning("Zero Block");
            target.previousNode = currentNode;
            return true;
        }

        while (currentNode != target)
        {
            currentNode = BestNodeCostFrontier();

            if (currentNode == null)
            {
                Debug.LogWarning("Không tìm thấy đường đi đến Target.");
                return false;
            }

            if (IsNodeTarget(currentNode))
            {
                return true;
            }

            if (AddExplored(currentNode))
            {
                AddNeighborsToFrontier(currentNode);
            }
        }

        return true;
    }

    Node BestNodeCostFrontier()
    {
        if (frontierNodes.Count <= 0) return null;

        float bestFCost = frontierNodes.Min(n => n.FCost);
        return frontierNodes
            .Where(n => n.FCost == bestFCost)
            .OrderBy(n => n.hCost)
            .First();
    }

    bool IsNodeTarget(Node node) => node == target;

    bool AddExplored(Node node)
    {
        if (!exploredNodes.Contains(node) && !node.isObstacle)
        {
            frontierNodes.Remove(node);
            exploredNodes.Add(node);
            return true;
        }
        return false;
    }

    void AddNeighborsToFrontier(Node node)
    {
        foreach (var neighbor in node.neighbors)
        {
            if (exploredNodes.Contains(neighbor) || neighbor.isObstacle)
                continue;

            float GCost = node.gCost + Vector3.Distance(node.transform.position, neighbor.transform.position);

            if (!frontierNodes.Contains(neighbor))
            {
                neighbor.previousNode = node;
                neighbor.gCost = GCost;
                neighbor.hCost = Vector3.Distance(neighbor.transform.position, target.transform.position);
                frontierNodes.Add(neighbor);
            }
            else
            {
                float newFCost = GCost + neighbor.hCost;
                if (newFCost < neighbor.FCost)
                {
                    neighbor.previousNode = node;
                    neighbor.gCost = GCost;
                }
            }
        }
    }
}

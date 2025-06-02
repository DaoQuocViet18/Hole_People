using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static EventDefine;

public enum Tag
{
    None,
    Red,
    Blue,
    Green
}

[RequireComponent(typeof(PeopleController))]
public class PeopleFindHole : MonoBehaviour
{
    [SerializeField] private GroupPeopleMovementCtrl groupPeopleController;

    [Header("Lists in Gameplay")]
    private List<Node> resultPath = new List<Node>();
    private List<Node> frontierNodes = new List<Node>();
    private List<Node> exploredNodes = new List<Node>();

    [Header("Nodes in Gameplay")]
    [SerializeField] private Node player;
    private Node target;
    private Node currentNode;

    [Header("Tag Group")]
    [SerializeField] private Tag tagSelf;
    private List<Node> movingNodes = new List<Node>();
    private bool isMoving = false;

    private void Awake()
    {
        LoadComponents();
    }

    private void Reset()
    {
        LoadComponents();
    }

    void LoadComponents ()
    {
        if (groupPeopleController == null)
            groupPeopleController = GetComponentInParent<GroupPeopleMovementCtrl>();
    }

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
        if (param is OnPeopleFindHole peopleFindHoleEvent && !isMoving)
        {
            this.target = peopleFindHoleEvent.target;
            if (Enum.TryParse(peopleFindHoleEvent.tag, out Tag incomingTag))
            {
                if (target != null && tagSelf == incomingTag)
                {
                    Setup();
                }
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
        frontierNodes.AddRange(currentNode.Neighbors.Where(n => !n.IsObstacle));

        foreach (var node in frontierNodes)
        {
            node.GCost = Vector3.Distance(player.transform.position, node.transform.position);
            node.HCost = Vector3.Distance(node.transform.position, target.transform.position);
            node.PreviousNode = currentNode;

        }

        exploredNodes.Add(currentNode);

        if (FindPath())
        {
            isMoving = true;
            //Debug.Log("Đã tìm thấy đường đi");

            HighlightPath();
            movingNodes.Reverse();
            groupPeopleController.PeopleController.MovePeople(movingNodes);
        }
    }

    void HighlightPath()
    {
        Node node = target;

        int i = 0;
        while (node != null && node != player)
        {
            HighlightNode(node, Color.red);
            node = node.PreviousNode;
            movingNodes.Add(node);

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
            //Debug.LogWarning("Cout frontier by zero");
            return false;
        }

        if (target == player)
        {
            //Debug.LogWarning("One Block");
            return true;
        }

        if (frontierNodes.Contains(target))
        {
            //Debug.LogWarning("Zero Block");
            target.PreviousNode = currentNode;
            return true;
        }

        while (currentNode != target)
        {
            currentNode = BestNodeCostFrontier();

            if (currentNode == null)
            {
                //Debug.LogWarning("Không tìm thấy đường đi đến Target.");
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
            .OrderBy(n => n.HCost)
            .First();
    }

    bool IsNodeTarget(Node node) => node == target;

    bool AddExplored(Node node)
    {
        if (!exploredNodes.Contains(node) && !node.IsObstacle)
        {
            frontierNodes.Remove(node);
            exploredNodes.Add(node);
            return true;
        }
        return false;
    }

    void AddNeighborsToFrontier(Node node)
    {
        foreach (var neighbor in node.Neighbors)
        {
            if (exploredNodes.Contains(neighbor) || neighbor.IsObstacle)
                continue;

            float GCost = node.GCost + Vector3.Distance(node.transform.position, neighbor.transform.position);

            if (!frontierNodes.Contains(neighbor))
            {
                neighbor.PreviousNode = node;
                neighbor.GCost = GCost;
                neighbor.HCost = Vector3.Distance(neighbor.transform.position, target.transform.position);
                frontierNodes.Add(neighbor);
            }
            else
            {
                float newFCost = GCost + neighbor.HCost;
                if (newFCost < neighbor.FCost)
                {
                    neighbor.PreviousNode = node;
                    neighbor.GCost = GCost;
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EventDefine;

public class Node : MonoBehaviour
{
    [Header("Node Information")]
    private float gCost;
    private float hCost;
    public float GCost { get => gCost; set => gCost = value; }
    public float HCost { get => hCost; set => hCost = value; }
    public float FCost => GCost + HCost;


    [SerializeField] private Node previousNode;
    [SerializeField] private List<Node> neighbors = new List<Node>();
    [SerializeField] private Vector3 position;
    [SerializeField] private bool isObstacle = false;
    public Node PreviousNode { get => previousNode; set => previousNode = value; }
    public List<Node> Neighbors { get => neighbors; set => neighbors = value; }
    public Vector3 Position { get => position; set => position = value; }
    public bool IsObstacle { get => isObstacle; set => isObstacle = value; }


    private void OnEnable()
    {
        EventDispatcher.Add<OnNodeRun>(OnHoleClickedSetUp);
    }

    private void OnDisable()
    {
        EventDispatcher.Remove<OnNodeRun>(OnHoleClickedSetUp);
    }

    private void Start()
    {
        SetUp();
    }

    void SetUp()
    {
        Position = transform.position;
        Neighbors.Clear();
        GCost = 0;
        HCost = 0;
        IsObstacle = false;

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
                if (node != null && node != this && !Neighbors.Contains(node))
                {
                    Neighbors.Add(node);
                }
            }
        }
    }

    private void OnHoleClickedSetUp(IEventParam param)
    {
        if (param is OnNodeRun nodeRunEvent)
        {
            //gameObject.GetComponent<Renderer>().material.color = Color.white;
            string tag = nodeRunEvent.tag;

            GCost = 0;
            HCost = 0;
            IsObstacle = false;

            Vector3 currentPos = transform.position;

            // ❗ Chỉ kiểm tra vật thể phía trên node hiện tại
            if (IsBlockedAbove(currentPos, tag))
            {
                IsObstacle = true;
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

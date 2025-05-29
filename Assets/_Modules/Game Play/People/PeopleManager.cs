using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PeopleManager : MonoBehaviour
{
    [SerializeField] private GameObject[] allPeople;

    private Dictionary<string, List<GameObject>> groups = new Dictionary<string, List<GameObject>>();

    void Start()
    {
        SetupPeople();

        SetupGroups();
    }

    void SetupPeople()
    {
        int peopleLayer = LayerMask.NameToLayer("People");
        List<GameObject> foundPeople = new List<GameObject>();

        foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj.layer == peopleLayer)
                foundPeople.Add(obj);
        }

        allPeople = foundPeople.ToArray();
        Debug.Log($"Tìm thấy {allPeople.Length} người trong layer 'People'.");
    }

    void SetupGroups()
    {
        groups.Clear();

        foreach (GameObject person in allPeople)
        {
            string tag = person.tag;
            if (!groups.ContainsKey(tag))
                groups[tag] = new List<GameObject>();

            groups[tag].Add(person);
        }

        Debug.Log($"Đã phân chia {allPeople.Length} người thành {groups.Count} nhóm dựa trên tag.");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Chuột trái
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                int layer = hit.collider.gameObject.layer;
                if (layer == LayerMask.NameToLayer("Hole"))
                {
                    string groupTag = hit.collider.gameObject.tag;

                    if (groups.ContainsKey(groupTag))
                    {
                        Vector3 targetPosition = hit.collider.gameObject.transform.position;
                        MoveGroup(groupTag, targetPosition);
                    }
                }
            }
        }
    }

    public void MoveGroup(string groupName, Vector3 target)
    {
        if (!groups.ContainsKey(groupName)) return;

        foreach (GameObject person in groups[groupName])
        {
            NavMeshAgent agent = person.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = 15;
                agent.SetDestination(target);
            }
        }
    }
}

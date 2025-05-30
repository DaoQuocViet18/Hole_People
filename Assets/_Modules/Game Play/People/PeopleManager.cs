using System.Collections;
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
                        StartCoroutine(HandleClickHole(hit.collider.gameObject, groupTag));
                    }
                }
            }
        }
    }

    private IEnumerator HandleClickHole(GameObject holeObject, string groupTag)
    {
        Debug.Log("groupTag: " + groupTag);

        // Gửi sự kiện click hole
        EventDispatcher.Dispatch(new EventDefine.OnClickHole { tag = groupTag });

        // Chờ 1 frame để các listener phản hồi
        yield return null;

        // Sau khi listener xử lý xong, tiếp tục kiểm tra Node bên dưới
        Vector3 checkPosBelow = holeObject.transform.position + Vector3.down;
        Collider[] hits = Physics.OverlapSphere(checkPosBelow, 0.1f, LayerMask.GetMask("Block"));

        foreach (var hitBelow in hits)
        {
            Node nodeBelow = hitBelow.GetComponent<Node>();
            if (nodeBelow != null)
            {
                EventDispatcher.Dispatch(new EventDefine.OnPeopleFindHole
                {
                    tag = groupTag,
                    target = nodeBelow
                });
                break;
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

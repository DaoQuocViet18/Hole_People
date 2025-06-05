using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ContainArrangement : MonoBehaviour
{
    [SerializeField] private Tag tagContain;
    [SerializeField] private List<GameObject> groupPeople = new List<GameObject>();
    [SerializeField] private int maxCapacity = 8;

    private static readonly Vector3 startOffset = new Vector3(1.5f, 0, -3.6f);
    private const int maxPerRow = 4;
    private const float spacingX = 1f;
    private const float spacingZ = 1f;

    public Tag TagContain { get => tagContain; set => tagContain = value; }
    public List<GameObject> GroupPeople { get => groupPeople; set => groupPeople = value; }
    private HashSet<GameObject> arrangedSet = new HashSet<GameObject>();
    public int ContainBlank => Mathf.Max(0, maxCapacity - groupPeople.Count);

    public void Arrangement()
    {
        StartCoroutine(ArrangeSmoothly());
    }

    private IEnumerator ArrangeSmoothly()
    {
        List<GameObject> allPeople = new List<GameObject>();

        foreach (var group in groupPeople)
        {
            if (group == null) continue;
            var people = group.GetComponentsInChildren<PeopleMovement>(true);
            foreach (var person in people)
            {
                if (person != null)
                    allPeople.Add(person.gameObject);
            }
        }

        for (int i = 0; i < allPeople.Count; i++)
        {
            GameObject person = allPeople[i];
            if (person == null) continue;

            int row = i / maxPerRow;
            int col = i % maxPerRow;

            Vector3 targetPos = transform.position + new Vector3(
                startOffset.x - (col * spacingX),
                startOffset.y,
                startOffset.z + (row * spacingZ)
            );

            if (!arrangedSet.Contains(person))
            {
                person.transform.position = targetPos;
                person.transform.rotation = Quaternion.LookRotation(Vector3.forward);
                yield return new WaitForSeconds(0.1f); // Đợi 0.1s rồi mới xử lý người tiếp theo
                arrangedSet.Add(person);
            }    
        }
    }
}

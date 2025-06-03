using System.Collections.Generic;
using UnityEngine;

public class ContainArrangement : MonoBehaviour
{
    [SerializeField] private Tag tagContain;
    [SerializeField] private List<GameObject> people = new List<GameObject>();
    [SerializeField] private int maxCapacity = 32;

    private static readonly Vector3 startOffset = new Vector3(1.5f, 0, -3.6f);
    private const int maxPerRow = 4;
    private const float spacingX = 1f;
    private const float spacingZ = 1f;

    public Tag TagContain { get => tagContain; set => tagContain = value; }
    public List<GameObject> People { get => people; set => people = value; }

    public int ContainBlank => Mathf.Max(0, maxCapacity - people.Count); // READ-ONLY

    public void Arrangement()
    {
        int count = Mathf.Min(people.Count, maxCapacity);
        for (int i = 0; i < count; i++)
        {
            GameObject person = people[i];
            if (person == null) continue;

            int row = i / maxPerRow;
            int col = i % maxPerRow;

            Vector3 offset = new Vector3(
                startOffset.x - (col * spacingX),
                startOffset.y,
            startOffset.z + (row * spacingZ)
            );

            person.transform.position = transform.position + offset;
            person.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        }
    }
}

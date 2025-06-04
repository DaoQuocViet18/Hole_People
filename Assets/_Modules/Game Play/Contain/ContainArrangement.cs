using System.Collections.Generic;
using UnityEngine;

public class ContainArrangement : MonoBehaviour
{
    [SerializeField] private Tag tagContain;
    [SerializeField] private List<GameObject> groupPeople = new List<GameObject>();
    [SerializeField] private int maxCapacity = 8; // Số lượng nhóm tối đa

    private static readonly Vector3 startOffset = new Vector3(1.5f, 0, -3.6f);
    private const int maxPerRow = 4;
    private const float spacingX = 1f;
    private const float spacingZ = 1f;

    public Tag TagContain { get => tagContain; set => tagContain = value; }
    public List<GameObject> GroupPeople { get => groupPeople; set => groupPeople = value; }

    // Số chỗ còn trống tính theo nhóm (mỗi group là 1 slot)
    public int ContainBlank => Mathf.Max(0, maxCapacity - groupPeople.Count);

    public void Arrangement()
    {
        // Gom toàn bộ PeopleMovement từ các group
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

        // Sắp xếp tất cả người thành lưới
        for (int i = 0; i < allPeople.Count; i++)
        {
            GameObject person = allPeople[i];
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

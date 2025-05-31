using System;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

public class PeopleHoleManager : MonoBehaviour
{
    [SerializeField] private HoleTouch[] hole;
    [SerializeField] private ContainArrangement[] containArrangements;
    private Dictionary<string, List<GameObject>> groups = new Dictionary<string, List<GameObject>>();

    private void Reset()
    {
        SetUp();
    }

    private void Awake()
    {
        SetUp();
    }

    void SetUp()
    {
        if (hole == null || hole.Length == 0)
            hole = UnityEngine.Object.FindObjectsByType<HoleTouch>(FindObjectsSortMode.None);

        if (containArrangements == null || containArrangements.Length == 0)
            containArrangements = UnityEngine.Object.FindObjectsByType<ContainArrangement>(FindObjectsSortMode.None);

        foreach (var h in hole)
        {
            h.OnAllPeopleEntered += HandleAllPeopleEnteredHole;
        }
    }

    private void HandleAllPeopleEnteredHole(List<GameObject> people)
    {
        if (people == null || people.Count == 0)
            return;

        string groupTag = people[0].tag;

        if (!groups.ContainsKey(groupTag))
        {
            groups[groupTag] = new List<GameObject>();
        }

        foreach (GameObject person in people)
        {
            if (!groups[groupTag].Contains(person))
                groups[groupTag].Add(person); 
        }
        //Debug.Log($"Tất cả people thuộc nhóm '{groupTag}' đã vào hole. Tổng số: {groups[groupTag].Count}");

        // Convert groupTag to enum
        if (!Enum.TryParse(groupTag, out Tag incomingTag))
        {
            Debug.LogWarning($"Failed to parse tag '{groupTag}' to Tag enum.");
            return;
        }

        ContainArrangement targetContain = null;

        // Bước 1: Tìm contain có tagContain == incomingTag
        foreach (ContainArrangement contain in containArrangements)
        {
            if (contain.tagContain == incomingTag && !contain.fulled)
            {
                targetContain = contain;
                break;
            }
        }

        // Bước 2: Nếu không tìm thấy, tìm contain có tagContain == Tag.None
        if (targetContain == null)
        {
            foreach (ContainArrangement contain in containArrangements)
            {
                if (contain.tagContain == Tag.None)
                {
                    contain.tagContain = incomingTag; // Gán tag
                    targetContain = contain;
                    break;
                }
            }
        }

        // Bước 3: Nếu tìm được, đưa object vào contain
        if (targetContain != null)
        {
            targetContain.newPeople.AddRange(groups[groupTag]);
            groups[groupTag].Clear();
            targetContain.Arrangement();
            Debug.Log($"Assigned people to contain with tag: {targetContain.tagContain}");
        }
        else
        {
            Debug.LogWarning($"No available ContainArrangement found for tag: {incomingTag}");
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class PeopleHoleManager : MonoBehaviour
{
    [SerializeField] private HoleTouch[] hole;
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
            hole = hole = Object.FindObjectsByType<HoleTouch>(FindObjectsSortMode.None);

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

        Debug.Log($"Tất cả people thuộc nhóm '{groupTag}' đã vào hole. Tổng số: {groups[groupTag].Count}");

        // TODO: Xử lý logic đưa họ tới vị trí đứng
    }

}

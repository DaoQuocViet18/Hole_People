using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PeopleGroup
{
    public string tag;
    public List<GameObject> people = new List<GameObject>();
}

public class PeopleSpawnManager : Singleton<PeopleSpawnManager>, ICtrl
{
    [SerializeField] private List<PeopleGroup> groupsInGame = new List<PeopleGroup>();
    [SerializeField] private List<PeopleGroup> groupsInContain = new List<PeopleGroup>();

    public List<PeopleGroup> GroupsInGame { get => groupsInGame; set => groupsInGame = value; }
    public List<PeopleGroup> GroupsInContain { get => groupsInContain; set => groupsInContain = value; }

    private void Awake()
    {
        LoadComponents();
    }

    private void Start()
    {
        Init();
    }

    private void Reset()
    {
        LoadComponents();
        ResetValue();
    }

    public void LoadComponents()
    {
        // Your implementation
        LoadGroupedPeopleInGame();
    }

    public void ResetValue()
    {
        // Your implementation
    }

    public void Init()
    {
        // Your implementation
    }

    private void LoadGroupedPeopleInGame()
    {
        GroupsInGame.Clear();

        PeopleMovement[] peopleMovements = FindObjectsByType<PeopleMovement>(FindObjectsSortMode.None);
        int loadedCount = 0;

        foreach (var person in peopleMovements)
        {
            if (person == null) continue;
            AddToGroup(GroupsInGame, person.tag, person.gameObject);
            loadedCount++;
        }

        Debug.Log($"Loaded {loadedCount} PeopleMovement objects into {GroupsInGame.Count} tag groups.");
    }

    public void SetActivePeople (GameObject obj, bool state)
    {
        obj.SetActive(state);
    }

    public void Despawn(GameObject obj)
    {
        if (obj == null) return;

        // Remove from groupsInGame
        RemoveFromGroup(groupsInGame, obj);

        // Add to groupsInContain
        AddToGroup(groupsInContain, obj.tag, obj);

        // Deactivate the object
        SetActivePeople(obj, false);
    }

    /// Thêm một GameObject vào nhóm có tag tương ứng trong danh sách.
    private void AddToGroup(List<PeopleGroup> groupList, string tag, GameObject obj)
    {
        if (string.IsNullOrEmpty(tag) || obj == null) return;

        // Tìm hoặc tạo nhóm theo tag
        PeopleGroup group = groupList.Find(g => g.tag == tag);
        if (group == null)
        {
            group = new PeopleGroup { tag = tag };
            groupList.Add(group);
        }

        // Thêm nếu chưa có
        if (!group.people.Contains(obj))
        {
            group.people.Add(obj);
        }
    }

    /// Gỡ GameObject khỏi nhóm chứa nó trong danh sách.
    private void RemoveFromGroup(List<PeopleGroup> groupList, GameObject obj)
    {
        if (obj == null) return;

        foreach (var group in groupList)
        {
            if (group.people.Remove(obj)) break; // Gỡ và thoát ngay khi tìm thấy
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class GroupedPeopleCtrl : CtrlMonoBehaviour
{
    [SerializeField] private List<GroupOfPeople> groupedPeopleInGame = new();
    [SerializeField] private List<GroupOfPeople> groupedPeopleFinishGame = new();

    public List<GroupOfPeople> GroupedPeopleInGame
    {
        get => groupedPeopleInGame;
        set => groupedPeopleInGame = value ?? new List<GroupOfPeople>();
    }

    public List<GroupOfPeople> GroupedPeopleFinishGame
    {
        get => groupedPeopleFinishGame;
        set => groupedPeopleFinishGame = value ?? new List<GroupOfPeople>();
    }

    protected override void LoadComponents()
    {
        LoadAllGroupedPeople();
    }

    private void LoadAllGroupedPeople()
    {
        groupedPeopleInGame.Clear();

        // Lấy tất cả GroupPeopleMovement trong scene (lấy component)
        GroupPeopleMovement[] allPeopleComponents = FindObjectsByType<GroupPeopleMovement>(FindObjectsSortMode.None);
        int total = 0;

        foreach (GroupPeopleMovement personComponent in allPeopleComponents)
        {
            if (personComponent == null) continue;

            GameObject personObj = personComponent.gameObject;
            string tagStr = personObj.tag;

            // Chuyển tag từ string sang enum Tag
            if (!Enum.TryParse(tagStr, ignoreCase: true, out Tag tagGroup))
            {
                Debug.LogWarning($"[PeopleManager] Unrecognized tag '{tagStr}' on GameObject '{personObj.name}'. Skipping.");
                continue;
            }

            AddPersonToGroup(tagGroup, personObj);
            total++;
        }

        //Debug.Log($"[PeopleManager] Loaded {total} people into {groupedPeopleInGame.Count} tag groups.");
    }

    private void AddPersonToGroup(Tag tag, GameObject personObj)
    {
        if (personObj == null) return;

        // Tìm nhóm theo tag
        GroupOfPeople group = groupedPeopleInGame.Find(g => g.tag == tag);

        // Nếu chưa có nhóm, tạo mới và thêm vào list
        if (group == null)
        {
            group = new GroupOfPeople { tag = tag };
            groupedPeopleInGame.Add(group);
        }

        // Thêm GameObject vào danh sách groupPeople nếu chưa có
        if (!group.groupPeople.Contains(personObj))
        {
            group.groupPeople.Add(personObj);
        }
    }

    /// Thêm một list GameObject vào nhóm có tag tương ứng trong danh sách.
    public void AddToGroup(List<GroupOfPeople> groupList, Tag tag, List<GameObject> objs)
    {
        if (objs == null || objs.Count == 0)
            return;

        var group = groupList.Find(g => g.tag == tag);
        if (group == null)
        {
            group = new GroupOfPeople { tag = tag };
            groupList.Add(group);
        }

        foreach (GameObject obj in objs)
        {
            if (obj == null)
                continue;

            if (!group.groupPeople.Contains(obj))
            {
                group.groupPeople.Add(obj);
            }
        }
    }


    /// Gỡ list GameObject khỏi nhóm chứa nó trong danh sách.
    public void RemoveFromGroup(List<GroupOfPeople> groupList, List<GameObject> objs)
    {
        if (objs == null || objs.Count == 0)
            return;

        foreach (GameObject obj in objs)
        {
            if (obj == null)
                continue;

            foreach (var group in groupList)
            {
                if (group.groupPeople.Remove(obj))
                    break;
            }
        }
    }

}

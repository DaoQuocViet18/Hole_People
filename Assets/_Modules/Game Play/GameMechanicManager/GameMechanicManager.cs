using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EventDefine;

public class GameMechanicManager : Singleton<GameMechanicManager>
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

        Debug.Log($"[PeopleManager] Loaded {total} people into {groupedPeopleInGame.Count} tag groups.");
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

    private void OnEnable()
    {
        EventDispatcher.Add<OnPeopleRun>(OnPeopleRun);
        EventDispatcher.Add<OnEntryHoleTouch>(OnEntryHoleTouch);
    }

    private void OnDisable()
    {
        EventDispatcher.Remove<OnPeopleRun>(OnPeopleRun);
        EventDispatcher.Remove<OnEntryHoleTouch>(OnEntryHoleTouch);
    }

    private void OnPeopleRun(IEventParam param)
    {
        if (param is not OnPeopleRun peopleRunEvent) return;
        if (!Enum.TryParse(peopleRunEvent.tag, true, out Tag tagHole)) return;

        GroupOfPeople group = groupedPeopleInGame.Find(g => g.tag == tagHole);
        if (group == null || group.groupPeople.Count == 0)
        {
            Debug.LogWarning($"[PeopleManager] No people found for tag '{tagHole}'");
            return;
        }

        int numberGroup = 0;

        // 1. MainHole cùng tag thì +4 mỗi cái
        if (FinishHoleManager.Instance.MainHoleLeft != null &&
            FinishHoleManager.Instance.MainHoleLeft.tag == peopleRunEvent.tag)
        {
            numberGroup += FinishHoleManager.Instance.LeftHoleSide.HoleBlank / 4;
        }

        if (FinishHoleManager.Instance.MainHoleRight != null &&
            FinishHoleManager.Instance.MainHoleRight.tag == peopleRunEvent.tag)
        {
            numberGroup += FinishHoleManager.Instance.RightHoleSide.HoleBlank / 4;
        }


        // 2. Contain logic

        foreach (var item in ContainManager.Instance.ContainArrangements)
        {
            if (item.TagContain == tagHole)
            {
                numberGroup += item.ContainBlank / 4;
                break;
            }
        }

        foreach (var item in ContainManager.Instance.ContainArrangements)
        {
            if (item.TagContain == Tag.None)
            {
                numberGroup += 8;
                break;
            }
        }

        // 3. Di chuyển nhóm
        PeopleManager.Instance.GroupPeopleFindHole(group.groupPeople, peopleRunEvent.target, numberGroup);
    }

    private void OnEntryHoleTouch(IEventParam param)
    {
        if (param is not OnEntryHoleTouch entryHoleTouchEvent) return;
        if (!Enum.TryParse(entryHoleTouchEvent.tag, true, out Tag tagHole)) return;

        List<GameObject> listParentPeople = entryHoleTouchEvent.groupParentPeople;

        HashSet<Transform> uniqueParents = new();
        List<GameObject> parentsToFinishGame = new();

        foreach (GameObject parentPeople in listParentPeople)
        {
            if (parentPeople == null) continue;

            Transform parentTransform = parentPeople.transform.parent;
            if (parentTransform == null || !uniqueParents.Add(parentTransform))
                continue; // Skip nếu null hoặc đã xử lý

            // Lấy tất cả PeopleMovement trong các con của parent
            PeopleMovement[] childrenMovements = parentTransform.GetComponentsInChildren<PeopleMovement>();

            bool allMoved = true;
            foreach (PeopleMovement movement in childrenMovements)
            {
                if (movement == null || !movement.Moved)
                {
                    allMoved = false;
                    break;
                }
            }

            if (allMoved)
            {
                parentsToFinishGame.Add(parentTransform.gameObject);
            }
        }

        if (parentsToFinishGame.Count > 0)
        {
            foreach (var parentObj in parentsToFinishGame)
            {
                BringIntoFinishGame(parentObj);
            }
        }
    }

    public void BringIntoFinishGame(GameObject parentObj)
    {
        if (parentObj == null) return;

        RemoveFromGroup(groupedPeopleInGame, parentObj);

        if (!Enum.TryParse<Tag>(parentObj.tag, true, out var tag)) return;

        AddToGroup(groupedPeopleFinishGame, tag, parentObj);

        PutInFinishHoleOrContain(tag, parentObj);
    }

    /// Thêm một GameObject vào nhóm có tag tương ứng trong danh sách.
    private void AddToGroup(List<GroupOfPeople> groupList, Tag tag, GameObject obj)
    {
        if (obj == null) return;

        var group = groupList.Find(g => g.tag == tag);
        if (group == null)
        {
            group = new GroupOfPeople { tag = tag };
            groupList.Add(group);
        }

        if (!group.groupPeople.Contains(obj))
        {
            group.groupPeople.Add(obj);
        }
    }

    /// Gỡ GameObject khỏi nhóm chứa nó trong danh sách.
    private void RemoveFromGroup(List<GroupOfPeople> groupList, GameObject obj)
    {
        if (obj == null) return;

        foreach (var group in groupList)
        {
            if (group.groupPeople.Remove(obj))
                break;
        }
    }

    void PutInFinishHoleOrContain(Tag tag, GameObject parentObj)
    {
        if (FinishHoleManager.Instance.CheckTagFinishHole(tag))
        {
            FinishHoleManager.Instance.PutPeopleInFinishHole(parentObj);
            RemoveFromGroup(groupedPeopleFinishGame, parentObj);
            Destroy(parentObj);
        }
        else if (ContainManager.Instance.CheckTagContain(tag))
            ContainManager.Instance.PutPeopleInContain(tag, parentObj);
    }
}

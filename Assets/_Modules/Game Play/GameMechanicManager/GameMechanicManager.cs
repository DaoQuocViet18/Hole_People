using System;
using System.Collections.Generic;
using UnityEngine;
using static EventDefine;

public class GameMechanicManager : Singleton<GameMechanicManager>, ICtrl
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
        LoadAllGroupedPeople();
    }

    public void ResetValue()
    {
        // Reset values here if needed
    }

    public void Init()
    {
        // Init game logic here if needed
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
    }

    private void OnDisable()
    {
        EventDispatcher.Remove<OnPeopleRun>(OnPeopleRun);
    }

    private void OnPeopleRun(IEventParam param)
    {
        if (param is not OnPeopleRun peopleRunEvent) return;

        if (!Enum.TryParse(peopleRunEvent.tag, true, out Tag tagHole)) return;

        // Lấy danh sách người trong group có tag tương ứng
        List<GameObject> groupPeople = new();

        foreach (GroupOfPeople group in groupedPeopleInGame)
        {
            if (group.tag == tagHole)
            {
                groupPeople.AddRange(group.groupPeople);
                break; // vì mỗi tag chỉ nên có 1 group
            }
        }

        if (groupPeople.Count == 0)
        {
            Debug.LogWarning($"[PeopleManager] No people found for tag '{tagHole}'");
            return;
        }

        PeopleManager.Instance.GroupPeopleFindHole(groupPeople, peopleRunEvent.target);
    }
}

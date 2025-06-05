using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EventDefine;

[RequireComponent(typeof(GroupedPeopleCtrl))]
public class GameMechanicManager : Singleton<GameMechanicManager>
{
    [SerializeField] private GroupedPeopleCtrl groupedPeopleCtrl;

    [SerializeField] private Tag[] mainFinishHoles = new Tag[2];
    [SerializeField] private Tag[] containes = new Tag[4];
    [SerializeField] private int numberFinishHole = 0;
    [SerializeField] private int numberContain = 0;
    [SerializeField] private int numberGroup => numberFinishHole + numberContain;
    protected override void LoadComponents()
    {
        groupedPeopleCtrl = GetComponent<GroupedPeopleCtrl>();
    }

    private void Start()
    {
        LoadListTag();
    }
    private void LoadListTag()
    {
        mainFinishHoles = FinishHoleManager.Instance.ExportTagFinishHole();
        containes = ContainManager.Instance.ExportTagContain();
    }

    private void OnEnable()
    {
        EventDispatcher.Add<OnPeopleRun>(OnPeopleRun);
        EventDispatcher.Add<OnEntryHoleTouch>(OnEntryHoleTouch);
        EventDispatcher.Add<OnChangeMainHole>(OnChangeMainHole);
    }

    private void OnDisable()
    {
        EventDispatcher.Remove<OnPeopleRun>(OnPeopleRun);
        EventDispatcher.Remove<OnEntryHoleTouch>(OnEntryHoleTouch);
        EventDispatcher.Remove<OnChangeMainHole>(OnChangeMainHole);
    }

    private void OnPeopleRun(IEventParam param)
    {
        if (param is not OnPeopleRun peopleRunEvent) return;
        if (!Enum.TryParse(peopleRunEvent.tag, true, out Tag tagHole)) return;

        GroupOfPeople group = groupedPeopleCtrl.GroupedPeopleInGame.Find(g => g.tag == tagHole);
        if (group == null || group.groupPeople.Count == 0)
        {
            Debug.LogWarning($"[PeopleManager] No people found for tag '{tagHole}'");
            return;
        }

        numberFinishHole = 0;
        numberContain = 0;
        FinishHoleManager fhManager = FinishHoleManager.Instance;

        if (fhManager.MainHoleLeft != null && fhManager.MainHoleLeft.tag == peopleRunEvent.tag)
            numberFinishHole += fhManager.LeftFHInfo.holeBlankGroups;

        if (fhManager.MainHoleRight != null && fhManager.MainHoleRight.tag == peopleRunEvent.tag)
            numberFinishHole += fhManager.RightFHInfo.holeBlankGroups;

        // Contain logic: ưu tiên container cùng tag (và còn chỗ trống), nếu không có thì tìm container None (nếu còn chỗ trống) và cộng 8
        ContainArrangement containWithTag = ContainManager.Instance.ContainArrangements.FirstOrDefault(c => c.TagContain == tagHole && c.ContainBlank > 0);
        if (containWithTag != null)
        {
            numberContain += containWithTag.ContainBlank;
        }
        else
        {
            ContainArrangement emptyContain = ContainManager.Instance.ContainArrangements
                .FirstOrDefault(c => c.TagContain == Tag.None && c.ContainBlank > 0);
            if (emptyContain != null)
                numberContain += 8;
        }

        // Di chuyển nhóm đến đích
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

    private void OnChangeMainHole(IEventParam param)
    {
        mainFinishHoles = FinishHoleManager.Instance.ExportTagFinishHole();
        containes = ContainManager.Instance.ExportTagContain();
        Debug.Log("OnChangeMainHole");
        PutContainToFinishHole();
    }

    public void BringIntoFinishGame(GameObject parentObj)
    {
        if (parentObj == null) return;

        groupedPeopleCtrl.RemoveFromGroup(groupedPeopleCtrl.GroupedPeopleInGame, parentObj);

        if (!Enum.TryParse(parentObj.tag, out Tag tag)) return;
        groupedPeopleCtrl.AddToGroup(groupedPeopleCtrl.GroupedPeopleFinishGame, tag, parentObj);

        PutInFinishHoleOrContain(tag, parentObj);
    }

    private void PutInFinishHoleOrContain(Tag tag, GameObject parentObj)
    {
        // Kiểm tra nếu tag phù hợp với mainFinishHoles
        if (mainFinishHoles.Contains(tag) && numberFinishHole > 0)
        {
            numberFinishHole--;
            FinishHoleManager.Instance.PutPeopleInFinishHole(parentObj, Kind.EntryHole);
            groupedPeopleCtrl.RemoveFromGroup(groupedPeopleCtrl.GroupedPeopleFinishGame, parentObj);
        }
        // Nếu không, kiểm tra nếu tag thuộc containes hoặc containes có chỗ trống (Tag.None)
        else if (containes.Any(t => t == tag || t == Tag.None) && numberContain > 0)
        {
            numberContain--;
            ContainManager.Instance.PutPeopleInContain(tag, parentObj);

        }
        mainFinishHoles = FinishHoleManager.Instance.ExportTagFinishHole();
        containes = ContainManager.Instance.ExportTagContain();
    }

    private void PutContainToFinishHole()
    {
        var matchingTag = mainFinishHoles
            .FirstOrDefault(tag => containes.Contains(tag));

        if (matchingTag == Tag.None) return;

        int indexMain = Array.IndexOf(mainFinishHoles, matchingTag);
        int indexContain = Array.IndexOf(containes, matchingTag);

        if (indexMain < 0 || indexContain < 0) return;
        Debug.Log($"Trùng tag ở: mainFinishHoles[{indexMain}] và containes[{indexContain}] => Tag: {matchingTag}");

        int mainBlank = indexMain == 0
            ? FinishHoleManager.Instance.LeftFHInfo.holeBlankGroups
            : FinishHoleManager.Instance.RightFHInfo.holeBlankGroups;

        var containData = ContainManager.Instance.ContainArrangements[indexContain];

        int takeCount = Mathf.Min(mainBlank, containData.GroupPeople.Count);

        // Lấy từ cuối danh sách
        List<GameObject> listPeopleInContain = containData.GroupPeople
            .Skip(containData.GroupPeople.Count - takeCount)
            .Take(takeCount)
            .ToList();

        // Xoá từ cuối danh sách
        containData.GroupPeople.RemoveRange(containData.GroupPeople.Count - takeCount, takeCount);

        // Gửi vào FinishHole
        foreach (var people in listPeopleInContain)
        {
            FinishHoleManager.Instance.PutPeopleInFinishHole(people, Kind.Contain);
        }

        
        Debug.Log($"mainFinishHoles[{indexMain}]: Blank còn lại = {mainBlank}, Đã chuyển: {takeCount}");
    }

}

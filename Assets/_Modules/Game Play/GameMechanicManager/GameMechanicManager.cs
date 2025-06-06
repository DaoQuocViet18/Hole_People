using System;
using System.Collections.Generic;
using System.Globalization;
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
    private int numberGroup => numberFinishHole + numberContain;

    protected override void LoadComponents()
    {
        // Khởi tạo tham chiếu đến GroupedPeopleCtrl
        groupedPeopleCtrl = GetComponent<GroupedPeopleCtrl>();
    }

    private void Start()
    {
        // Tải danh sách tag của các hố chính và container từ các Manager
        LoadListTag();
    }

    private void LoadListTag()
    {
        // Cập nhật mainFinishHoles và containes
        mainFinishHoles = FinishHoleManager.Instance.FinishHoleCtrl.ExportTagFinishHole();
        containes = ContainManager.Instance.ExportTagContain();
    }

    private void OnEnable()
    {
        // Đăng ký sự kiện
        EventDispatcher.Add<OnPeopleRun>(OnPeopleRun);
        EventDispatcher.Add<OnEntryHoleTouch>(OnEntryHoleTouch);
        EventDispatcher.Add<OnChangeMainHole>(OnChangeMainHole);
    }

    private void OnDisable()
    {
        // Hủy đăng ký sự kiện
        EventDispatcher.Remove<OnPeopleRun>(OnPeopleRun);
        EventDispatcher.Remove<OnEntryHoleTouch>(OnEntryHoleTouch);
        EventDispatcher.Remove<OnChangeMainHole>(OnChangeMainHole);
    }

    private void OnPeopleRun(IEventParam param)
    {
        // Xử lý khi có nhóm người bắt đầu chạy và tính toán số chỗ còn trống
        if (param is not OnPeopleRun peopleRunEvent) return;
        if (!Enum.TryParse(peopleRunEvent.tag, true, out Tag tagHole)) return;

        var group = groupedPeopleCtrl.GroupedPeopleInGame.FirstOrDefault(g => g.tag == tagHole);
        if (group == null || group.groupPeople.Count == 0)
        {
            Debug.LogWarning($"[PeopleManager] No people found for tag '{tagHole}'");
            return;
        }

        numberFinishHole = 0;
        numberContain = 0;
        var fhCtrl = FinishHoleManager.Instance.FinishHoleCtrl;

        if (fhCtrl.MainHoleLeft != null && fhCtrl.MainHoleLeft.tag.Equals(peopleRunEvent.tag, StringComparison.OrdinalIgnoreCase))
            numberFinishHole += fhCtrl.LeftFHInfo.holeBlankGroups;
        if (fhCtrl.MainHoleRight != null && fhCtrl.MainHoleRight.tag.Equals(peopleRunEvent.tag, StringComparison.OrdinalIgnoreCase))
            numberFinishHole += fhCtrl.RightFHInfo.holeBlankGroups;

        var containWithTag = ContainManager.Instance.ContainArrangements
            .FirstOrDefault(c => c.TagContain == tagHole && c.ContainBlank > 0);
        if (containWithTag != null)
        {
            numberContain += containWithTag.ContainBlank;
        }
        else
        {
            var emptyContain = ContainManager.Instance.ContainArrangements
                .FirstOrDefault(c => c.TagContain == Tag.None && c.ContainBlank > 0);
            if (emptyContain != null)
                numberContain += 8;
        }

        PeopleManager.Instance.GroupPeopleFindHole(group.groupPeople, peopleRunEvent.target, numberGroup);
    }

    private void OnEntryHoleTouch(IEventParam param)
    {
        // Xác định các parent đã hoàn thành di chuyển và đưa họ vào kết thúc game
        if (param is not OnEntryHoleTouch entryEvent) return;
        if (!Enum.TryParse(entryEvent.tag, true, out Tag tagHole)) return;

        var uniqueParents = new HashSet<Transform>();
        var parentsToFinish = new List<GameObject>();

        foreach (var obj in entryEvent.groupParentPeople)
        {
            if (obj?.transform.parent is not Transform parent || !uniqueParents.Add(parent))
                continue;

            var children = parent.GetComponentsInChildren<PeopleMovement>(true);
            if (children.All(m => m != null && m.Moved))
                parentsToFinish.Add(parent.gameObject);
        }

        if (parentsToFinish.Count > 0)
            BringIntoFinishGame(parentsToFinish);
    }

    private void OnChangeMainHole(IEventParam param)
    {
        // Cập nhật lại tag các hố chính và container khi hố chính thay đổi
        mainFinishHoles = FinishHoleManager.Instance.FinishHoleCtrl.ExportTagFinishHole();
        containes = ContainManager.Instance.ExportTagContain();
        PutContainToFinishHole();
    }

    public void BringIntoFinishGame(List<GameObject> listParents)
    {
        // Di chuyển các parent khỏi nhóm In-Game, thêm vào nhóm Finish-Game, sau đó xử lý hố hoặc container
        if (listParents == null || listParents.Count == 0) return;

        groupedPeopleCtrl.RemoveFromGroup(groupedPeopleCtrl.GroupedPeopleInGame, listParents);
        var tagString = listParents[0].tag;
        if (!Enum.TryParse(tagString, out Tag tag)) return;

        groupedPeopleCtrl.AddToGroup(groupedPeopleCtrl.GroupedPeopleFinishGame, tag, listParents);
        PutInFinishHoleOrContain(tag, listParents);
    }

    private void PutInFinishHoleOrContain(Tag tag, List<GameObject> parents)
    {
        // Đưa các parent vào hố chính trước nếu còn chỗ, sau đó vào container nếu cần
        if (parents == null || parents.Count == 0) return;

        if (mainFinishHoles.Contains(tag) && numberFinishHole > 0)
        {
            int finishCount = Mathf.Min(numberFinishHole, parents.Count);
            var finishList = parents.Take(finishCount).ToList();
            numberFinishHole -= finishCount;

            FinishHoleManager.Instance.PutPeopleFromEntryHoleToFinishHole(finishList);
            groupedPeopleCtrl.RemoveFromGroup(groupedPeopleCtrl.GroupedPeopleFinishGame, finishList);
            parents.RemoveAll(p => finishList.Contains(p));
        }

        if (parents.Count > 0 && containes.Any(t => t == tag || t == Tag.None) && numberContain > 0)
        {
            int containCount = Mathf.Min(numberContain, parents.Count);
            var containList = parents.Take(containCount).ToList();
            numberContain -= containCount;

            ContainManager.Instance.PutPeopleInContain(tag, containList);
            groupedPeopleCtrl.RemoveFromGroup(groupedPeopleCtrl.GroupedPeopleFinishGame, containList);
            parents.RemoveAll(p => containList.Contains(p));
        }

        mainFinishHoles = FinishHoleManager.Instance.FinishHoleCtrl.ExportTagFinishHole();
        containes = ContainManager.Instance.ExportTagContain();
    }

    private void PutContainToFinishHole()
    {
        // Chuyển nhóm từ container vào hố chính khi tag khớp và còn chỗ trống
        Tag matchTag = Tag.None;
        for (int i = 0; i < mainFinishHoles.Length; i++)
        {
            if (mainFinishHoles[i] == Tag.None) continue;
            if (containes.Contains(mainFinishHoles[i]))
            {
                matchTag = mainFinishHoles[i];
                break;
            }
        }
        if (matchTag == Tag.None) return;

        int mainIndex = Array.IndexOf(mainFinishHoles, matchTag);
        int containIndex = Array.IndexOf(containes, matchTag);
        if (mainIndex < 0 || containIndex < 0) return;

        int mainBlank = mainIndex == 0
            ? FinishHoleManager.Instance.FinishHoleCtrl.LeftFHInfo.holeBlankGroups
            : FinishHoleManager.Instance.FinishHoleCtrl.RightFHInfo.holeBlankGroups;
        var containerData = ContainManager.Instance.ContainArrangements[containIndex];
        int containCount = containerData.GroupPeople.Count;
        if (mainBlank <= 0 || containCount == 0) return;

        int takeCount = Mathf.Min(mainBlank, containCount);
        var toMove = containerData.GroupPeople.GetRange(containCount - takeCount, takeCount);
        containerData.GroupPeople.RemoveRange(containCount - takeCount, takeCount);

        if (mainIndex == 0)
        {
            var info = FinishHoleManager.Instance.FinishHoleCtrl.LeftFHInfo;
            info.holeBlankGroups -= takeCount;
            FinishHoleManager.Instance.PutPeopleFromContainToFinishHole(toMove, Side.Left);
        }
        else
        {
            var info = FinishHoleManager.Instance.FinishHoleCtrl.RightFHInfo;
            info.holeBlankGroups -= takeCount;
            FinishHoleManager.Instance.PutPeopleFromContainToFinishHole(toMove, Side.Right);
        }
    }
}

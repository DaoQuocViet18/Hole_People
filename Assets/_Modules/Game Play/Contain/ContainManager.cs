using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ContainManager : Singleton<ContainManager>
{
    [SerializeField] private ContainArrangement[] containArrangements;
    [SerializeField] private ContainEndGame containEndGame;
    public ContainArrangement[] ContainArrangements { get => containArrangements; set => containArrangements = value; }
    public ContainEndGame ContainEndGame { get => containEndGame; set => containEndGame = value; }

    protected override void LoadComponents()
    {
        // Your implementation
        LoadContainArrangement();
        LoadContainEndGame();
    }

    void LoadContainArrangement()
    {
        // Lấy tất cả ContainArrangement trong con cháu của đối tượng này
        containArrangements = GetComponentsInChildren<ContainArrangement>(true);

        // Sắp xếp theo thứ tự sibling index của từng transform
        Array.Sort(containArrangements, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
    }

    void LoadContainEndGame()
    {
        if (containEndGame == null)
        {
            // Lấy ContainEndGame đầu tiên trong con cháu
            containEndGame = GetComponentInChildren<ContainEndGame>(true);
        }
    }

    public Tag[] ExportTagContain()
    {
        return containArrangements
            .Select(c => c.TagContain)
            .Concat(Enumerable.Repeat(Tag.None, 4)) // Đảm bảo đủ 4 phần tử
            .Take(4)
            .ToArray();
    }

    public void PutPeopleInContain(Tag tagPeople, GameObject parentObj)
    {
        // Ưu tiên chứa đúng tag
        ContainArrangement targetContain = containArrangements.FirstOrDefault(c => c.TagContain == tagPeople && c.ContainBlank > 0);
        // Nếu không có, chọn container chưa gán tag
        if (targetContain == null)
            targetContain = containArrangements.FirstOrDefault(c => c.TagContain == Tag.None);

        // Nếu vẫn không có, cho vào end game
        if (targetContain != null)
        {
            targetContain.TagContain = tagPeople;
            targetContain.GroupPeople.Add(parentObj);
            targetContain.Arrangement();
        }
        else if (containEndGame != null)
        {
            containEndGame.GroupPeople.Add(parentObj);
            containEndGame.Arrangement();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy nơi để chứa người.");
        }
    }

}

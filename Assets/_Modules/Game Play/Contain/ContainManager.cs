using System;
using System.Collections.Generic;
using System.Linq;
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

    public bool CheckTagContain(Tag tag)
    {
        foreach (var contain in containArrangements)
        {
            if (contain.TagContain == tag || contain.TagContain == Tag.None)
                return true;
        }
        return false;
    }

    public void PutPeopleInContain(Tag tagPeople, GameObject parentObj)
    {
        if (parentObj == null) return;

        List<GameObject> listObj = new();

        PeopleMovement[] peopleMovements = parentObj.GetComponentsInChildren<PeopleMovement>(true);

        foreach (var movement in peopleMovements)
        {
            if (movement != null)
                listObj.Add(movement.gameObject);
        }

        if (listObj.Count == 0) return;

        // Ưu tiên chứa đúng tag
        ContainArrangement targetContain = containArrangements.FirstOrDefault(c => c.TagContain == tagPeople);
        // Nếu không có, chọn container chưa gán tag
        if (targetContain == null)
            targetContain = containArrangements.FirstOrDefault(c => c.TagContain == Tag.None);

        // Nếu vẫn không có, cho vào end game
        if (targetContain != null)
        {
            targetContain.TagContain = tagPeople;
            targetContain.People.AddRange(listObj);
            targetContain.Arrangement();
        }
        else if (containEndGame != null)
        {
            containEndGame.People.AddRange(listObj);
            containEndGame.Arrangement();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy nơi để chứa người.");
        }
    }

}

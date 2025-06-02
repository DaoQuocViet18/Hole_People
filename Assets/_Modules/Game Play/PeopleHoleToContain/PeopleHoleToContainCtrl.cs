using System;
using UnityEngine;

[RequireComponent(typeof(PeopleHoleToContainManager))]
public class PeopleHoleToContainCtrl : CtrlMonoBehaviour
{
    [SerializeField] private PeopleHoleToContainManager peopleHoleToContain;
    [SerializeField] private HoleTouch[] hole;
    [SerializeField] private ContainArrangement[] containArrangements;
    [SerializeField] private ContainEndGame containEndGame;

    public PeopleHoleToContainManager PeopleHoleToContain { get => peopleHoleToContain; set => peopleHoleToContain = value; }
    public HoleTouch[] Hole { get => hole; set => hole = value; }
    public ContainArrangement[] ContainArrangements { get => containArrangements; set => containArrangements = value; }
    public ContainEndGame ContainEndGame { get => containEndGame; set => containEndGame = value; }

    protected override void LoadComponents()
    {
        LoadPeopleHoleToContain();
        LoadHoleTouch();
        LoadPeopleHoleToContain();
        LoadContainEndGame();
    }

    void LoadHoleTouch() 
    {
        if (PeopleHoleToContain == null)
            PeopleHoleToContain = GetComponent<PeopleHoleToContainManager>();
    }

    void LoadPeopleHoleToContain()
    {
        if (Hole == null || Hole.Length == 0)
            Hole = UnityEngine.Object.FindObjectsByType<HoleTouch>(FindObjectsSortMode.None);
    }

    void LoadContainArrangement()
    {
        containArrangements = UnityEngine.Object.FindObjectsByType<ContainArrangement>(FindObjectsSortMode.None);
        Array.Sort(containArrangements, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
    }

    void LoadContainEndGame()
    {
        if (ContainEndGame == null)
        {
            ContainEndGame[] foundObjects = UnityEngine.Object.FindObjectsByType<ContainEndGame>(FindObjectsSortMode.None);
            if (foundObjects.Length > 0)
                ContainEndGame = foundObjects[0]; // Lấy object đầu tiên tìm được
        }
    }
}

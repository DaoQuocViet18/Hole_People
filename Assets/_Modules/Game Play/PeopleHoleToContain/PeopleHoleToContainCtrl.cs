using System;
using UnityEngine;

[RequireComponent(typeof(PeopleHoleToContainManager))]
public class PeopleHoleToContainCtrl : Singleton<PeopleHoleToContainCtrl>, ICtrl
{
    [SerializeField] private PeopleHoleToContainManager peopleHoleToContainManager;
    [SerializeField] private HoleTouch[] hole;
    [SerializeField] private ContainArrangement[] containArrangements;
    [SerializeField] private ContainEndGame containEndGame;

    public PeopleHoleToContainManager PeopleHoleToContainManager { get => peopleHoleToContainManager; set => peopleHoleToContainManager = value; }
    public HoleTouch[] Hole { get => hole; set => hole = value; }
    public ContainArrangement[] ContainArrangements { get => containArrangements; set => containArrangements = value; }
    public ContainEndGame ContainEndGame { get => containEndGame; set => containEndGame = value; }

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
        LoadPeopleHoleToContain();
        LoadHoleTouch();
        LoadContainArrangement();
        LoadContainEndGame();
    }

    public void ResetValue()
    {
        // Your implementation
    }

    public void Init()
    {
        // Your implementation
    }

    void LoadHoleTouch() 
    {
        if (PeopleHoleToContainManager == null)
            PeopleHoleToContainManager = GetComponent<PeopleHoleToContainManager>();
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

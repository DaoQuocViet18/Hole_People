using System;
using UnityEngine;

[RequireComponent(typeof(PeopleFindHole))]
[RequireComponent(typeof(PeopleController))]
public class GroupPeopleMovementCtrl : MonoBehaviour, ICtrl
{
    private PeopleFindHole peopleFindHole;
    private PeopleController peopleController;
    private PeopleMovement[] peopleMovements;

    public PeopleFindHole PeopleFindHole { get => peopleFindHole; set => peopleFindHole = value; }
    public PeopleController PeopleController { get => peopleController; set => peopleController = value; }
    public PeopleMovement[] PeopleMovements { get => peopleMovements; set => peopleMovements = value; }

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
        LoadPeopleFindHole();
        LoadPeopleController();
        LoadPeopleMovement();
    }

    public void ResetValue()
    {
        // Your implementation
    }

    public void Init()
    {
        // Your implementation
    }

    void LoadPeopleFindHole()
    {
        if (PeopleFindHole == null)
            PeopleFindHole = GetComponent<PeopleFindHole>();
    }

    void LoadPeopleController()
    {
        if (PeopleController == null)
            PeopleController = GetComponent<PeopleController>();
    }

    void LoadPeopleMovement()
    {
        if (PeopleMovements == null || PeopleMovements.Length == 0)
            PeopleMovements = GetComponentsInChildren<PeopleMovement>();
    }
}

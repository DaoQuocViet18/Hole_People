using UnityEngine;

[RequireComponent(typeof(PeopleFindHole))]
[RequireComponent(typeof(PeopleController))]
public class GroupPeopleMovementCtrl : CtrlMonoBehaviour
{
    private PeopleFindHole peopleFindHole;
    private PeopleController peopleController;
    private PeopleMovement[] peopleMovements;

    public PeopleFindHole PeopleFindHole { get => peopleFindHole; set => peopleFindHole = value; }
    public PeopleController PeopleController { get => peopleController; set => peopleController = value; }
    public PeopleMovement[] PeopleMovements { get => peopleMovements; set => peopleMovements = value; }

    protected override void LoadComponents()
    {
        if (PeopleFindHole == null)
            PeopleFindHole = GetComponent<PeopleFindHole>();

        if (PeopleController == null)
            PeopleController = GetComponent<PeopleController>();

        if (PeopleMovements == null || PeopleMovements.Length == 0)
            PeopleMovements = GetComponentsInChildren<PeopleMovement>();
    }


}

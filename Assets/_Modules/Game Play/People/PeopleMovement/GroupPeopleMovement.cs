using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PeopleFindHole))]
public class GroupPeopleMovement : MonoBehaviour
{
    private PeopleFindHole peopleFindHole;
    private PeopleMovement[] peopleMovements;

    public PeopleFindHole PeopleFindHole { get => peopleFindHole; set => peopleFindHole = value; }
    public PeopleMovement[] PeopleMovements { get => peopleMovements; set => peopleMovements = value; }

    private void Awake()
    {
        LoadComponents();
    }

    private void Reset()
    {
        LoadComponents();
    }

    public void LoadComponents()
    {
        // Your implementation
        LoadPeopleFindHole();
        LoadPeopleMovement();
    }

    void LoadPeopleFindHole()
    {
        if (PeopleFindHole == null)
            PeopleFindHole = GetComponent<PeopleFindHole>();
    }

    void LoadPeopleMovement()
    {
        if (PeopleMovements == null || PeopleMovements.Length == 0)
            PeopleMovements = GetComponentsInChildren<PeopleMovement>();
    }

    public List<Node> ListGroupWay(Node targetNode)
    {
        return peopleFindHole.FindHole(targetNode);
    }

    public void PeopleMovement(List<Node> movingNodes)
    {
        StartCoroutine(MovePeopleThroughNodes(movingNodes));
    }

    private IEnumerator MovePeopleThroughNodes(List<Node> movingNodes)
    {
        foreach (var people in PeopleMovements)
        {
            people.Moving(movingNodes);

            yield return new WaitForSeconds(0.5f);
        }
    }
}

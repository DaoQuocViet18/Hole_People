using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleController : MonoBehaviour
{
    [Header("People Settings")]
    [SerializeField] private PeopleMovement[] peopleMovements;

    private void Awake()
    {
        if (peopleMovements == null || peopleMovements.Length == 0)
            peopleMovements = GetComponentsInChildren<PeopleMovement>();
    }

    private void Reset()
    {
        if (peopleMovements == null || peopleMovements.Length == 0)
            peopleMovements = GetComponentsInChildren<PeopleMovement>();
    }

    public void MovePeople(List<Node> movingNodes)
    {
        StartCoroutine(MovePeopleThroughNodes(movingNodes));
    }

    private IEnumerator MovePeopleThroughNodes(List<Node> movingNodes)
    {
        foreach (var people in peopleMovements)
        {
            people.Moving(movingNodes);

            yield return new WaitForSeconds(0.5f);
        }
    }
}

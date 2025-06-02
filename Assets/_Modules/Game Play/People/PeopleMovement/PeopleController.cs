using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleController : MonoBehaviour
{
    [Header("People Settings")]
    [SerializeField] private GroupPeopleMovementCtrl groupPeopleController;

    private void Awake()
    {
        LoadComponents();
    }

    private void Reset()
    {
        LoadComponents();
    }

    void LoadComponents()
    {
        if (groupPeopleController == null)
            groupPeopleController = GetComponentInParent<GroupPeopleMovementCtrl>();
    }

    public void MovePeople(List<Node> movingNodes)
    {
        StartCoroutine(MovePeopleThroughNodes(movingNodes));
    }

    private IEnumerator MovePeopleThroughNodes(List<Node> movingNodes)
    {
        foreach (var people in groupPeopleController.PeopleMovements)
        {
            people.Moving(movingNodes);

            yield return new WaitForSeconds(0.5f);
        }
    }
}

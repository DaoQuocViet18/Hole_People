using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PeopleHoleToContainManager : MonoBehaviour
{
    private void Start()
    {
        foreach (var hole in PeopleHoleToContainCtrl.Instance.Hole)
        {
            hole.OnAllPeopleEntered += HandleAllPeopleEnteredHole;
        }
    }

    private void HandleAllPeopleEnteredHole(Tag tagPeople, List<GameObject> despawnedObjects)
    {
        List<GameObject> matchedPeople = GetPeopleFromContain(despawnedObjects);

        if (matchedPeople.Count == 0)
            return;

        //bool moved = FinishHoleManager.Instance.EntryHoleToFinishHole(despawnedObjects);
        //if (moved)
        //{
        //    return;
        //}

        ContainArrangement targetContain = FindOrAssignContain(tagPeople);

        if (targetContain != null)
        {
            targetContain.People.AddRange(matchedPeople);
            targetContain.Arrangement();
        }
        else if (PeopleHoleToContainCtrl.Instance.ContainEndGame != null)
        {
            PeopleHoleToContainCtrl.Instance.ContainEndGame.People.AddRange(matchedPeople);
            PeopleHoleToContainCtrl.Instance.ContainEndGame.Arrangement();
        }
    }

    private List<GameObject> GetPeopleFromContain(List<GameObject> despawnedObjects)
    {
        return PeopleSpawnManager.Instance.GroupsInContain
            .SelectMany(group => group.people)
            .Where(person => despawnedObjects.Contains(person))
            .ToList();
    }

    private ContainArrangement FindOrAssignContain(Tag incomingTag)
    {
        foreach (var contain in PeopleHoleToContainCtrl.Instance.ContainArrangements)
        {
            if (contain.TagContain == incomingTag && contain.People.Count < 32)
                return contain;
        }

        foreach (var contain in PeopleHoleToContainCtrl.Instance.ContainArrangements)
        {
            if (contain.TagContain == Tag.None)
            {
                contain.TagContain = incomingTag;
                return contain;
            }
        }

        return null;
    }
}

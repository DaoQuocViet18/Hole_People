using System;
using System.Collections.Generic;
using UnityEngine;

public class PeopleHoleToContainManager : MonoBehaviour
{
    [SerializeField] private PeopleHoleToContainCtrl peopleHoleToContainCtrl;

    private void Reset()
    {
        LoadComponent();
    }

    private void Awake()
    {
        LoadComponent();
    }

    private void LoadComponent()
    {
        if (peopleHoleToContainCtrl == null)
            peopleHoleToContainCtrl = GetComponent<PeopleHoleToContainCtrl>();

        if (peopleHoleToContainCtrl == null)
        {
            Debug.LogError("Missing PeopleHoleToContainCtrl component.");
            return;
        }

        foreach (var hole in peopleHoleToContainCtrl.Hole)
        {
            hole.OnAllPeopleEntered += HandleAllPeopleEnteredHole;
        }
    }

    private void HandleAllPeopleEnteredHole()
    {
        List<GameObject> inactivePeople = GetInactivePeopleFromContain();

        if (inactivePeople.Count == 0)
        {
            //Debug.LogWarning("No inactive people found in GroupsInContain.");
            return;
        }

        string groupTag = inactivePeople[0].tag;

        if (!Enum.TryParse(groupTag, out Tag incomingTag))
        {
            //Debug.LogWarning($"Failed to parse tag '{groupTag}' to Tag enum.");
            return;
        }

        ContainArrangement targetContain = FindOrAssignContain(incomingTag);

        if (targetContain != null)
        {
            targetContain.newPeople.AddRange(inactivePeople);
            targetContain.Arrangement();
            //Debug.Log($"Assigned {inactivePeople.Count} people to Contain with tag: {targetContain.tagContain}");
        }
        else if (peopleHoleToContainCtrl.ContainEndGame != null)
        {
            peopleHoleToContainCtrl.ContainEndGame.newPeople.AddRange(inactivePeople);
            peopleHoleToContainCtrl.ContainEndGame.Arrangement();
            //Debug.Log($"Assigned {inactivePeople.Count} people to ContainEndGame with tag: {incomingTag}");
        }
        else
        {
            //Debug.LogWarning($"No suitable ContainArrangement or ContainEndGame found for tag: {incomingTag}");
        }
    }

    private List<GameObject> GetInactivePeopleFromContain()
    {
        List<GameObject> result = new List<GameObject>();

        foreach (PeopleGroup group in PeopleSpawnManager.Instance.GroupsInContain)
        {
            foreach (GameObject person in group.people)
            {
                if (!person.activeSelf)
                {
                    result.Add(person);
                }
            }
        }

        return result;
    }

    private ContainArrangement FindOrAssignContain(Tag incomingTag)
    {
        foreach (var contain in peopleHoleToContainCtrl.ContainArrangements)
        {
            if (contain.tagContain == incomingTag && contain.newPeople.Count < 32)
                return contain;
        }

        foreach (var contain in peopleHoleToContainCtrl.ContainArrangements)
        {
            if (contain.tagContain == Tag.None)
            {
                contain.tagContain = incomingTag;
                return contain;
            }
        }

        return null;
    }
}

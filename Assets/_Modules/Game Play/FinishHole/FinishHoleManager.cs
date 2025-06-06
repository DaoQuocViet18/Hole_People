using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EventDefine;

[RequireComponent(typeof(FinishHoleCtrl))]
public class FinishHoleManager : Singleton<FinishHoleManager>
{
    [SerializeField] private FinishHoleCtrl finishHoleCtrl;
    public FinishHoleCtrl FinishHoleCtrl { get => finishHoleCtrl; set => finishHoleCtrl = value; }

    // Initialize the finishHoleCtrl reference
    protected override void LoadComponents()
    {
        base.LoadComponents();
        if (FinishHoleCtrl == null)
            FinishHoleCtrl = GetComponent<FinishHoleCtrl>();
    }

    // Gather all PeopleMovement children, choose the correct hole by tag, and start entry-hole coroutine
    public void PutPeopleFromEntryHoleToFinishHole(List<GameObject> parentObjs)
    {
        if (parentObjs == null || parentObjs.Count == 0)
            return;

        foreach (var parentObj in parentObjs)
        {
            if (parentObj == null)
                continue;

            var people = parentObj
                .GetComponentsInChildren<PeopleMovement>(true)
                .Where(move => move != null && move.gameObject != null)
                .Select(move => move.gameObject)
                .ToList();

            if (people.Count == 0)
                continue;

            string tag = parentObj.tag;
            if (string.IsNullOrEmpty(tag))
                tag = people[0].tag;

            bool isLeft = finishHoleCtrl.MainHoleLeft != null
                          && finishHoleCtrl.MainHoleLeft.tag == tag
                          && finishHoleCtrl.LeftFHInfo.holeBlankGroups > 0;
            bool isRight = finishHoleCtrl.MainHoleRight != null
                           && finishHoleCtrl.MainHoleRight.tag == tag
                           && finishHoleCtrl.RightFHInfo.holeBlankGroups > 0;

            if (isLeft)
                StartCoroutine(MovePeopleFromEntryHoleToFinishHoleCoroutine(
                    finishHoleCtrl.MainHoleLeft, people, Side.Left));
            else if (isRight)
                StartCoroutine(MovePeopleFromEntryHoleToFinishHoleCoroutine(
                    finishHoleCtrl.MainHoleRight, people, Side.Right));
        }
    }

    // Sequentially move each person above the hole and call fall animation with a small delay, then trigger hole change
    private IEnumerator MovePeopleFromEntryHoleToFinishHoleCoroutine(GameObject mainHole, List<GameObject> people, Side side)
    {
        Vector3 holePos = mainHole.transform.position;
        float delay = 0f;

        foreach (var person in people)
        {
            yield return new WaitForSeconds(delay);
            if (person == null)
                continue;

            person.transform.position = holePos + Vector3.up * 2f;
            if (person.TryGetComponent(out PeopleMovement move))
                move.FallIntoHole(holePos);
            else
                Debug.LogWarning($"{person.name} missing PeopleMovement component.");

            delay += 0.05f;
        }

        StartCoroutine(finishHoleCtrl.ChangeMainHoleByEntryHole(side));
    }

    // Aggregate people by side from container parents, then call a single coroutine per side
    public void PutPeopleFromContainToFinishHole(List<GameObject> parentObjs, Side side)
    {
        //Debug.Log("parentObjs = " + parentObjs.Count);
        if (parentObjs == null || parentObjs.Count == 0)
            return;

        var peopleToLeft = new List<GameObject>();
        var peopleToRight = new List<GameObject>();

        foreach (var parentObj in parentObjs)
        {
            if (parentObj == null)
                continue;

            var people = parentObj
                .GetComponentsInChildren<PeopleMovement>(true)
                .Where(move => move != null && move.gameObject != null)
                .Select(move => move.gameObject)
                .ToList();

            if (people.Count == 0)
                continue;

            string tagString = parentObj.tag;
            if (string.IsNullOrEmpty(tagString))
                tagString = people[0].tag;

            if (finishHoleCtrl.MainHoleLeft != null && finishHoleCtrl.MainHoleLeft.tag == tagString)
                peopleToLeft.AddRange(people);
            else if (finishHoleCtrl.MainHoleRight != null && finishHoleCtrl.MainHoleRight.tag == tagString)
                peopleToRight.AddRange(people);
        }

        if (peopleToLeft.Count > 0)
        {
            StartCoroutine(MovePeopleFromContainToFinishHoleCoroutine(
                finishHoleCtrl.MainHoleLeft, peopleToLeft, Side.Left));
        }

        if (peopleToRight.Count > 0)
        {
            StartCoroutine(MovePeopleFromContainToFinishHoleCoroutine(
                finishHoleCtrl.MainHoleRight, peopleToRight, Side.Right));
        }
    }

    // Move people toward the hole with staggered starts, wait until all have deactivated, then trigger hole change
    private IEnumerator MovePeopleFromContainToFinishHoleCoroutine(GameObject mainHole, List<GameObject> people, Side side)
    {
        Vector3 holePos = mainHole.transform.position;
        float delayBetweenStarts = 0.05f;
        var movers = new List<GameObject>();

        foreach (var person in people)
        {
            if (person.TryGetComponent(out PeopleMovement pm))
            {
                StartCoroutine(pm.MovingTowards(holePos));
                movers.Add(person);
                yield return new WaitForSeconds(delayBetweenStarts);
            }
            else
            {
                Debug.LogWarning($"{person.name} missing PeopleMovement component.");
            }
        }

        yield return new WaitUntil(() => movers.All(obj => obj == null || !obj.activeSelf));
        StartCoroutine(finishHoleCtrl.ChangeMainHoleByCotain(side));
    }
}

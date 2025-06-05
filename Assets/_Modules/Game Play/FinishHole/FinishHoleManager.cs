using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventDefine;

[RequireComponent(typeof(FinishHoleCtrl))]
public class FinishHoleManager : Singleton<FinishHoleManager>
{
    [SerializeField] private FinishHoleCtrl finishHoleCtrl;

    public FinishHoleCtrl FinishHoleCtrl { get => finishHoleCtrl; set => finishHoleCtrl = value; }

    protected override void LoadComponents()
    {
        base.LoadComponents();

        if (FinishHoleCtrl == null)
            FinishHoleCtrl = GetComponent<FinishHoleCtrl>();
    }

    // Gather all PeopleMovement children, choose the correct hole by tag, then start the corresponding coroutine
    public void PutPeopleInFinishHole(GameObject parentObj, Kind startKind)
    {
        if (parentObj == null) return;

        List<GameObject> people = new List<GameObject>();
        foreach (var move in parentObj.GetComponentsInChildren<PeopleMovement>(true))
            people.Add(move.gameObject);

        if (people.Count == 0) return;

        string tag = parentObj.tag;
        if (string.IsNullOrEmpty(tag))
            tag = people[0].tag;

        bool isLeft = finishHoleCtrl.MainHoleLeft != null && finishHoleCtrl.MainHoleLeft.tag == tag && finishHoleCtrl.LeftFHInfo.holeBlankGroups > 0;
        bool isRight = finishHoleCtrl.MainHoleRight != null && finishHoleCtrl.MainHoleRight.tag == tag && finishHoleCtrl.RightFHInfo.holeBlankGroups > 0;

        if (isLeft)
            StartCoroutine(PutPeopleThenMaybeChangeSide(finishHoleCtrl.MainHoleLeft, people, startKind, Side.Left));
         else if (isRight)
            StartCoroutine(PutPeopleThenMaybeChangeSide(finishHoleCtrl.MainHoleRight, people, startKind, Side.Right));
    }

    // Coroutine for either side: wait for all people to finish moving, decrement holeBlankGroups, then animate hole and change if needed
    private IEnumerator PutPeopleThenMaybeChangeSide(GameObject hole, List<GameObject> people, Kind startKind, Side side)
    {
        if (startKind == Kind.EntryHole)
            yield return StartCoroutine(MovePeopleFromEntryHoleToFinishHoleCoroutine(hole, people));
        else if (startKind == Kind.Contain)
            yield return StartCoroutine(MovePeopleFromContainToFinishHoleCoroutine(hole, people));

        StartCoroutine(finishHoleCtrl.ChangeMainHole(hole, side));
    }

    // Sequentially move each person above the hole and call their fall animation with a small increasing delay
    private IEnumerator MovePeopleFromEntryHoleToFinishHoleCoroutine(GameObject mainHole, List<GameObject> people)
    {
        Vector3 holePos = mainHole.transform.position;
        float t = 0f;

        for (int i = 0; i < people.Count; i++)
        {
            yield return new WaitForSeconds(t);

            GameObject person = people[i];
            person.transform.position = holePos + Vector3.up * 2f;

            if (person.TryGetComponent(out PeopleMovement move))
                move.FallIntoHole(holePos);
            else
                Debug.LogWarning($"{person.name} missing PeopleMovement component.");

            t += 0.05f;
        }
    }

    // FinishHoleCtrl.cs (hoặc file chứa coroutine)

    private IEnumerator MovePeopleFromContainToFinishHoleCoroutine(GameObject mainHole, List<GameObject> people)
    {
        Vector3 holePos = mainHole.transform.position;
        float delayBetweenStarts = 0.05f;
        List<GameObject> movers = new List<GameObject>();

        for (int i = 0; i < people.Count; i++)
        {
            GameObject person = people[i];
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

        // Đợi đến khi tất cả GameObject đã bị SetActive(false)
        yield return new WaitUntil(() =>
        {
            foreach (GameObject obj in movers)
            {
                if (obj != null && obj.activeSelf) return false;
            }
            return true;
        });
    }

}

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishHoleManager : Singleton<FinishHoleManager>
{
    [Header("Main Holes")]
    [SerializeField] private GameObject mainHoleLeft;
    [SerializeField] private GameObject mainHoleRight;
    [SerializeField] private float moveDuration = 0.5f;

    [Header("Finish Hole Info")]
    [SerializeField] private FinishHoleInfo leftFHInfo;
    [SerializeField] private FinishHoleInfo rightFHInfo;

    [Header("Finish Hole Spawn Info")]
    [SerializeField] private FinishHoleSpawnInfo leftFHSpawnInfo;
    [SerializeField] private FinishHoleSpawnInfo rightFHSpawnInfo;

    public GameObject MainHoleLeft => mainHoleLeft;
    public GameObject MainHoleRight => mainHoleRight;
    public FinishHoleInfo LeftFHInfo => leftFHInfo;
    public FinishHoleInfo RightFHInfo => rightFHInfo;
    public FinishHoleSpawnInfo LeftFHSpawnInfo => leftFHSpawnInfo;
    public FinishHoleSpawnInfo RightFHSpawnInfo => rightFHSpawnInfo;

    // Initialize default values when resetting the component
    protected override void ResetValue()
    {
        leftFHInfo.holeBlankGroups = 4;
        leftFHSpawnInfo.StartPosition = new Vector3(0, 0, -35);
        leftFHSpawnInfo.SpacingZ = 7f;

        rightFHInfo.holeBlankGroups = 4;
        rightFHSpawnInfo.StartPosition = new Vector3(-10, 0, -35);
        rightFHSpawnInfo.SpacingZ = 7f;
    }

    // Spawn holes from prefabs and assign the first as mainHole
    private void Start()
    {
        leftFHInfo.holeInstances = SpawnHoles(leftFHInfo.holePrefabs, leftFHSpawnInfo);
        rightFHInfo.holeInstances = SpawnHoles(rightFHInfo.holePrefabs, rightFHSpawnInfo);

        if (leftFHInfo.holeInstances.Count > 0)
            mainHoleLeft = leftFHInfo.holeInstances[0];
        if (rightFHInfo.holeInstances.Count > 0)
            mainHoleRight = rightFHInfo.holeInstances[0];
    }

    // Create and return instances of hole prefabs based on spawn config
    private List<GameObject> SpawnHoles(List<GameObject> prefabs, FinishHoleSpawnInfo config)
    {
        var instances = new List<GameObject>();
        Vector3 pos = config.StartPosition;

        foreach (var prefab in prefabs)
        {
            var instance = Instantiate(prefab, pos, Quaternion.identity);
            instances.Add(instance);
            pos.z -= config.SpacingZ;
        }

        return instances;
    }

    // Return the tags of the two current main holes
    public Tag[] ExportTagFinishHole()
    {
        Tag leftTag = Tag.None;
        Tag rightTag = Tag.None;

        if (mainHoleLeft != null && System.Enum.TryParse(mainHoleLeft.tag, out Tag parsedLeft))
            leftTag = parsedLeft;
        if (mainHoleRight != null && System.Enum.TryParse(mainHoleRight.tag, out Tag parsedRight))
            rightTag = parsedRight;

        return new[] { leftTag, rightTag };
    }

    // Gather all PeopleMovement children, choose the correct hole by tag, then start the corresponding coroutine
    public void PutPeopleInFinishHole(GameObject parentObj, Kind startPosition)
    {
        if (parentObj == null) return;

        List<GameObject> people = new List<GameObject>();
        foreach (var move in parentObj.GetComponentsInChildren<PeopleMovement>(true))
            people.Add(move.gameObject);

        if (people.Count == 0) return;

        string tag = parentObj.tag;
        if (string.IsNullOrEmpty(tag))
            tag = people[0].tag;

        bool isLeft = mainHoleLeft != null && mainHoleLeft.tag == tag && leftFHInfo.holeBlankGroups > 0;
        bool isRight = mainHoleRight != null && mainHoleRight.tag == tag && rightFHInfo.holeBlankGroups > 0;

        if (isLeft)
            StartCoroutine(PutPeopleThenMaybeChangeSide(mainHoleLeft, people, startPosition, Side.Left));
         else if (isRight)
            StartCoroutine(PutPeopleThenMaybeChangeSide(mainHoleRight, people, startPosition, Side.Right));
    }

    // Coroutine for either side: wait for all people to finish moving, decrement holeBlankGroups, then animate hole and change if needed
    private IEnumerator PutPeopleThenMaybeChangeSide(GameObject hole, List<GameObject> people, Kind startPosition, Side side)
    {
        if (startPosition == Kind.EntryHole)
            yield return StartCoroutine(MovePeopleFromEntryHoleToFinishHoleCoroutine(hole, people));
        else if (startPosition == Kind.Contain)
            yield return StartCoroutine(MovePeopleFromContainToFinishHoleCoroutine(hole, people));

        if (side == Side.Left)
        {
            leftFHInfo.holeBlankGroups--;
            if (leftFHInfo.holeBlankGroups <= 0)
            {
                yield return StartCoroutine(EffectHoleEnd(hole, 0.5f));
                ChangeMainHole(ref mainHoleLeft, leftFHInfo, leftFHSpawnInfo, true);
            }
        }
        else // Side.Right
        {
            rightFHInfo.holeBlankGroups--;
            if (rightFHInfo.holeBlankGroups <= 0)
            {
                yield return StartCoroutine(EffectHoleEnd(hole, 0.5f));
                ChangeMainHole(ref mainHoleRight, rightFHInfo, rightFHSpawnInfo, false);
            }
        }
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

    private IEnumerator MovePeopleFromContainToFinishHoleCoroutine(GameObject mainHole, List<GameObject> people)
    {
        Vector3 holePos = mainHole.transform.position;
        float t = 0f;

        for (int i = 0; i < people.Count; i++)
        {
            yield return new WaitForSeconds(t);

            GameObject person = people[i];

            if (person.TryGetComponent(out PeopleMovement move))
                move.Moving(mainHole);
            else
                Debug.LogWarning($"{person.name} missing PeopleMovement component.");

            t += 0.05f;
        }
    }

    // Animate the hole scaling up and back down over the given duration
    private IEnumerator EffectHoleEnd(GameObject hole, float duration)
    {
        Vector3 originalScale = hole.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        Sequence seq = DOTween.Sequence();
        seq.Append(hole.transform.DOScale(targetScale, duration / 2f).SetEase(Ease.OutQuad));
        seq.Append(hole.transform.DOScale(originalScale, duration / 2f).SetEase(Ease.InQuad));

        yield return seq.WaitForCompletion();
    }

    // Replace the current hole: deactivate the old one, promote the next, move others into position, reset holeBlankGroups
    private void ChangeMainHole(ref GameObject currentHole, FinishHoleInfo side, FinishHoleSpawnInfo config, bool isLeft)
    {
        if (side.holeInstances.Count <= 1)
        {
            Debug.LogWarning("No more holes to replace.");
            currentHole.SetActive(false);
            return;
        }

        GameObject oldHole = currentHole;
        oldHole.SetActive(false);
        side.holeInstances.RemoveAt(0);

        GameObject newMain = side.holeInstances[0];
        if (!newMain.activeSelf)
            newMain.SetActive(true);

        Vector3 basePos = oldHole.transform.position;
        StartCoroutine(MoveToPosition(newMain, basePos, moveDuration));
        currentHole = newMain;

        if (isLeft)
            mainHoleLeft = newMain;
        else
            mainHoleRight = newMain;

        for (int i = 1; i < side.holeInstances.Count; i++)
        {
            var hole = side.holeInstances[i];
            if (!hole.activeSelf)
                hole.SetActive(true);

            Vector3 targetPos = new Vector3(
                basePos.x,
                basePos.y,
                basePos.z - config.SpacingZ * i
            );
            StartCoroutine(MoveToPosition(hole, targetPos, moveDuration));
        }

        Destroy(oldHole);
        side.holeBlankGroups = 4;
        EventDispatcher.Dispatch(new EventDefine.OnChangeMainHole());
    }

    // Tween an object from its current position to targetPos over duration seconds
    private IEnumerator MoveToPosition(GameObject obj, Vector3 targetPos, float duration)
    {
        Vector3 startPos = obj.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            obj.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = targetPos;
    }
}

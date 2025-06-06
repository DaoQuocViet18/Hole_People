using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishHoleCtrl : CtrlMonoBehaviour
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

    // Initialize default hole and spawn settings when resetting this component
    protected override void ResetValue()
    {
        leftFHInfo.holeBlankGroups = 4;
        leftFHSpawnInfo.StartPosition = new Vector3(0, 0, -35);
        leftFHSpawnInfo.SpacingZ = 7f;

        rightFHInfo.holeBlankGroups = 4;
        rightFHSpawnInfo.StartPosition = new Vector3(-10, 0, -35);
        rightFHSpawnInfo.SpacingZ = 7f;
    }

    // Spawn hole instances from provided prefabs and assign the first as mainHoleLeft and mainHoleRight
    private void Start()
    {
        leftFHInfo.holeInstances = SpawnHoles(leftFHInfo.holePrefabs, leftFHSpawnInfo);
        rightFHInfo.holeInstances = SpawnHoles(rightFHInfo.holePrefabs, rightFHSpawnInfo);

        if (leftFHInfo.holeInstances.Count > 0)
            mainHoleLeft = leftFHInfo.holeInstances[0];
        if (rightFHInfo.holeInstances.Count > 0)
            mainHoleRight = rightFHInfo.holeInstances[0];
    }

    // Instantiate a list of hole GameObjects based on prefab list and spawn configuration
    private List<GameObject> SpawnHoles(List<GameObject> prefabs, FinishHoleSpawnInfo config)
    {
        var instances = new List<GameObject>();
        var position = config.StartPosition;

        foreach (var prefab in prefabs)
        {
            var instance = Instantiate(prefab, position, Quaternion.identity);
            instances.Add(instance);
            position.z -= config.SpacingZ;
        }

        return instances;
    }

    // Return an array of Tags corresponding to current mainHoleLeft and mainHoleRight
    public Tag[] ExportTagFinishHole()
    {
        var leftTag = Tag.None;
        var rightTag = Tag.None;

        if (mainHoleLeft != null && System.Enum.TryParse(mainHoleLeft.tag, out Tag parsedLeft))
            leftTag = parsedLeft;
        if (mainHoleRight != null && System.Enum.TryParse(mainHoleRight.tag, out Tag parsedRight))
            rightTag = parsedRight;

        return new[] { leftTag, rightTag };
    }

    // Handle replacing the main hole when a group enters via an entry hole, decrementing holeBlankGroups or swapping holes
    public IEnumerator ChangeMainHoleByEntryHole(Side side)
    {
        var currentHole = (side == Side.Left) ? mainHoleLeft : mainHoleRight;
        var info = (side == Side.Left) ? leftFHInfo : rightFHInfo;
        var spawnInfo = (side == Side.Left) ? leftFHSpawnInfo : rightFHSpawnInfo;

        // If more than one blank group remains, decrement and exit
        if (info.holeBlankGroups > 1)
        {
            info.holeBlankGroups--;
            yield break;
        }

        // Play end-of-hole effect before swapping
        yield return StartCoroutine(EffectHoleEnd(currentHole, 0.5f));

        // If no replacement instances remain, exit
        if (info.holeInstances.Count <= 1)
            yield break;

        var basePos = currentHole.transform.position;

        // Deactivate and remove the current hole
        currentHole.SetActive(false);
        info.holeInstances.RemoveAt(0);

        // Promote the next instance as new main hole
        var newMain = info.holeInstances[0];
        if (!newMain.activeSelf)
            newMain.SetActive(true);

        // Move new main hole into the saved base position
        StartCoroutine(MoveToPosition(newMain, basePos, moveDuration));

        if (side == Side.Left)
            mainHoleLeft = newMain;
        else
            mainHoleRight = newMain;

        // Reposition remaining holes behind the new main
        for (var i = 1; i < info.holeInstances.Count; i++)
        {
            var hole = info.holeInstances[i];
            if (!hole.activeSelf)
                hole.SetActive(true);

            var targetPos = new Vector3(
                basePos.x,
                basePos.y,
                basePos.z - spawnInfo.SpacingZ * i
            );
            StartCoroutine(MoveToPosition(hole, targetPos, moveDuration));
        }

        Destroy(currentHole);
        info.holeBlankGroups = 4;
    }

    // Handle replacing the main hole when triggered by container logic, playing effect and swapping as needed
    public IEnumerator ChangeMainHoleByCotain(Side side)
    {
        var currentHole = (side == Side.Left) ? mainHoleLeft : mainHoleRight;
        var info = (side == Side.Left) ? leftFHInfo : rightFHInfo;
        var spawnInfo = (side == Side.Left) ? leftFHSpawnInfo : rightFHSpawnInfo;

        // Play end-of-hole effect before swapping
        yield return StartCoroutine(EffectHoleEnd(currentHole, 0.5f));

        // If no replacement instances remain, exit
        if (info.holeInstances.Count <= 1)
            yield break;

        var basePos = currentHole.transform.position;

        // Deactivate and remove the current hole
        currentHole.SetActive(false);
        info.holeInstances.RemoveAt(0);

        // Promote the next instance as new main hole
        var newMain = info.holeInstances[0];
        if (!newMain.activeSelf)
            newMain.SetActive(true);

        // Move new main hole into the saved base position
        StartCoroutine(MoveToPosition(newMain, basePos, moveDuration));

        if (side == Side.Left)
            mainHoleLeft = newMain;
        else
            mainHoleRight = newMain;

        // Reposition remaining holes behind the new main
        for (var i = 1; i < info.holeInstances.Count; i++)
        {
            var hole = info.holeInstances[i];
            if (!hole.activeSelf)
                hole.SetActive(true);

            var targetPos = new Vector3(
                basePos.x,
                basePos.y,
                basePos.z - spawnInfo.SpacingZ * i
            );
            StartCoroutine(MoveToPosition(hole, targetPos, moveDuration));
        }

        Destroy(currentHole);
        info.holeBlankGroups = 4;
    }

    // Animate a hole scaling up and down to indicate it is ending
    private IEnumerator EffectHoleEnd(GameObject hole, float duration)
    {
        var originalScale = hole.transform.localScale;
        var targetScale = originalScale * 1.2f;

        var sequence = DOTween.Sequence();
        sequence.Append(hole.transform.DOScale(targetScale, duration / 2f).SetEase(Ease.OutQuad));
        sequence.Append(hole.transform.DOScale(originalScale, duration / 2f).SetEase(Ease.InQuad));

        yield return sequence.WaitForCompletion();
    }

    // Smoothly move a GameObject from its current position to the specified target over given duration, then dispatch an event
    private IEnumerator MoveToPosition(GameObject obj, Vector3 targetPos, float duration)
    {
        var startPos = obj.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            obj.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = targetPos;
        EventDispatcher.Dispatch(new EventDefine.OnChangeMainHole());
    }
}

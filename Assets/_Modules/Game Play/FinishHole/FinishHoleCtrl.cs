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

    // Replace the current hole: deactivate the old one, promote the next, move others into position, reset holeBlankGroups
    public IEnumerator ChangeMainHole(GameObject hole, Side side)
    {
        GameObject currentHole = (side == Side.Left) ? mainHoleLeft : mainHoleRight;
        FinishHoleInfo info = (side == Side.Left) ? leftFHInfo : rightFHInfo;
        FinishHoleSpawnInfo spawnInfo = (side == Side.Left) ? leftFHSpawnInfo : rightFHSpawnInfo;

        // Nếu vẫn còn >1 nhóm trống, chỉ giảm rồi dừng
        if (info.holeBlankGroups > 1)
        {
            info.holeBlankGroups--;
            yield break;
        }

        // info.holeBlankGroups hiện <= 1, nghĩa là sau lần này sẽ thay hố
        // Chạy hiệu ứng phóng to–thu nhỏ rồi mới thay hố
        yield return StartCoroutine(EffectHoleEnd(currentHole, 0.5f));

        // Nếu chỉ còn 1 hoặc 0 instance, đóng hố rồi dừng
        if (info.holeInstances.Count <= 1)
        {
            //Debug.LogWarning("No more holes to replace.");
            //currentHole.SetActive(false);
            yield break;
        }

        // Lưu vị trí hố cũ trước khi xóa
        Vector3 basePos = currentHole.transform.position;

        // Ẩn và loại bỏ hố cũ
        currentHole.SetActive(false);
        info.holeInstances.RemoveAt(0);

        // Chọn hố mới (phần tử đầu trong danh sách), kích hoạt nếu cần
        GameObject newMain = info.holeInstances[0];
        if (!newMain.activeSelf)
            newMain.SetActive(true);

        // Tween hố mới về đúng vị trí của hố cũ
        StartCoroutine(MoveToPosition(newMain, basePos, moveDuration));

        // Cập nhật biến mainHoleLeft / mainHoleRight cho đúng side
        if (side == Side.Left)
            mainHoleLeft = newMain;
        else
            mainHoleRight = newMain;

        // Di chuyển các hố còn lại xuống sau hố chính mới, căn theo SpacingZ
        for (int i = 1; i < info.holeInstances.Count; i++)
        {
            GameObject h = info.holeInstances[i];
            if (!h.activeSelf)
                h.SetActive(true);

            Vector3 targetPos = new Vector3(
                basePos.x,
                basePos.y,
                basePos.z - spawnInfo.SpacingZ * i
            );
            StartCoroutine(MoveToPosition(h, targetPos, moveDuration));
        }

        // Hủy hố cũ
        Destroy(currentHole);

        // Reset lại holeBlankGroups cho side này
        info.holeBlankGroups = 4;
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

        // Gửi event thông báo đã thay hố
        EventDispatcher.Dispatch(new EventDefine.OnChangeMainHole());
    }
}

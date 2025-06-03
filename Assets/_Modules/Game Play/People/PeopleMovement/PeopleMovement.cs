using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PeopleMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float movementSlowDownFactor = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool isFalling = false; // để ngăn di chuyển tiếp khi rơi vào hole
    private Vector3 target;
    private bool moved = false;
    public bool Moved => moved;

    public void Moving(List<Node> movingNodes)
    {
        target = movingNodes[movingNodes.Count - 1].transform.position;
        StartCoroutine(MoveThroughNodes(movingNodes));
    }

    public void Moving(GameObject movingObj)
    {
        if (movingObj == null) return;

        Vector3 targetPosition = movingObj.transform.position;
        targetPosition.y = transform.position.y; // Giữ nguyên chiều cao hiện tại

        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.DORotateQuaternion(lookRotation, 1f / rotationSpeed);
        }

        float distance = Vector3.Distance(transform.position, targetPosition);
        float duration = (distance / moveSpeed) * movementSlowDownFactor;

        transform.DOMove(targetPosition, duration).SetEase(Ease.Linear);
    }

    public void MovementInstant(GameObject movingObj)
    {
        transform.position = movingObj.transform.position;
        transform.position += Vector3.up * 2;

        transform.DOMoveY(movingObj.transform.position.y - 2f, 0.2f)
                              .SetEase(Ease.InQuad)
                              .OnComplete(() =>
                              {
                                  moved = true;
                                  //gameObject.SetActive(false);
                              });
    }


    private IEnumerator MoveThroughNodes(List<Node> movingNodes)
    {
        if (movingNodes == null || movingNodes.Count == 0)
            yield break;

        Vector3 startPosition = transform.position;
        Vector3 initialPosition = movingNodes[0].Position;
        initialPosition.y = transform.position.y;
        transform.position = initialPosition;

        for (int i = 1; i < movingNodes.Count; i++)
        {
            if (isFalling) yield break; // dừng nếu đã rơi vào hole

            Vector3 targetPosition = movingNodes[i].Position;
            targetPosition.y = transform.position.y;

            if (Mathf.Approximately(targetPosition.x, startPosition.x) &&
                Mathf.Approximately(targetPosition.z, startPosition.z))
                continue;

            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.DORotateQuaternion(lookRotation, 1f / rotationSpeed);
            }

            float distance = Vector3.Distance(transform.position, targetPosition);
            float duration = (distance / moveSpeed) * movementSlowDownFactor;

            Tween moveTween = transform.DOMove(targetPosition, duration).SetEase(Ease.Linear);
            yield return moveTween.WaitForCompletion();

            startPosition = targetPosition;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFalling) return;

        GameObject other = collision.gameObject;

        // Kiểm tra layer và tag
        if (other.layer == LayerMask.NameToLayer("Hole") && other.CompareTag(gameObject.tag))
        {
            isFalling = true;
            JumpIntoHole(target); // vị trí va chạm
        }
    }


    private void JumpIntoHole(Vector3 holePosition)
    {
        Vector3 startPos = transform.position;

        // Tạo điểm đến: cùng độ cao hiện tại nhưng lệch về phía hole một chút
        Vector3 jumpTarget = holePosition;
        jumpTarget.y = startPos.y; // giữ nguyên chiều cao để DoJump lo phần nhảy

        float jumpPower = 2.5f;      // Độ cao bật lên
        float jumpDuration = 0.6f;   // Tổng thời gian nhảy

        // Nhảy tới vị trí hole với độ cao cong
        transform.DOJump(jumpTarget, jumpPower, 1, jumpDuration)
                 .SetEase(Ease.OutQuad)
                 .OnComplete(() =>
                 {
                     // Sau khi chạm đất, rơi xuống trong hole
                     transform.DOMoveY(holePosition.y - 2f, 0.2f)
                              .SetEase(Ease.InQuad)
                              .OnComplete(() =>
                              {
                                  moved = true;
                                  //gameObject.SetActive(false);
                              });
                 });
    }


}

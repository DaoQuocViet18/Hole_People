using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PeopleMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float movementSlowDownFactor = 2f; // Hệ số làm chậm
    public float rotationSpeed = 5f; // Tốc độ quay

    public void Moving(List<Node> movingNodes)
    {
        StartCoroutine(MoveThroughNodes(movingNodes));
    }

    private IEnumerator MoveThroughNodes(List<Node> movingNodes)
    {
        if (movingNodes == null || movingNodes.Count == 0)
            yield break;

        Vector3 startPosition = transform.position;

        // Đặt vị trí bắt đầu tại node đầu tiên
        Vector3 initialPosition = movingNodes[0].position;
        initialPosition.y = transform.position.y;
        transform.position = initialPosition;

        for (int i = 1; i < movingNodes.Count; i++)
        {
            Vector3 targetPosition = movingNodes[i].position;
            targetPosition.y = transform.position.y;

            if (Mathf.Approximately(targetPosition.x, startPosition.x) &&
                Mathf.Approximately(targetPosition.z, startPosition.z))
                continue;

            // Tính hướng quay
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.DORotateQuaternion(lookRotation, 1f / rotationSpeed); // quay mượt
            }

            float distance = Vector3.Distance(transform.position, targetPosition);
            float duration = (distance / moveSpeed) * movementSlowDownFactor;

            Tween moveTween = transform.DOMove(targetPosition, duration).SetEase(Ease.Linear);
            yield return moveTween.WaitForCompletion();

            startPosition = targetPosition;

            Debug.Log($"Đã đến node {i}: {targetPosition}");
        }
    }
}

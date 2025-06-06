﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PeopleMovement : CtrlMonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private PeopleMovementInfo movementInfo;

    [SerializeField] private bool isFalling = false;
    [SerializeField] private bool moved = false;

    private Vector3 target;

    public bool Moved => moved;
    public float MoveSpeed => movementInfo.MoveSpeed;
    public float SlowDownFactor => movementInfo.MovementSlowDownFactor;
    public float RotationSpeed => movementInfo.RotationSpeed;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        movementInfo.MoveSpeed = 10f;
        movementInfo.MovementSlowDownFactor = 2f;
        movementInfo.RotationSpeed = 5f;
    }

    public void Moving(List<Node> movingNodes)
    {
        if (movingNodes == null || movingNodes.Count == 0) return;

        target = movingNodes[^1].Position;
        StartCoroutine(MoveThroughNodes(movingNodes));
    }

    public IEnumerator MovingTowards(Vector3 targetPosition)
    {
        moved = false;
        bool movedToThreshold = false;

        // Đảm bảo giữ nguyên chiều cao y
        targetPosition.y = transform.position.y;

        // Quay mặt về phía mục tiêu
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > 3f)
        {
            // Tính vị trí dừng cách target 3f
            Vector3 thresholdPos = targetPosition - direction * 3f;
            float distToThreshold = Vector3.Distance(transform.position, thresholdPos);
            float duration = (distToThreshold / MoveSpeed) * SlowDownFactor;

            // Hủy tween cũ trước khi tạo tween mới
            transform.DOKill();
            transform.DOMove(thresholdPos, duration)
                     .SetEase(Ease.Linear)
                     .OnComplete(() => { movedToThreshold = true; });

            yield return new WaitUntil(() => movedToThreshold);
        }

        // Hủy tween (nếu có) trước khi nhảy
        transform.DOKill();

        // Thực hiện nhảy vào hố
        JumpIntoHole(targetPosition);

        // Đợi đến khi nhảy xong (moved = true)
        yield return new WaitUntil(() => moved);

        // Hủy tween trước khi xóa
        transform.DOKill();
       gameObject.SetActive(false);
    }



    private IEnumerator MoveThroughNodes(List<Node> nodes)
    {
        if (nodes == null || nodes.Count == 0) yield break;

        transform.position = new Vector3(nodes[0].Position.x, transform.position.y, nodes[0].Position.z);

        for (int i = 1; i < nodes.Count; i++)
        {
            if (isFalling) yield break;

            Vector3 targetPos = new Vector3(nodes[i].Position.x, transform.position.y, nodes[i].Position.z);
            if (Vector3.Distance(transform.position, targetPos) < 0.01f) continue;

            RotateTo(targetPos);
            yield return MoveToCoroutine(targetPos);
        }
    }

    private void RotateTo(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.DORotateQuaternion(lookRotation, 1f / RotationSpeed);
    }

    private void MoveTo(Vector3 targetPos)
    {
        float distance = Vector3.Distance(transform.position, targetPos);
        float duration = (distance / MoveSpeed) * SlowDownFactor;

        transform.DOMove(targetPos, duration).SetEase(Ease.Linear);
    }

    private IEnumerator MoveToCoroutine(Vector3 targetPos)
    {
        float distance = Vector3.Distance(transform.position, targetPos);
        float duration = (distance / MoveSpeed) * SlowDownFactor;

        Tween moveTween = transform.DOMove(targetPos, duration).SetEase(Ease.Linear);
        yield return moveTween.WaitForCompletion();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFalling) return;

        GameObject other = collision.gameObject;
        if (other.layer == LayerMask.NameToLayer("Hole") && other.CompareTag(gameObject.tag))
        {
            isFalling = true;
            JumpIntoHole(target);
        }
    }

    private void JumpIntoHole(Vector3 holePosition)
    {
        Vector3 jumpTarget = new Vector3(holePosition.x, transform.position.y, holePosition.z);

        transform.DOJump(jumpTarget, 2.5f, 1, 0.6f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOMoveY(holePosition.y - 2f, 0.2f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => moved = true);
            });
    }

    public void FallIntoHole(Vector3 holePosition)
    {
        float radius = 1f;
        float targetY = holePosition.y - 2f;

        Vector3 offset = new Vector3(
            Random.Range(-radius, radius),
            0f,
            Random.Range(-radius, radius)
        );

        Vector3 targetPos = new Vector3(holePosition.x, targetY, holePosition.z) + offset;

        transform.DOMove(targetPos, 0.2f)
            .SetEase(Ease.InQuad)
            .OnComplete(() => Destroy(gameObject));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PeopleMovement : CtrlMonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private PeopleMovementInfo movementInfo;

    [SerializeField] private bool isFalling = false; // để ngăn di chuyển tiếp khi rơi vào hole
    private Vector3 target;
    [SerializeField] private bool moved = false;

    public bool Moved => moved;

    // Optional accessors
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

        target = movingNodes[^1].transform.position;
        StartCoroutine(MoveThroughNodes(movingNodes));
    }

    public void Moving(GameObject movingObj)
    {
        if (movingObj == null) return;

        Vector3 targetPosition = movingObj.transform.position;
        targetPosition.y = transform.position.y;

        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.DORotateQuaternion(lookRotation, 1f / RotationSpeed);
        }

        float distance = Vector3.Distance(transform.position, targetPosition);
        float duration = (distance / MoveSpeed) * SlowDownFactor;

        transform.DOMove(targetPosition, duration).SetEase(Ease.Linear);
    }

    public void MovementInstant(GameObject movingObj)
    {
        transform.position = movingObj.transform.position + Vector3.up * 2;

        transform.DOMoveY(movingObj.transform.position.y - 2f, 0.2f)
                 .SetEase(Ease.InQuad)
                 .OnComplete(() =>
                 {
                     moved = true;
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
            if (isFalling) yield break;

            Vector3 targetPosition = movingNodes[i].Position;
            targetPosition.y = transform.position.y;

            if (Mathf.Approximately(targetPosition.x, startPosition.x) &&
                Mathf.Approximately(targetPosition.z, startPosition.z))
                continue;

            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.DORotateQuaternion(lookRotation, 1f / RotationSpeed);
            }

            float distance = Vector3.Distance(transform.position, targetPosition);
            float duration = (distance / MoveSpeed) * SlowDownFactor;

            Tween moveTween = transform.DOMove(targetPosition, duration).SetEase(Ease.Linear);
            yield return moveTween.WaitForCompletion();

            startPosition = targetPosition;
        }
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
        Vector3 startPos = transform.position;
        Vector3 jumpTarget = holePosition;
        jumpTarget.y = startPos.y;

        float jumpPower = 2.5f;
        float jumpDuration = 0.6f;

        transform.DOJump(jumpTarget, jumpPower, 1, jumpDuration)
                 .SetEase(Ease.OutQuad)
                 .OnComplete(() =>
                 {
                     transform.DOMoveY(holePosition.y - 2f, 0.2f)
                              .SetEase(Ease.InQuad)
                              .OnComplete(() =>
                              {
                                  moved = true;
                              });
                 });
    }

    public void FallIntoHole(Vector3 holePosition)
    {
        float targetY = holePosition.y - 2f;

        transform.DOMoveY(targetY, 0.2f)
                 .SetEase(Ease.InQuad);
    }
}

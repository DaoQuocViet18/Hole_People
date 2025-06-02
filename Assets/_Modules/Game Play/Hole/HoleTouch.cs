using System;
using System.Collections;
using UnityEngine;

public class HoleTouch : MonoBehaviour
{
    [SerializeField] private int expectedPeopleCount = 4; // Số người dự kiến vào hole
    [SerializeField] private GameObject lastPerson; // Người cuối cùng vào
    [SerializeField] private int numberPeople = 0;

    public event Action OnAllPeopleEntered;

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;

        // Kiểm tra có PeopleMovement không
        PeopleMovement movement = obj.GetComponent<PeopleMovement>();
        if (movement == null)
        {
            Debug.LogWarning($"Object '{obj.name}' has no PeopleMovement component.");
            return;
        }

        PeopleSpawnManager.Instance.Despawn(obj);
        numberPeople++;
        lastPerson = obj;

        if (numberPeople >= expectedPeopleCount)
        {
            StartCoroutine(InvokeAfterDelay(0.5f));
        }
    }

    private IEnumerator InvokeAfterDelay(float delay)
    {
        if (lastPerson == null)
        {
            Debug.LogWarning("No last person found.");
            yield break;
        }

        PeopleMovement movement = lastPerson.GetComponent<PeopleMovement>();
        if (movement == null)
        {
            Debug.LogWarning("Last person has no PeopleMovement component.");
            yield break;
        }

        while (!movement.Moved)
        {
            yield return null;
        }

        yield return new WaitForSeconds(delay);

        OnAllPeopleEntered?.Invoke();

        // Reset
        lastPerson = null;
        numberPeople = 0;
    }
}

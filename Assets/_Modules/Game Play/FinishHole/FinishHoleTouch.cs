using System;
using System.Collections.Generic;
using UnityEngine;

public class FinishHoleTouch : MonoBehaviour
{
    [SerializeField] private int expectedPeopleCount = 16;
    private readonly List<PeopleMovement> peopleInHole = new List<PeopleMovement>();

    public event Action<GameObject> OnFulledFinishHole;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PeopleMovement movement))
        {
            Debug.LogWarning($"Object '{other.gameObject.name}' has no PeopleMovement component.");
            return;
        }

        if (!peopleInHole.Contains(movement))
            peopleInHole.Add(movement);

        if (peopleInHole.Count >= expectedPeopleCount)
        {
            OnFulledFinishHole?.Invoke(gameObject);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class EntryHoleTouch : MonoBehaviour
{
    [SerializeField] private int expectedPeopleCount = 4;

    private readonly List<PeopleMovement> peopleInHole = new List<PeopleMovement>();

    // Sự kiện truyền ra Tag và list GameObject đã despawn đúng expectedPeopleCount
    public event Action<Tag, List<GameObject>> OnAllPeopleEntered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PeopleMovement movement))
        {
            if (!peopleInHole.Contains(movement))
            {
                peopleInHole.Add(movement);
            }
        }
        else
        {
            Debug.LogWarning($"Object '{other.gameObject.name}' has no PeopleMovement component.");
        }
    }

    private void Update()
    {
        if (peopleInHole.Count < expectedPeopleCount)
            return;

        // Lọc ra những người đã moved
        int movedCount = 0;
        List<PeopleMovement> readyToDespawn = new List<PeopleMovement>(expectedPeopleCount);
        foreach (var person in peopleInHole)
        {
            if (person.Moved)
            {
                readyToDespawn.Add(person);
                movedCount++;
                if (movedCount >= expectedPeopleCount)
                    break;
            }
        }

        if (movedCount >= expectedPeopleCount)
        {
            var despawnedObjects = new List<GameObject>(expectedPeopleCount);
            for (int i = 0; i < expectedPeopleCount; i++)
            {
                var movement = readyToDespawn[i];
                GameObject go = movement.gameObject;

                PeopleSpawnManager.Instance.Despawn(go);
                despawnedObjects.Add(go);
                peopleInHole.Remove(movement);
            }

            // Lấy tag từ object đầu tiên, parse an toàn hơn
            Tag incomingTag = Tag.None;
            Enum.TryParse(despawnedObjects[0].tag, out incomingTag);

            OnAllPeopleEntered?.Invoke(incomingTag, despawnedObjects);
        }
    }
}

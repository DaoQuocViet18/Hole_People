using System;
using System.Collections.Generic;
using UnityEngine;

public class HoleTouch : MonoBehaviour
{
    [SerializeField] private int expectedPeopleCount = 4;

    private List<PeopleMovement> peopleInHole = new List<PeopleMovement>();
    private bool havePeople = false;

    // Truyền ra đúng 4 GameObject đã moved
    public event Action<Tag, List<GameObject>> OnAllPeopleEntered;

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;

        PeopleMovement movement = obj.GetComponent<PeopleMovement>();
        if (movement == null)
        {
            Debug.LogWarning($"Object '{obj.name}' has no PeopleMovement component.");
            return;
        }

        if (!peopleInHole.Contains(movement))
        {
            peopleInHole.Add(movement);
            havePeople = true;
        }
    }

    private void Update()
    {
        if (!havePeople || peopleInHole.Count < expectedPeopleCount)
            return;

        // Lọc danh sách người đã moved
        List<PeopleMovement> readyToDespawn = new List<PeopleMovement>();
        foreach (var person in peopleInHole)
        {
            if (person.Moved)
                readyToDespawn.Add(person);

            if (readyToDespawn.Count >= expectedPeopleCount)
                break; // Đủ 4 người thì dừng
        }

        if (readyToDespawn.Count >= expectedPeopleCount)
        {
            List<GameObject> despawnedObjects = new List<GameObject>();
            for (int i = 0; i < expectedPeopleCount; i++)
            {
                var movement = readyToDespawn[i];
                GameObject go = movement.gameObject;

                PeopleSpawnManager.Instance.Despawn(go);
                despawnedObjects.Add(go);
                peopleInHole.Remove(movement);
            }

            // Lấy tag từ 1 người trong danh sách vừa despawn
            Tag incomingTag = Tag.None;
            Enum.TryParse(despawnedObjects[0].tag, out incomingTag);

            // Invoke với đúng 4 người đã despawn
            OnAllPeopleEntered?.Invoke(incomingTag, despawnedObjects);
        }

        if (peopleInHole.Count == 0)
        {
            havePeople = false;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class EntryHoleTouch : MonoBehaviour
{
    [SerializeField] private List<PeopleMovement> peopleInHole = new();
    [SerializeField] private bool _shouldCheckMovedPeople = false;

    public event Action<Tag, List<GameObject>> OnAllPeopleEntered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PeopleMovement movement) && !peopleInHole.Contains(movement))
        {
            peopleInHole.Add(movement);
            _shouldCheckMovedPeople = true; // Đánh dấu cần kiểm tra
        }
    }

    private void Update()
    {
        if (!_shouldCheckMovedPeople || peopleInHole.Count == 0)
            return;

        List<PeopleMovement> readyToDispatch = new();
        foreach (var person in peopleInHole)
        {
            if (person != null && person.Moved)
            {
                readyToDispatch.Add(person);
            }
        }

        if (readyToDispatch.Count > 0)
        {
            var dispatchedObjects = new List<GameObject>();
            foreach (var person in readyToDispatch)
            {
                dispatchedObjects.Add(person.gameObject);
            }

            peopleInHole.RemoveAll(p => readyToDispatch.Contains(p));

            // Dispatch nếu có ít nhất 1 object
            if (dispatchedObjects.Count > 0)
            {
                Tag incomingTag = Tag.None;
                Enum.TryParse(dispatchedObjects[0].tag, true, out incomingTag);

                OnAllPeopleEntered?.Invoke(incomingTag, dispatchedObjects);

                EventDispatcher.Dispatch(new EventDefine.OnEntryHoleTouch
                {
                    tag = dispatchedObjects[0].tag,
                    people = dispatchedObjects
                });
            }
        }

        // ✅ LUÔN duy trì kiểm tra nếu vẫn còn người
        _shouldCheckMovedPeople = peopleInHole.Count > 0;
    }

}

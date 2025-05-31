using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleTouch : MonoBehaviour
{
    [SerializeField] private int expectedPeopleCount = 4; // số người dự kiến vào hole
    [SerializeField] private List<GameObject> people = new List<GameObject>();

    public event Action<List<GameObject>> OnAllPeopleEntered;

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;

        if (!people.Contains(obj))
        {
            people.Add(obj);

            if (people.Count >= expectedPeopleCount)
            {
                StartCoroutine(InvokeAfterDelay(0.5f));
            }
        }
    }

    private IEnumerator InvokeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        OnAllPeopleEntered?.Invoke(new List<GameObject>(people));
        people.Clear();
    }
}

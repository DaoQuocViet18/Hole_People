using System;
using System.Collections.Generic;
using UnityEngine;

public class HoleTouch : MonoBehaviour
{
    [SerializeField] private int expectedPeopleCount = 4; // số người dự kiến vào hole
    [SerializeField] private List<GameObject> people = new List<GameObject>();

    public event Action<List<GameObject>> OnAllPeopleEntered;

    private bool isWaitingToClear = false;
    [SerializeField] private float clearDelay = 1f;   // thời gian chờ trước khi clear danh sách
    private float timer = 0f;

    private void Update()
    {
        if (!isWaitingToClear)
            return;

        timer += Time.deltaTime;
        if (timer >= clearDelay)
        {
            people.Clear();
            timer = 0f;
            isWaitingToClear = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;

        if (!people.Contains(obj))
        {
            people.Add(obj);

            if (people.Count >= expectedPeopleCount)
            {
                OnAllPeopleEntered?.Invoke(new List<GameObject>(people));
                isWaitingToClear = true;
                timer = 0f;
            }
        }
    }
}

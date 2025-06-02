using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ContainArrangement : MonoBehaviour
{
    public Tag tagContain;
    public List<GameObject> people = new List<GameObject>();
    public Vector3 positionPeople = new Vector3();

    public void Arrangement()
    {
        positionPeople = new Vector3(1.5f, 0, -3.6f);

        Debug.Log("people: " + people.Count);

        int column = 0;
        int maxPerRow = 4; // mỗi hàng 4 người
        int placedCount = 0;

        foreach (GameObject person in people)
        {
            if (placedCount >= 32)
            {
                Debug.LogWarning("ContainArrangement is full (more than 32 people).");
                break;
            }

            // Đặt người vào vị trí
            person.transform.position = transform.position + positionPeople;

            // Đặt lại rotation về hướng mong muốn (ví dụ: nhìn về phía Z+)
            person.transform.rotation = Quaternion.LookRotation(Vector3.forward);

            PeopleSpawnManager.Instance.SetActivePeople(person, true);

            column++;
            placedCount++;

            // Cập nhật vị trí tiếp theo
            if (column >= maxPerRow)
            {
                column = 0;
                positionPeople.x = 1.5f;
                positionPeople.z += 1; // xuống hàng
            }
            else
            {
                positionPeople.x -= 1; // sang phải
            }
        }
    }
}

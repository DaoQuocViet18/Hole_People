using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ContainArrangement : MonoBehaviour
{
    public Tag tagContain;
    public List<GameObject> newPeople = new List<GameObject>();
    public List<GameObject> people = new List<GameObject>();
    public Vector3 positionPeople = new Vector3();
    public bool fulled = false;

    public void Arrangement()
    {
        // Reset vị trí bắt đầu sắp xếp
        positionPeople = new Vector3(-2, 0.5f, 2);

        Debug.Log("newPeople: " + newPeople.Count);

        int placedCount = 0;

        foreach (GameObject person in newPeople)
        {
            if (placedCount >= 32)
            {
                fulled = true;
                Debug.LogWarning("ContainArrangement is full (more than 32 people).");
                break;
            }

            // Tính vị trí và đặt người
            person.transform.position = this.transform.position + positionPeople;
            person.SetActive(true);

            // Cập nhật vị trí cho người kế tiếp
            if (positionPeople.x < 1)
            {
                positionPeople.x += 1;
            }
            else
            {
                positionPeople.x = -2;
                positionPeople.z -= 1;
            }

            placedCount++;
        }

        // Nếu sau vòng lặp có đúng 32 người, cũng set fulled = true
        if (placedCount >= 32)
        {
            fulled = true;
        }

        // Chuyển tất cả người từ newPeople sang people và clear newPeople
        people.AddRange(newPeople);
        newPeople.Clear();
    }
}

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
        // Vị trí bắt đầu tính theo vị trí thế giới của ContainArrangement + offset
        positionPeople = new Vector3(-2, 0.5f, 2);

        Debug.Log("newPeople: " + newPeople.Count);
        foreach (GameObject person in newPeople)
        {
            // Tính vị trí thế giới của từng person bằng cách cộng offset positionPeople vào vị trí ContainArrangement
            person.transform.position = this.transform.position + positionPeople;

            if (positionPeople.x < 2)
            {
                positionPeople.x += 1;
            }
            else
            {
                positionPeople.x = -2;
                positionPeople.z -= 1;

                if (positionPeople.z > 4)
                {
                    fulled = true;
                }
            }
            person.SetActive(true);
        }
    }
}

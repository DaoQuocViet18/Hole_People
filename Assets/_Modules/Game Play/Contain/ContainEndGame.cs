using System.Collections.Generic;
using UnityEngine;

public class ContainEndGame : MonoBehaviour
{
    public List<GameObject> newPeople = new List<GameObject>();

    public void Arrangement()
    {
        foreach (GameObject person in newPeople)
        {
            person.transform.position = this.transform.position;
            
            person.SetActive(true);
        }
    }
}

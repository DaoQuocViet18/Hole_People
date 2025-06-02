using System.Collections.Generic;
using UnityEngine;

public class ContainEndGame : MonoBehaviour
{
    public List<GameObject> people = new List<GameObject>();

    public void Arrangement()
    {
        foreach (GameObject person in people)
        {
            person.transform.position = this.transform.position;
            
            person.SetActive(true);
        }
    }
}

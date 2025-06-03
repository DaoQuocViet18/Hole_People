using System.Collections.Generic;
using UnityEngine;

public class ContainEndGame : MonoBehaviour
{
    private List<GameObject> people = new List<GameObject>();

    public List<GameObject> People { get => people; set => people = value; }

    public void Arrangement()
    {
        foreach (GameObject person in People)
        {
            person.transform.position = this.transform.position;
            
            person.SetActive(true);
        }
    }
}

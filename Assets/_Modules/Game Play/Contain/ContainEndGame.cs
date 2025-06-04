using System.Collections.Generic;
using UnityEngine;

public class ContainEndGame : MonoBehaviour
{
    private List<GameObject> groupPeople = new List<GameObject>();

    public List<GameObject> GroupPeople { get => groupPeople; set => groupPeople = value; }

    public void Arrangement()
    {
        List<GameObject> allPeople = new List<GameObject>();

        foreach (var group in groupPeople)
        {
            if (group == null) continue;
            var people = group.GetComponentsInChildren<PeopleMovement>(true);
            foreach (var person in people)
            {
                if (person != null)
                    allPeople.Add(person.gameObject);
            }
        }

        foreach (GameObject person in allPeople)
        {
            person.transform.position = this.transform.position;
            
            person.SetActive(true);
        }
    }
}

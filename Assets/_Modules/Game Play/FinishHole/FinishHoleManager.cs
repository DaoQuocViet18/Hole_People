using System;
using System.Collections.Generic;
using UnityEngine;

public class FinishHoleManager : Singleton<FinishHoleManager>
{
    public GameObject mainHoleLeft;
    public GameObject mainHoleRight;
    public List<GameObject> holeLeftPrefabs = new List<GameObject>();
    public List<GameObject> holeRightPrefabs = new List<GameObject>();

    private List<GameObject> holeLeftInstances = new List<GameObject>();
    private List<GameObject> holeRightInstances = new List<GameObject>();

    public Vector3 startPositionHoleLeft = new Vector3(0, 0, -35);
    public Vector3 startPositionHoleRight = new Vector3(-10, 0, -35);
    public float spacingZ = 7f;

    void Start()
    {
        holeLeftInstances = SpawnHoles(holeLeftPrefabs, startPositionHoleLeft);
        holeRightInstances = SpawnHoles(holeRightPrefabs, startPositionHoleRight);

        if (holeLeftInstances.Count > 0)
            mainHoleLeft = holeLeftInstances[0];
        if (holeRightInstances.Count > 0)
            mainHoleRight = holeRightInstances[0];

        RegisterHoleEvents(holeLeftInstances);
        RegisterHoleEvents(holeRightInstances);
    }

    private void RegisterHoleEvents(List<GameObject> holeInstances)
    {
        foreach (var hole in holeInstances)
        {
            var touchComponent = hole.GetComponent<FinishHoleTouch>();
            if (touchComponent != null)
                touchComponent.OnFulledFinishHole += OnFulledFinishHole;
        }
    }

    private void OnFulledFinishHole(GameObject obj)
    {
        if (holeLeftInstances.Contains(obj))
        {
            Debug.Log("Hole đầy thuộc bên LEFT");
            ChangeMainHoleLeft();
        }
        else if (holeRightInstances.Contains(obj))
        {
            Debug.Log("Hole đầy thuộc bên RIGHT");
            ChangeMainHoleRight();
        }
        else
        {
            Debug.LogWarning("Hole không xác định thuộc bên nào!");
        }
    }

    private List<GameObject> SpawnHoles(List<GameObject> holePrefabs, Vector3 startPosition)
    {
        Vector3 currentPos = startPosition;
        List<GameObject> instances = new List<GameObject>();

        foreach (var prefab in holePrefabs)
        {
            GameObject instance = Instantiate(prefab, currentPos, Quaternion.identity);
            instances.Add(instance);
            currentPos.z -= spacingZ;
        }

        return instances;
    }

    public void ChangeMainHoleLeft()
    {
        ChangeMainHole(ref mainHoleLeft, holeLeftInstances);
        ContainToFinishHole(mainHoleLeft);
    }

    public void ChangeMainHoleRight()
    {
        ChangeMainHole(ref mainHoleRight, holeRightInstances);
        ContainToFinishHole(mainHoleRight);
    }

    private void ChangeMainHole(ref GameObject mainHole, List<GameObject> holeInstances)
    {
        if (holeInstances.Count <= 1)
        {
            Debug.LogWarning("Không còn hole nào để thay thế.");
            return;
        }

        GameObject oldMainHole = mainHole;
        mainHole.SetActive(false);

        holeInstances.RemoveAt(0);
        GameObject newMainHole = holeInstances[0];
        newMainHole.transform.position = oldMainHole.transform.position;
        newMainHole.SetActive(true);
        mainHole = newMainHole;

        Vector3 pos = newMainHole.transform.position;
        for (int i = 1; i < holeInstances.Count; i++)
        {
            pos.z -= spacingZ;
            holeInstances[i].transform.position = pos;
        }
    }

    private void ContainToFinishHole(GameObject mainHole)
    {
        if (mainHole == null) return;

        if (!Enum.TryParse(mainHole.tag, out Tag incomingTag))
            return;

        foreach (ContainArrangement contain in PeopleHoleToContainCtrl.Instance.ContainArrangements)
        {
            if (contain.TagContain != incomingTag)
                continue;

            foreach (GameObject person in contain.People)
            {
                if (person == null) continue;
                if (person.TryGetComponent(out PeopleMovement movement))
                {
                    //movement.Moving(mainHole);

                }
                else
                    Debug.LogWarning($"'{person.name}' is missing PeopleMovement component.");
            }

            break; // Chỉ xử lý contain đầu tiên khớp tag
        }
    }

    public bool EntryHoleToFinishHole(List<GameObject> people)
    {
        if (people == null || people.Count == 0)
            return false;

        bool movedAny = false;

        Enum.TryParse(mainHoleLeft?.tag, out Tag tagLeft);
        Enum.TryParse(mainHoleRight?.tag, out Tag tagRight);

        foreach (GameObject person in people)
        {
            if (person == null) continue;

            if (!person.TryGetComponent(out PeopleMovement movement))
            {
                Debug.LogWarning($"'{person.name}' is missing PeopleMovement component.");
                continue;
            }

            Enum.TryParse(person.tag, out Tag personTag);

            if (personTag == tagLeft && mainHoleLeft != null)
            {
                person.SetActive(true);
                movement.MovementInstant(mainHoleLeft);
                movedAny = true;
            }
            else if (personTag == tagRight && mainHoleRight != null)
            {
                person.SetActive(true);
                movement.MovementInstant(mainHoleRight);
                movedAny = true;
            }
        }

        return movedAny;
    }
}

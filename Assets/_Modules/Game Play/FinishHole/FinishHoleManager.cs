using System;
using System.Collections.Generic;
using UnityEngine;

public class FinishHoleManager : Singleton<FinishHoleManager>
{
    [SerializeField] private GameObject mainHoleLeft;
    [SerializeField] private GameObject mainHoleRight;
    [SerializeField] private List<GameObject> holeLeftPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> holeRightPrefabs = new List<GameObject>();

    [SerializeField] private List<GameObject> holeLeftInstances = new List<GameObject>();
    [SerializeField] private List<GameObject> holeRightInstances = new List<GameObject>();

    [SerializeField] private int holeBlankLeft = 16;
    [SerializeField] private int holeBlankRight = 16;

    [SerializeField] private Vector3 startPositionHoleLeft = new Vector3(0, 0, -35);
    [SerializeField] private Vector3 startPositionHoleRight = new Vector3(-10, 0, -35);
    [SerializeField] private float spacingZ = 7f;

    public List<GameObject> HoleLeftInstances { get => holeLeftInstances; set => holeLeftInstances = value; }
    public List<GameObject> HoleRightInstances { get => holeRightInstances; set => holeRightInstances = value; }
    public GameObject MainHoleLeft { get => mainHoleLeft; set => mainHoleLeft = value; }
    public GameObject MainHoleRight { get => mainHoleRight; set => mainHoleRight = value; }
    public int HoleBlankLeft { get => holeBlankLeft; set => holeBlankLeft = value; }
    public int HoleBlankRight { get => holeBlankRight; set => holeBlankRight = value; }

    void Start()
    {
        HoleLeftInstances = SpawnHoles(holeLeftPrefabs, startPositionHoleLeft);
        HoleRightInstances = SpawnHoles(holeRightPrefabs, startPositionHoleRight);

        if (HoleLeftInstances.Count > 0)
            MainHoleLeft = HoleLeftInstances[0];
        if (HoleRightInstances.Count > 0)
            MainHoleRight = HoleRightInstances[0];
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

    public bool CheckTagFinishHole(Tag tag)
    {
        if (mainHoleLeft.tag == tag.ToString() && holeBlankLeft > 0)
            return true;
        else if (mainHoleRight.tag == tag.ToString() && holeBlankRight > 0)
            return true;
        return false;
    }

    public void PutPeopleInFinishHole(GameObject parentObj)
    {
        if (parentObj == null) return;

        List<GameObject> listObj = new();
        PeopleMovement[] peopleMovements = parentObj.GetComponentsInChildren<PeopleMovement>(true);

        foreach (var movement in peopleMovements)
        {
            if (movement != null)
                listObj.Add(movement.gameObject);
        }

        if (listObj.Count == 0) return;

        // Lấy tag từ parent hoặc từ phần tử đầu tiên nếu cần
        string groupTag = parentObj.tag;
        if (string.IsNullOrEmpty(groupTag) && listObj.Count > 0)
            groupTag = listObj[0].tag;

        // Ưu tiên hố bên trái
        if (mainHoleLeft != null && mainHoleLeft.tag == groupTag && holeBlankLeft > 0)
        {
            MovePeopleOnFinishHole(mainHoleLeft, listObj, true);
        }
        // Nếu không thì thử bên phải
        else if (mainHoleRight != null && mainHoleRight.tag == groupTag && holeBlankRight > 0)
        {
            MovePeopleOnFinishHole(mainHoleRight, listObj, false);
        }
        else
        {
            Debug.LogWarning($"Không có hố nào phù hợp hoặc đủ chỗ cho nhóm với tag '{groupTag}'");
        }
    }

    private void MovePeopleOnFinishHole(GameObject mainHole, List<GameObject> listObj, bool isLeft)
    {
        int holeBlank = isLeft ? holeBlankLeft : holeBlankRight;
        int countToMove = Mathf.Min(holeBlank, listObj.Count);
        List<GameObject> selectedPeople = listObj.GetRange(0, countToMove);

        Vector3 holePos = mainHole.transform.position;

        foreach (GameObject person in selectedPeople)
        {
            person.transform.position = holePos + Vector3.up * 2;

            if (person.TryGetComponent(out PeopleMovement movement))
                movement.FallIntoHole(holePos);
            else
                Debug.LogWarning($"Object {person.name} is missing PeopleMovement component.");
        }

        // Cập nhật slot trống
        if (isLeft)
            holeBlankLeft -= countToMove;
        else
            holeBlankRight -= countToMove;

        listObj.RemoveRange(0, countToMove);

        // Đổi hố nếu hết chỗ
        if (isLeft && holeBlankLeft <= 0)
            ChangeMainHole(ref mainHoleLeft);
        else if (!isLeft && holeBlankRight <= 0)
            ChangeMainHole(ref mainHoleRight);
    }



    private void ChangeMainHole(ref GameObject mainHole)
    {
        List<GameObject> holeInstances = (mainHole == mainHoleLeft) ? holeLeftInstances : holeRightInstances;

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

        // Reset lại số chỗ trống cho hole mới
        if (mainHole == mainHoleLeft)
            holeBlankLeft = 16;
        else if (mainHole == mainHoleRight)
            holeBlankRight = 16;
    }

}

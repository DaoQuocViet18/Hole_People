using System;
using System.Collections.Generic;
using UnityEngine;

public class FinishHoleManager : Singleton<FinishHoleManager>
{
    [Header("Main Holes")]
    [SerializeField] private GameObject mainHoleLeft;
    [SerializeField] private GameObject mainHoleRight;

    [Header("Hole Sides Data")]
    [SerializeField] private HoleSideInfo leftHoleSide;
    [SerializeField] private HoleSideInfo rightHoleSide;

    [Header("Spawn Configs")]
    [SerializeField] private HoleSpawnConfig leftHoleConfig;
    [SerializeField] private HoleSpawnConfig rightHoleConfig;

    public GameObject MainHoleLeft => mainHoleLeft;
    public GameObject MainHoleRight => mainHoleRight;

    public HoleSideInfo LeftHoleSide => leftHoleSide;
    public HoleSideInfo RightHoleSide => rightHoleSide;

    public HoleSpawnConfig LeftHoleConfig => leftHoleConfig;
    public HoleSpawnConfig RightHoleConfig => rightHoleConfig;

    protected override void ResetValue()
    {
        // Reset values here if needed
        leftHoleSide.HoleBlank = 16;
        leftHoleConfig.StartPosition = new Vector3(0, 0, -35);
        leftHoleConfig.SpacingZ = 7f;

        rightHoleSide.HoleBlank = 16;
        rightHoleConfig.StartPosition = new Vector3(-10, 0, -35);
        rightHoleConfig.SpacingZ = 7f;
    }

    private void Start()
    {
        leftHoleSide.HoleInstances = SpawnHoles(leftHoleSide.HolePrefabs, leftHoleConfig);
        rightHoleSide.HoleInstances = SpawnHoles(rightHoleSide.HolePrefabs, rightHoleConfig);

        if (leftHoleSide.HoleInstances.Count > 0)
            mainHoleLeft = leftHoleSide.HoleInstances[0];
        if (rightHoleSide.HoleInstances.Count > 0)
            mainHoleRight = rightHoleSide.HoleInstances[0];
    }


    private List<GameObject> SpawnHoles(List<GameObject> prefabs, HoleSpawnConfig config)
    {
        List<GameObject> instances = new();
        Vector3 currentPos = config.StartPosition;

        foreach (var prefab in prefabs)
        {
            GameObject instance = Instantiate(prefab, currentPos, Quaternion.identity);
            instances.Add(instance);
            currentPos.z -= config.SpacingZ;
        }

        return instances;
    }

    public bool CheckTagFinishHole(Tag tag)
    {
        if (mainHoleLeft != null && mainHoleLeft.tag == tag.ToString() && leftHoleSide.HoleBlank > 0)
            return true;
        if (mainHoleRight != null && mainHoleRight.tag == tag.ToString() && rightHoleSide.HoleBlank > 0)
            return true;
        return false;
    }

    public void PutPeopleInFinishHole(GameObject parentObj)
    {
        if (parentObj == null) return;

        List<GameObject> people = new();
        PeopleMovement[] movements = parentObj.GetComponentsInChildren<PeopleMovement>(true);
        foreach (var move in movements)
        {
            if (move != null) people.Add(move.gameObject);
        }

        if (people.Count == 0) return;

        string tag = parentObj.tag;
        if (string.IsNullOrEmpty(tag) && people.Count > 0)
            tag = people[0].tag;

        if (mainHoleLeft != null && mainHoleLeft.tag == tag && leftHoleSide.HoleBlank > 0)
            MovePeopleToHole(mainHoleLeft, people, true);
        else if (mainHoleRight != null && mainHoleRight.tag == tag && rightHoleSide.HoleBlank > 0)
            MovePeopleToHole(mainHoleRight, people, false);
        else
            Debug.LogWarning($"No suitable finish hole available for tag '{tag}'");
    }

    private void MovePeopleToHole(GameObject mainHole, List<GameObject> people, bool isLeft)
    {
        HoleSideInfo side = isLeft ? leftHoleSide : rightHoleSide;
        int moveCount = Mathf.Min(side.HoleBlank, people.Count);
        List<GameObject> selected = people.GetRange(0, moveCount);

        Vector3 holePos = mainHole.transform.position;

        foreach (GameObject person in selected)
        {
            person.transform.position = holePos + Vector3.up * 4;
            if (person.TryGetComponent(out PeopleMovement move))
                move.FallIntoHole(holePos);
            else
                Debug.LogWarning($"{person.name} missing PeopleMovement.");
        }

        side.HoleBlank -= moveCount;
        people.RemoveRange(0, moveCount);

        if (side.HoleBlank <= 0)
        {
            ChangeMainHole(ref mainHole, side, isLeft ? leftHoleConfig : rightHoleConfig);
        }
    }

    private void ChangeMainHole(ref GameObject currentHole, HoleSideInfo side, HoleSpawnConfig config)
    {
        if (side.HoleInstances.Count <= 1)
        {
            Debug.LogWarning("No more holes to replace.");
            return;
        }

        GameObject oldHole = currentHole;
        oldHole.SetActive(false);
        side.HoleInstances.RemoveAt(0);
        Destroy(oldHole);

        GameObject newMain = side.HoleInstances[0];
        newMain.transform.position = oldHole.transform.position;
        newMain.SetActive(true);
        currentHole = newMain;

        Vector3 pos = newMain.transform.position;
        for (int i = 1; i < side.HoleInstances.Count; i++)
        {
            pos.z -= config.SpacingZ;
            side.HoleInstances[i].transform.position = pos;
        }

        side.HoleBlank = 16; // Reset capacity for new hole
    }
}

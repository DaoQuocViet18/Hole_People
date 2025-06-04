using System;
using System.Collections.Generic;
using UnityEngine;

public class FinishHoleManager : Singleton<FinishHoleManager>
{
    [Header("Main Holes")]
    [SerializeField] private GameObject mainHoleLeft;
    [SerializeField] private GameObject mainHoleRight;

    [Header("Finish Hole Info")]
    [SerializeField] private FinishHoleInfo leftFHInfo;
    [SerializeField] private FinishHoleInfo rightFHInfo;

    [Header("Finish Hole Spawn Info")]
    [SerializeField] private FinishHoleSpawnInfo leftFHSpawnInfo;
    [SerializeField] private FinishHoleSpawnInfo rightFHSpawnInfo;

    public GameObject MainHoleLeft => mainHoleLeft;
    public GameObject MainHoleRight => mainHoleRight;

    public FinishHoleInfo LeftFHInfo => leftFHInfo;
    public FinishHoleInfo RightFHInfo => rightFHInfo;

    public FinishHoleSpawnInfo LeftFHSpawnInfo => leftFHSpawnInfo;
    public FinishHoleSpawnInfo RightFHSpawnInfo => rightFHSpawnInfo;

    protected override void ResetValue()
    {
        // Reset values here if needed
        leftFHInfo.holeBlankGroups = 4;
        leftFHSpawnInfo.StartPosition = new Vector3(0, 0, -35);
        leftFHSpawnInfo.SpacingZ = 7f;

        rightFHInfo.holeBlankGroups = 4;
        rightFHSpawnInfo.StartPosition = new Vector3(-10, 0, -35);
        rightFHSpawnInfo.SpacingZ = 7f;
    }

    private void Start()
    {
        leftFHInfo.holeInstances = SpawnHoles(leftFHInfo.holePrefabs, leftFHSpawnInfo);
        rightFHInfo.holeInstances = SpawnHoles(rightFHInfo.holePrefabs, rightFHSpawnInfo);

        if (leftFHInfo.holeInstances.Count > 0)
            mainHoleLeft = leftFHInfo.holeInstances[0];
        if (rightFHInfo.holeInstances.Count > 0)
            mainHoleRight = rightFHInfo.holeInstances[0];
    }


    private List<GameObject> SpawnHoles(List<GameObject> prefabs, FinishHoleSpawnInfo config)
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

    public Tag[] ExportTagFinishHole()
    {
        Tag leftTag = Tag.None;
        Tag rightTag = Tag.None;

        if (mainHoleLeft != null && Enum.TryParse(mainHoleLeft.tag, out Tag parsedLeft))
            leftTag = parsedLeft;

        if (mainHoleRight != null && Enum.TryParse(mainHoleRight.tag, out Tag parsedRight))
            rightTag = parsedRight;

        return new Tag[] { leftTag, rightTag };
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

        bool isLeft = mainHoleLeft != null && mainHoleLeft.tag == tag && leftFHInfo.holeBlankGroups > 0;
        bool isRight = mainHoleRight != null && mainHoleRight.tag == tag && rightFHInfo.holeBlankGroups > 0;

        if (isLeft)
        {
            MovePeopleToHole(mainHoleLeft, people);
            leftFHInfo.holeBlankGroups--;

            if (leftFHInfo.holeBlankGroups <= 0)
                ChangeMainHole(ref mainHoleLeft, leftFHInfo, leftFHSpawnInfo, true);
        }
        else if (isRight)
        {
            MovePeopleToHole(mainHoleRight, people);
            rightFHInfo.holeBlankGroups--;

            if (rightFHInfo.holeBlankGroups <= 0)
                ChangeMainHole(ref mainHoleRight, rightFHInfo, rightFHSpawnInfo, false);
        }
        else
        {
            Debug.LogWarning($"No suitable finish hole available for tag '{tag}'");
        }
    }


    private void MovePeopleToHole(GameObject mainHole, List<GameObject> people)
    {
        Vector3 holePos = mainHole.transform.position;

        foreach (GameObject person in people)
        {
            person.transform.position = holePos + Vector3.up * 4;
            if (person.TryGetComponent(out PeopleMovement move))
                move.FallIntoHole(holePos);
            else
                Debug.LogWarning($"{person.name} missing PeopleMovement.");
        }
    }


    private void ChangeMainHole(ref GameObject currentHole, FinishHoleInfo side, FinishHoleSpawnInfo config, bool isLeft)
    {
        if (side.holeInstances.Count <= 1)
        {
            Debug.LogWarning("No more holes to replace.");
            currentHole.SetActive(false);
            return;
        }

        GameObject oldHole = currentHole;
        oldHole.SetActive(false);
        side.holeInstances.RemoveAt(0);

        GameObject newMain = side.holeInstances[0];
        newMain.transform.position = oldHole.transform.position;
        newMain.SetActive(true);
        currentHole = newMain;

        // Update mainHoleLeft hoặc mainHoleRight trực tiếp trong class
        if (isLeft)
            mainHoleLeft = newMain;
        else
            mainHoleRight = newMain;

        Vector3 pos = newMain.transform.position;
        for (int i = 1; i < side.holeInstances.Count; i++)
        {
            pos.z -= config.SpacingZ;
            side.holeInstances[i].transform.position = pos;
        }

        Destroy(oldHole);

        side.holeBlankGroups = 4; // Reset capacity for new hole

        //EventDispatcher.Dispatch(new EventDefine.OnChangeMainHole());
    }

}

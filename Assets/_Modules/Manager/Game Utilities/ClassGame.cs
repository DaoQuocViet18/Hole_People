using System.Collections.Generic;
using UnityEngine;

public class ClassGame : MonoBehaviour
{

}

[System.Serializable]
public class GroupOfPeople
{
    public Tag tag;
    public List<GameObject> groupPeople = new List<GameObject>();
}

[System.Serializable]
public class FinishHoleInfo
{
    public int holeBlankGroups;
    public List<GameObject> holePrefabs = new List<GameObject>();
    public List<GameObject> holeInstances = new List<GameObject>();
}

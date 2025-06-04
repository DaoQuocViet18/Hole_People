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
public class HoleSideInfo
{
    public int HoleBlank;
    public List<GameObject> HolePrefabs = new List<GameObject>();
    public List<GameObject> HoleInstances = new List<GameObject>();
}

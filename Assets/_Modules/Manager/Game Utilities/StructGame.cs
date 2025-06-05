using System.Collections.Generic;
using UnityEngine;

public class StructGame : MonoBehaviour
{

}
public enum Tag
{
    None,
    Red,
    Blue,
    Green
}

public enum Kind
{
    Contain,
    EntryHole,
    FinishHole
}

public enum Side
{
    Left,
    Right
}

[System.Serializable]
public struct FinishHoleSpawnInfo
{
    public Vector3 StartPosition;
    public float SpacingZ;
}

[System.Serializable]
public struct NodeCostInfo
{
    public float GCost;
    public float HCost;

    public float FCost => GCost + HCost;

    public NodeCostInfo(float gCost, float hCost)
    {
        GCost = gCost;
        HCost = hCost;
    }
}

[System.Serializable]
public struct PeopleMovementInfo
{
    public float MoveSpeed;
    public float MovementSlowDownFactor;
    public float RotationSpeed;
}

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public partial class EventDefine : IEventParam
{
    public struct OnLoadScene : IEventParam { }

    public struct OnNodeRun : IEventParam
    {
        public string tag;
    }

    public struct OnPeopleRun: IEventParam {
        public string tag;
        public Node target; 
    }

    public struct OnEntryHoleTouch : IEventParam
    {
        public string tag;
        public List<GameObject> groupParentPeople;
    }

    public struct OnChangeMainHole : IEventParam { }
}
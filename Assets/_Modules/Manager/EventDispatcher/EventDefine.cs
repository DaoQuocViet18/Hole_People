using NUnit.Framework;
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
}
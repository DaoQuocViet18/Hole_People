using NUnit.Framework;
using UnityEngine;

public partial class EventDefine : IEventParam
{
    public struct OnLoadScene : IEventParam { }

    public struct OnClickHole : IEventParam
    {
        public string tag;
    }

    public struct OnPeopleFindHole: IEventParam {
        public string tag;
        public Node target; 
    }
}
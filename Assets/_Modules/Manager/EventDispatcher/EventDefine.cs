using UnityEngine;

public partial class EventDefine : IEventParam
{

    public struct OnLoadScene : IEventParam { }

    public struct OnIncreasePoint : IEventParam
    {

    }

    public struct OnUpdateProgressBar : IEventParam
    {

    }

    public struct OnTutorialGame : IEventParam
    {
        public bool isTutorial;
    }
    public struct OnGamePaused : IEventParam
    {
        public bool isPaused;
    }

    public struct OnWinGame : IEventParam { }
    public struct OnLoseGame : IEventParam { }
}
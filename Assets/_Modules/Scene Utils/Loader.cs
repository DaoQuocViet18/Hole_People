using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Need to match the scene name in the build settings
public enum SceneName
{
    MainMenuScene,
    SelectLevelScene,
    GameScene,
    EndingScene,
    Level1,
    Level2,
    Level3,
    Level4,
    Level5,
    Level6,
}

public class Loader : Singleton<Loader>
{

    public void Load(SceneName targetScene)
    {
        EventDispatcher.Dispatch(new EventDefine.OnLoadScene());
        SceneManager.LoadSceneAsync(targetScene.ToString());
    }

    public async void LoadWithFade(SceneName targetScene)
    {
        Transform fadeTransitionPrefab = Resources.Load<Transform>("pfFadeSceneTransition");
        FadeTransition fadeTransition = Instantiate(fadeTransitionPrefab).GetComponent<FadeTransition>();
        DontDestroyOnLoad(fadeTransition.gameObject);

        await fadeTransition.FadeOut();
        Load(targetScene);
        await fadeTransition.FadeIn();
        Destroy(fadeTransition.gameObject);
    }
}

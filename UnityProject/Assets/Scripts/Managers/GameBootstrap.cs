using UnityEngine;

/// <summary>
/// シーンに何も配置しなくても、Playを押した瞬間にこのメソッドが自動実行され、
/// ゲーム全体を構築する GameRoot を生成する。
/// これにより「空のシーンでPlayを押すだけ」でゲームが起動する。
/// </summary>
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        // 既にGameRootが存在する場合は二重生成しない
        if (Object.FindObjectOfType<GameRoot>() != null) return;

        GameObject rootObj = new GameObject("_GameRoot");
        rootObj.AddComponent<GameRoot>();
    }
}

using UnityEngine;
using UnityEditor;

public class PlaySceneWithoutLoad : EditorWindow
{
	public string Scene;

    // We still have to support Unity 5.2 in modules :(
    #if UNITY_5_2

    public static void Play(string targetScene)
    {
        var returnScene = EditorApplication.currentScene;
        if(string.IsNullOrEmpty(returnScene))
        {
            if(EditorApplication.SaveCurrentSceneIfUserWantsTo())
            {
                if(!string.IsNullOrEmpty(EditorApplication.currentScene))
                {
                    PlaySceneWithoutLoad.Play(targetScene);
                }
                else
                {
                    EditorApplication.OpenScene(targetScene);
                    EditorApplication.isPlaying = true;
                }
                return;
            }
        }
        else if(returnScene == targetScene)
        {
            EditorApplication.isPlaying = true;
        }
        else
        {
            var window = (PlaySceneWithoutLoad)GetWindow(typeof(PlaySceneWithoutLoad));
            window.Scene = returnScene;
            window.maxSize = new Vector2(200, 20);

            EditorApplication.OpenScene(targetScene);
            EditorApplication.isPlaying = true;
        }
    }
    void Update()
    {
        if(!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.OpenScene(Scene);
            Close ();
        }
    }

    #else

    public static void Play(string targetScene)
    {
        var returnScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if(string.IsNullOrEmpty(returnScene.path))
        {
            if(UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                if(!string.IsNullOrEmpty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path))
                {
                    Play(targetScene);
                }
                else
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(targetScene);
                    EditorApplication.isPlaying = true;
                }
                return;
            }
        }
        else if(returnScene.path == targetScene)
        {
            EditorApplication.isPlaying = true;
        }
        else
        {
            var window = (PlaySceneWithoutLoad)GetWindow(typeof(PlaySceneWithoutLoad));
            window.Scene = returnScene.path;
            window.maxSize = new Vector2(200, 20);

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(targetScene);
            EditorApplication.isPlaying = true;
        }
    }
    void Update()
    {
        if(!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(Scene);
            Close ();
        }
    }

    #endif
	
	void OnGUI()
	{
		GUILayout.Label("Waiting to finish play...");
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class EditorMenus 
{
	[MenuItem("Game/Run %l")]
	public static void DebugUI()
	{
		if (!EditorSceneManager.GetActiveScene().isDirty || EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
		{
			PlaySceneWithoutLoad.Play("Assets/Scenes/main.unity");
		}
	}
}

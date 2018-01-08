using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System;

public class BasicEditorTextWindow : EditorWindow
{
	string text;
	string _title;
	Action<string> onConfirm;

	public static void Show(string title, Action<string> onConfirm, string prefill = null)
	{
		var window = CreateInstance<BasicEditorTextWindow>();
		window._title = title;
#if UNITY_4_6
		window.title = title;
		#else
		window.titleContent = new GUIContent(title);
		#endif
		window.text = prefill != null ? prefill : "";
		window.onConfirm = onConfirm;
		window.position = new Rect(Screen.width/2,Screen.height/2, 300, 200);
		window.ShowPopup();
	}
	
	public void OnGUI()
	{
		GUILayout.Label (_title);
		text = EditorGUILayout.TextField(text);
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Cancel"))
		{
			Close();
		}
		if (GUILayout.Button("Continue"))
		{
			onConfirm(text);
			Close();
		}
		EditorGUILayout.EndHorizontal();
	}
}
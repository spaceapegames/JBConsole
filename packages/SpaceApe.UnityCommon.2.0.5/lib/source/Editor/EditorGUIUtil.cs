using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

public static class EditorGUIUtil 
{
	// EditorGUI.kNumberW == 40f but is internal
	public const float kNumberW = 40.0f;
	public const float LINE_HEIGHT = 16f;
	
	public static void WithColor(Color color, Action action)
	{
		var colourBefore = GUI.color;
		GUI.color = color;
		try { action(); } 
		finally { GUI.color = colourBefore; }
	}
	public static void WithBackgroundColor(Color color, Action action)
	{
		var colourBefore = GUI.backgroundColor;
		GUI.backgroundColor = color;
		try { action(); } 
		finally { GUI.backgroundColor = colourBefore; }
	}
	
	public static void WithEnabled(bool enabled, Action action)
	{
		EditorGUI.BeginDisabledGroup(!enabled);
		//		var enabledBefore = GUI.enabled;
		//		GUI.enabled = enabled;
		try { action(); }
		//		finally { GUI.enabled = enabledBefore; }
		finally { EditorGUI.EndDisabledGroup(); }
	}
	
	public static void WithHorizontal(Action action)
	{
		EditorGUILayout.BeginHorizontal();
		try { action(); }
		finally { EditorGUILayout.EndHorizontal(); }
	}
	
	public static void WithVertical(Action action)
	{
		EditorGUILayout.BeginVertical();
		try { action(); }
		finally { EditorGUILayout.EndVertical(); }
	}
	
	public static bool LayoutFoldout(bool foldout, GUIContent content, bool toggleOnLabelClick = true, GUIStyle style = null)
	{
		style = style ?? EditorStyles.foldout;
		Rect position = GUILayoutUtility.GetRect(kNumberW, kNumberW, LINE_HEIGHT, LINE_HEIGHT, style);
		return EditorGUI.Foldout(position, foldout, content, toggleOnLabelClick, style);
	}
	public static bool LayoutFoldout(bool foldout, string content, bool toggleOnLabelClick = true, GUIStyle style = null)
	{
		style = style ?? EditorStyles.foldout;
		return LayoutFoldout(foldout, new GUIContent(content), toggleOnLabelClick, style);
	}
	
	
}
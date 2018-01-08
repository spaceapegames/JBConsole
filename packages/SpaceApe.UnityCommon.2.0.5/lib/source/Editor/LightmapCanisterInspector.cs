using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(LightmapCanister))]
public class LightmapCanisterInspector : Editor
{
    public override void OnInspectorGUI()
	{
		base.DrawDefaultInspector();
		var canister = target as LightmapCanister;
		if (canister != null)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Capture"))
			{
				canister.Capture();
			}
			if (GUILayout.Button("Deploy"))
			{
				canister.Deploy();
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}


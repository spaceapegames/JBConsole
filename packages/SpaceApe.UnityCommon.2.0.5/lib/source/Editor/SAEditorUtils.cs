using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class SAEditorUtils
{
	public static string[] FindAllGuids<T>(string subdir = "") where T : UnityEngine.Object
	{
		var type = typeof(T);
		var cname = type.Name;
		var dir = "Assets"+(string.IsNullOrEmpty(subdir) ? "" : "/"+subdir);
		var guids = AssetDatabase.FindAssets("t:"+cname, new string[]{dir});
		return guids;
	}

	public static IEnumerable<T> IterateAllAssets<T>(string subdir = "") where T : UnityEngine.Object
	{
		var guids = FindAllGuids<T>(subdir);
		return IterateAllGuids<T>(guids);
	}

	public static IEnumerable<T> IterateAllGuids<T>(string[] guids) where T : UnityEngine.Object
	{
		foreach(var guid in guids)
		{
			var go = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(T)) as T;
			if (go)
			{
				yield return go;
			}
		}
	}

	public static IEnumerable<GameObject> IterateAllPrefabs(string subdir = "")
    {
		return IterateAllAssets<GameObject>(subdir);
    }

	public static List<T> FindAllAsset<T>(string subdir = "") where T : UnityEngine.Object
    {
		var objs = new List<T>();
		objs.AddRange(IterateAllAssets<T>(subdir));
		return objs;
	}

    public static List<GameObject> FindAllPrefabs(string subdir = "")
    {
		return FindAllAsset<GameObject>(subdir);
	}

    public static void SetLayerIncludingChildren(GameObject go, int layer)
    {
        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = layer;
        }
    }

    public static string GetHierarchicalPathOfGameObject(GameObject go)
    {
        var path = go.name;
        for (var t = go.transform.parent; t; t = t.parent)
        {
            path = t.gameObject.name + "/" + path;
        }
        return path;
	}

	[MenuItem("SpaceApe/Find Components")]
	private static void FindComponents()
	{
		BasicEditorTextWindow.Show("Enter Class Name", delegate(string typeName){
			if(typeName == null || typeName.Length < 3)
			{
				Debug.Log("Search too short: " + typeName +".");
				return;
			}
			typeName = typeName.ToLower();

			var guids = FindAllGuids<GameObject>();
			for(int i = 0, l = guids.Length; i < l; i++)
			{
				if(EditorUtility.DisplayCancelableProgressBar("Searching for: " + typeName, typeName, i / (float)l))
				{
					break;
				}
				var prefab = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(GameObject)) as GameObject;
				if(!prefab) continue;
				foreach(var component in prefab.GetComponentsInChildren<Component>(true))
				{
					if(component && component.GetType().Name.ToLower().Contains(typeName))
					{
						Debug.Log(AssetDatabase.GetAssetPath(prefab) + ": " +SAEditorUtils.GetHierarchicalPathOfGameObject(component.gameObject), prefab);
					}
				}
			}
			EditorUtility.ClearProgressBar();
		});
	}
	
	
	[MenuItem("SpaceApe/Delete Empty Directories")]
	public static void DeleteAllEmptyDirectories()
	{
		var deletedPaths = new List<string> ();
		DeleteEmptyDirectories("Assets/", deletedPaths);
		Debug.Log (string.Format("Deleted {0} empty directories:\n{1}", deletedPaths.Count, string.Join("\n", deletedPaths.ToArray())));
	}

	public static void DeleteEmptyDirectories(string startLocation, List<string> deletedPaths = null)
	{
		if(!Directory.Exists(startLocation))
		{
			throw new Exception("Not a directory: " + startLocation);
		}
		foreach (var directory in Directory.GetDirectories(startLocation))
		{
			DeleteEmptyDirectories(directory, deletedPaths);
			if (Directory.GetFiles(directory).Length == 0 && 
			    Directory.GetDirectories(directory).Length == 0)
			{
				//Directory.Delete(directory, false);
				AssetDatabase.DeleteAsset(directory);
				if(deletedPaths != null)
				{
					deletedPaths.Add(directory);
				}
			}
		}
	}
}
using UnityEngine;

public static class GameObjectUtils
{
	
    public static void DestroyChildren(Component component, bool removeFromParent = false)
	{
		if(component) DestroyChildren(component.gameObject, removeFromParent);
	}

	public static void DestroyChildren(GameObject go, bool removeFromParent = false)
	{
		if(go)
		{
			var t = go.transform;

			var len = t.childCount;
			for (var i = len - 1; i >= 0; i--)
			{
				var child = t.GetChild(i);
				if (removeFromParent)
				{
					child.gameObject.transform.SetParent(null);
				}
				Destroy(child.gameObject);
			}
		}
	}

	public static void DestroyChildrenImmediately(GameObject go)
	{
		if (go)
		{
			var t = go.transform;

			var len = t.childCount;
			for (var i = len - 1; i >= 0; i--)
			{
				Object.DestroyImmediate(t.GetChild(i).gameObject);
			}
		}
	}

	public static void SetActiveIfExists(GameObject gameObject, bool active)
	{
		if (gameObject != null)
			gameObject.SetActive (active);
	}

    public static void SetAllActive(bool active, params GameObject[] objects)
    {
        if (objects != null)
        {
            foreach (var obj in objects)
            {
                if (obj != null)
                    obj.SetActive(active);
            }
        }
    }

    public static void SetAllActive(bool active, params Component[] objects)
    {
        if (objects != null)
        {
            foreach (var obj in objects)
            {
                if (obj != null && obj.gameObject!=null)
                    obj.gameObject.SetActive(active);
            }
        }
    }

    public static void SetActiveIfExists(Component component, bool active)
	{
		if (component != null)
			SetActiveIfExists (component.gameObject, active);
	}

	public static void Destroy(Object obj)
	{
		if (Application.isPlaying || !Application.isEditor) Object.Destroy(obj);
		else Object.DestroyImmediate(obj);
	}

	public static void FindAndPlayAnimation(GameObject go, AnimationClip clip)
	{
		var childAnim = go.GetComponentInChildren<Animation>();
		if(childAnim)
		{
			var name = clip.name;
			if (childAnim.GetClip(name) == null)
				childAnim.AddClip(clip, name);
			childAnim.Play(name);
		}
	}
	
	public static string GetHierarchicalPath(GameObject go)
	{
		var path = go.name;
		for (var t = go.transform.parent; t; t = t.parent)
		{
			path = t.gameObject.name + "/" + path;
		}
		return path;
	}
}
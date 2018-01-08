using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GameObjectExtensions
{
    public static T AddMissingComponent<T>(this GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null)
        {
            return go.AddComponent<T>();
        }
        return c;
    }

    public static void SetParentLocal(this Transform child, Transform parent)
    {
		child.SetParent(parent);
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
    }

    public static T GetComponentInParents<T>(this GameObject go, bool inclusive = true) where T : Component
    {
        if (inclusive)
        {
            foreach (var parentgo in GetParentsInclusive(go))
            {
                T component = parentgo.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
            return null;
        }
        else
        {
            foreach (var parentgo in GetParents(go))
            {
                T component = parentgo.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
            return null;
        }
    }

    public static IEnumerable<GameObject> GetParents(this GameObject go)
    {
        Transform loop_transform = go.transform.parent;
        while (loop_transform != null)
        {
            yield return loop_transform.gameObject;
            loop_transform = loop_transform.parent;
        }
    }

    public static IEnumerable<GameObject> GetParentsInclusive(this GameObject go)
    {
        Transform loop_transform = go.transform;
        while (loop_transform != null)
        {
            yield return loop_transform.gameObject;
            loop_transform = loop_transform.parent;
        }
    }

    public static void DestroyImmediateChildren(this GameObject go)
    {
        var children = new List<GameObject>();
        foreach (Transform child in go.transform)
            children.Add(child.gameObject);
        children.ForEach(child => GameObject.DestroyImmediate(child));
    }

    public static void DestroyChildren(this GameObject go)
    {
        var children = new List<GameObject>();
        foreach (Transform child in go.transform)
            children.Add(child.gameObject);
        children.ForEach(child => GameObject.Destroy(child));
    }

    public static GameObject FindInChildren(this GameObject go, string name)
    {
        return go.GetChildren().FirstOrDefault(c => c.name == name);
    }

    public static IEnumerable<GameObject> GetChildren(this GameObject go)
    {
        foreach (Transform child in go.transform)
        {
            var child_go = child.gameObject;
            yield return child_go;

            foreach (var sub_go in GetChildren(child_go))
            {
                yield return sub_go;
            }
        }
    }

    /// <summary>Unity-version-independent replacement for active GO property.</summary>
    /// <returns>Unity 3.5: active. Any newer Unity: activeInHierarchy.</returns>
    public static bool GetActive(this GameObject target)
    {
#if UNITY_3_5
        return target.active;
#else
        return target.activeInHierarchy;
#endif
    }

#if UNITY_3_5
    /// <summary>Unity-version-independent setter for active and SetActive().</summary>
    public static void SetActive(this GameObject target, bool value)
    {
        target.active = value;
    }
#endif
}


using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Text;

public abstract class SAInspector : EditorWindow
{
	public delegate void FoldoutContents();
	
	private Vector2 mScroll = Vector2.zero;
	private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();
	private Dictionary<string, object> parameterLists = new Dictionary<string, object>();
	private Dictionary<string, object> returnLists = new Dictionary<string, object>();
	private Dictionary<string, object> originalValues = new Dictionary<string, object>();

	[SerializeField]
	string[] foldoutsSerialized;

	void OnEnable()
	{
		if (foldoutsSerialized != null)
		{
			foldouts = foldoutsSerialized.ToDictionary(k => k, k => true);
		}
	}

	void OnDisable()
	{
		foldoutsSerialized = foldouts.Where(f => f.Value).Select(f => f.Key).ToArray();
	}

	public void Update()
	{
		Repaint();
	}

	public void OnGUI()
	{
		mScroll = GUI.BeginScrollView(
			new Rect(0, 0, position.width, position.height),
			mScroll,
			new Rect(0, 0, position.width, 10000)
			);
		DrawContents();
		GUI.EndScrollView();
	}

	protected abstract void DrawContents();
		
	protected void DoFoldout(string key, string name, FoldoutContents contents)
	{
		bool val = EditorGUILayout.Foldout(GetFoldOut(key), name);
		SetFoldOut(key, val);
		if (val)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical();
			contents();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}
	}
	
	protected bool GetFoldOut(string foldoutName)
	{
		if (foldouts.ContainsKey(foldoutName))
		{
			return foldouts[foldoutName] == true;
		}
		else
		{
			return false;
		}
	}
	
	protected void SetFoldOut(string foldoutName, bool value)
	{
		foldouts[foldoutName] = value;
	}
	

	protected void FoldoutMethods(string key, string name, object obj)
    {
        DoFoldout(key, name, delegate()
        {
            ShowMethodsForObject(obj, key);
        });
    }

	protected void FoldoutMethods(string key, string name, Type type)
    {
        DoFoldout(key, name, delegate()
        {
            ShowMethodsForType(null, type, key);
        });
    }

	protected object FoldoutFields(string key, string name, object obj, bool hideNulls = false)
   {
       string nameLabel = name;


       bool showChanges = key.StartsWith("showGlobals");
       if (showChanges)
       {
            foreach (var valkey in originalValues.Keys)
            {
                if (valkey.StartsWith(key))
                {
                    nameLabel = "*" + name;
                }
            }
       }

		System.Object returnVal = null;
		DoFoldout(key, nameLabel, delegate()
		{
			if (obj is UnityEngine.Object && GUILayout.Button("Show in Inspector"))
			{
				UnityEditor.Selection.activeObject = (UnityEngine.Object)obj;
			}
			returnVal = ShowFieldsForObject(obj, key, hideNulls);
		});
		return returnVal ?? obj;
   }


	protected System.Object ShowFieldsForObject(System.Object obj, string identifier, bool hideNulls = false)
	{
		if (obj == null || (obj is UnityEngine.Object && (UnityEngine.Object)obj == false))
		{
		    if (!hideNulls)
		    {
		        EditorGUILayout.LabelField(identifier, "NULL");
		    }
		    return null;
		}
		
		bool changed = false;
		Type type = obj.GetType();


	    var titleField = obj.ToString() + " (" + type.Name + ")" + " (#" + obj.GetHashCode().ToString("X") + ")";
	    if (obj is UnityEngine.Object)
	    {

#if UNITY_5_6_OR_NEWER
			titleField = titleField + " - " + UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(obj as UnityEngine.Object).ToString("N0") + " bytes";
#elif UNITY_5_4_OR_NEWER
			titleField = titleField + " - " + Profiler.GetRuntimeMemorySize(obj as UnityEngine.Object).ToString("N0") + " bytes";
#else
# error Undefined Unity version
#endif

	        if (obj is GameObject)
	        {
	            if (GUILayout.Button("Report Total Size"))
	            {
	                var objects = ObjectAndChildrenAsList(obj as GameObject);
	                objects.Sort(SortBySize);

	                var total = 0L;
	                StringBuilder sb = new StringBuilder();
	                foreach (var gameObject in objects)
	                {
	                    var bytes = 0L;
	                    sb.AppendLine(CreateByteReportStringForObject(gameObject, out bytes));
	                    total += bytes;
	                }
	                sb.Insert(0, "Total bytes for " + obj + " : " + total.ToString("N0") + "\n\nFull report:\n\n");
                    Debug.Log(sb.ToString());
	            }
	        }
	    }

	    
        
		
		EditorGUILayout.LabelField(titleField);
		
		
		if (obj is IDictionary)
		{
			IDictionary dict = obj as IDictionary;
			Type[] myTypes = dict.GetType().GetGenericArguments();
			
			EditorGUILayout.LabelField("Entries", ""+dict.Count);
			if (myTypes != null && myTypes.Length >= 2)
			{
				EditorGUILayout.LabelField("Key Type", "" + myTypes[0].Name);
				EditorGUILayout.LabelField("Entry Type", "" + myTypes[1].Name);
			}
			
			
			Dictionary<object,object> replacements = new Dictionary<object, object>();

			List<object> keys = new List<object>();
			
			foreach (object key in dict.Keys)
			{
				keys.Add(key);
			}
			
			keys.Sort((a,b) => {
				if (a is IComparable && b is IComparable)
				{
					return ((IComparable)a).CompareTo(b);
				}
				else
				{
					return (a != null ? a.GetHashCode() : 0) - (b != null ? b.GetHashCode() : 0);
				}
			});
			
			foreach (var key in keys)
			{
				object newVal = ShowValueEditField(identifier + ".dict." + key.ToString(), dict[key], key.ToString(), hideNulls);
				if (newVal != null && dict[key] != newVal)
				{
					changed = true;
					replacements[key] = newVal;
				}
			}
			
			foreach (object key in replacements.Keys)
			{
				dict[key] = replacements[key];
			}
		}
		else if (type.IsGenericType && typeof(IList).IsAssignableFrom(type))
		{
			IList list = obj as IList;
			EditorGUILayout.LabelField("Entries", "" + list.Count);
			
			for (int i = 0; i < list.Count; i++)
			{
				string entryName = "Entry ";
				if (list[i] != null)
				{
					entryName = list[i].ToString();
				}
				object newVal = ShowValueEditField(identifier + ".list." + i, list[i], entryName + " (" + i + ")", hideNulls);
				
				if (newVal != null && newVal != list[i])
				{
					changed = true;
					list[i] = newVal;
				}
			}
			
		}
		else if (type.IsArray)
		{
			Array array = obj as Array;
			
			if (type.GetArrayRank() == 1)
			{
				EditorGUILayout.LabelField("Entries", "" + array.GetLength(0));
				for (int i = 0; i < array.GetLength(0); i++)
				{
					string entryName = "Entry";
					if (array.GetValue(i) != null)
					{
						entryName = array.GetValue(i).ToString();
					}
					object newVal = ShowValueEditField(identifier + ".list." + i, array.GetValue(i), entryName + " (" + i + ")", hideNulls);
					
					if (newVal != null && newVal != array.GetValue(i))
					{
						changed = true;
						array.SetValue(newVal, i);
					}
				}
			}
			else if (type.GetArrayRank() == 2)
			{
				EditorGUILayout.LabelField("Entries", "" + array.GetLength(0) + " * " + array.GetLength(1));
				for (int i = 0; i < array.GetLength(0); i++)
				{
					for (int j = 0; j < array.GetLength(0); j++)
					{
						string entryName = "Entry";
						if (array.GetValue(i,j) != null)
						{
							entryName = array.GetValue(i,j).GetType().Name;
						}
						object newVal = ShowValueEditField(identifier + ".list." + i, array.GetValue(i,j), entryName + " " + i + "," + j, hideNulls);
						
						if (newVal != null && newVal != array.GetValue(i,j))
						{
							changed = true;
							array.SetValue(newVal, i,j );
						}
					}
				}
			}
			else if (type.GetArrayRank() == 3)
			{
				EditorGUILayout.LabelField("Entries", "" + array.GetLength(0) + " * " + array.GetLength(1) + " * " + array.GetLength(2));
				for (int i = 0; i < array.GetLength(0); i++)
				{
					for (int j = 0; j < array.GetLength(1); j++)
					{
						for (int k = 0; k < array.GetLength(2); k++)
						{
							string entryName = "Entry ";
							if (array.GetValue(i, j, k) != null)
							{
								entryName = array.GetValue(i,j,k).GetType().Name;
							}
							object newVal = ShowValueEditField(identifier + ".list." + i, array.GetValue(i, j, k),
							                                   entryName + " " +  i + "," + j + "," + k, hideNulls);
							
							if (newVal != null && newVal != array.GetValue(i,j,k))
							{
								changed = true;
								array.SetValue(newVal, i, j, k);
							}
						}
					}
				}
			}
		}
		else
		{
			FoldoutMethods(identifier + "AllMethods", "Show Methods", obj);
			EditorGUILayout.Separator();
			
			Type baseType = type;
			
			while (baseType != null)
			{
				List<FieldInfo> allFields =
					new List<FieldInfo>(
						baseType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
				
				var allProperties = new List<PropertyInfo>(baseType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
				
				if (allFields.Count > 0)
				{
					EditorGUILayout.LabelField("*** " + baseType.Name + " Fields:");
					var fieldsChanged = DoFields(obj, identifier, allFields, hideNulls);
					changed = changed || fieldsChanged;
				}
				
				if (allProperties.Count > 0)
				{
					EditorGUILayout.LabelField("*** " + baseType.Name + " Properties:");
					
					changed = changed || DoProperties(obj, identifier, allProperties, hideNulls);
				}
				
				baseType = baseType.BaseType;
			}
			
		}
		
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		
		return changed ? obj : null;
	}

    private string CreateByteReportStringForObject(GameObject gameObject, out long bytes)
    {
        bytes = GetRuntimeMemorySizeOfAllComponents(gameObject);
        var path = "";
        var obj = gameObject;
        path = obj.name;
        var parent = obj.transform.parent;
        if (parent != null)
        {
            obj = parent.gameObject;
        }
        else
        {
            obj = null;
        }
        while (obj != null)
        {
            path = obj.name + "\\" + path;
            parent = obj.transform.parent;
            if (parent != null)
            {
                obj = parent.gameObject;
            }
            else
            {
                obj = null;
            }
        }

        return bytes.ToString("N0") + " - " + path;
    }

    private int SortBySize(GameObject x, GameObject y)
    {
		return GetRuntimeMemorySizeOfAllComponents (x).CompareTo (GetRuntimeMemorySizeOfAllComponents (y));
    }

    
#if UNITY_5_6_OR_NEWER
	private long GetRuntimeMemorySizeOfAllComponents(GameObject obj)
	{
		var total = 0L;
		total += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong (obj);
		foreach (var component in obj.GetComponents(typeof(Component))) {
			total += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong (component);
		}
		return total;
	}
#elif UNITY_5_4_OR_NEWER
	private long GetRuntimeMemorySizeOfAllComponents(GameObject obj)
	{
		var total = 0L;
		total += Profiler.GetRuntimeMemorySize(obj);
		foreach (var component in obj.GetComponents(typeof(Component)))
		{
			total += Profiler.GetRuntimeMemorySize(component);
		}
		return total;
	}
#else
# error Undefined Unity version
#endif

    private List<GameObject> ObjectAndChildrenAsList(GameObject gameObject)
    {
        var result = new List<GameObject>();
        if (gameObject != null)
        {
            result.Add(gameObject);

            foreach (var tran in gameObject.transform)
            {
                var trans = tran as Transform;
                if (trans != null && trans.gameObject != null)
                {
                    result.AddRange(ObjectAndChildrenAsList(trans.gameObject));
                }
            }
        }
        return result;
    }

    protected bool DoProperties(object obj, string identifier, List<PropertyInfo> properties, bool hideNulls = false)
	{
		var props = properties.OrderBy(x => x.Name).ToList();
		bool changed = false;
		
		foreach (PropertyInfo prop in props)
		{
			string propName = prop.Name + " (" + prop.PropertyType.Name + ")";
			try
			{
				if (prop.CanRead && prop.GetIndexParameters().Length == 0)
				{
					object propVal = prop.GetValue(obj, null);
					if (prop.CanWrite)
					{
						object value = ShowValueEditField(identifier, propVal, propName, hideNulls);
						if (value != null && !value.Equals(propVal))
						{
							try
							{
								changed = true;
								prop.SetValue(obj, value, null);
							}
							catch (Exception e)
							{
								EditorGUILayout.LabelField("Exception setting " + propName, e.ToString());
							}
						}
					}
					else
					{
						ShowValueEditField(identifier, propVal, propName, hideNulls);
					}
				}
				else
				{
					EditorGUILayout.LabelField(propName);
				}
			}
			catch (Exception e)
			{
				EditorGUILayout.LabelField("Exception with " + propName, e.ToString());
			}
		}
		EditorGUILayout.Separator();
		
		return changed;
	}
	
	protected bool DoFields(object obj, string identifier, List<FieldInfo> allFields, bool hideNulls = false)
	{
		bool changed = false;
		var fields = allFields.OrderBy(x => x.Name).ToList();
		foreach (FieldInfo field in fields)
		{
			object fieldValue = field.GetValue(obj);
			string fieldName = field.Name + " (" + field.FieldType.Name + ")";
			
			if (field.IsLiteral || (field.IsInitOnly && field.FieldType.IsValueType))
			{
				if (fieldValue == null)
				{
                    if (!hideNulls)
                    {
                        EditorGUILayout.LabelField(fieldName, "NULL");
                    }
				}
				else
				{
					EditorGUILayout.LabelField(fieldName, fieldValue.ToString());
				}
			}
			else
			{
				object value = ShowValueEditField(identifier, fieldValue, fieldName, hideNulls);
				if (!field.IsInitOnly && value != null && !value.Equals(fieldValue))
				{
					changed = true;
					field.SetValue(obj, value);
				}
			}
		}
		EditorGUILayout.Separator();
		
		return changed;
	}
	
	protected object ShowValueEditField(string ident, object fieldValue, string fieldName, bool hideNulls = false)
	{
		string fieldNameLabel = fieldName;
		
		string fieldIdentifier = ident + ".field." + fieldName;
		Font font = EditorStyles.standardFont;
		
		bool showChanges = ident.StartsWith("showGlobals");
		if (showChanges)
		{
			if (originalValues.Keys.Contains(fieldIdentifier))
			{
				fieldNameLabel = "[" + originalValues[fieldIdentifier] + "] " + fieldName;
				font = EditorStyles.boldFont;
			}
			else
			{
				foreach (var key in originalValues.Keys)
				{
					if (key.StartsWith(fieldIdentifier))
					{
						fieldNameLabel = "*" + fieldName;
					}
				}
			}
		}
		
		if (fieldValue is int)
		{
			GUIStyle style = EditorStyles.numberField;
			style.font = font;
			int newVal = EditorGUILayout.IntField(fieldNameLabel, (int)fieldValue);
			
			if (showChanges)
			{
				if (newVal != (int) fieldValue)
				{
					if (!originalValues.ContainsKey(fieldIdentifier))
					{
						originalValues[fieldIdentifier] = fieldValue;
					}
				}
				else if (originalValues.ContainsKey(fieldIdentifier))
				{
					if ((int) originalValues[fieldIdentifier] == newVal)
					{
						originalValues.Remove(fieldIdentifier);
					}
				}
			}
			return newVal;
		}
		else if (fieldValue is float)
		{
			GUIStyle style = EditorStyles.numberField;
			style.font = font;
			float newVal = EditorGUILayout.FloatField(fieldNameLabel, (float)fieldValue);
			
			if (showChanges)
			{
				if (newVal != (float) fieldValue)
				{
					if (!originalValues.ContainsKey(fieldIdentifier))
					{
						originalValues[fieldIdentifier] = fieldValue;
					}
				}
				else if (originalValues.ContainsKey(fieldIdentifier))
				{
					if ((float) originalValues[fieldIdentifier] == newVal)
					{
						originalValues.Remove(fieldIdentifier);
					}
				}
			}
			
			return newVal;
		}
		else if (fieldValue is double)
		{
			GUIStyle style = EditorStyles.numberField;
			style.font = font;
			try
			{
				double newVal = EditorGUILayout.FloatField(fieldNameLabel, (float)fieldValue);
				
				if (showChanges)
				{
					if (newVal != (float) fieldValue)
					{
						if (!originalValues.ContainsKey(fieldIdentifier))
						{
							originalValues[fieldIdentifier] = (float) fieldValue;
						}
					}
					else if (originalValues.ContainsKey(fieldIdentifier))
					{
						if ((float) originalValues[fieldIdentifier] == newVal)
						{
							originalValues.Remove(fieldIdentifier);
						}
					}
				}
				
				
				return newVal;
			}
			catch (Exception e)
			{
				EditorGUILayout.LabelField(fieldNameLabel, "" + fieldValue+", "+e.Message);
				return fieldValue;
			}
		}
		else if (fieldValue is bool)
		{
			GUIStyle style = EditorStyles.toggle;
			style.font = font;
			bool newVal = EditorGUILayout.Toggle(fieldNameLabel, (bool)fieldValue);
			
			if (showChanges)
			{
				if (newVal != (bool) fieldValue)
				{
					if (!originalValues.ContainsKey(fieldIdentifier))
					{
						originalValues[fieldIdentifier] = fieldValue;
					}
				}
				else if (originalValues.ContainsKey(fieldIdentifier))
				{
					if ((bool) originalValues[fieldIdentifier] == newVal)
					{
						originalValues.Remove(fieldIdentifier);
					}
				}
			}
			
			
			return newVal;
		}
		else if (fieldValue == null)
		{
		    if (!hideNulls)
		    {
		        EditorGUILayout.LabelField(fieldNameLabel, "NULL");
		    }
		    return null;
		}
		else if (fieldValue is string)
		{
			GUIStyle style = EditorStyles.textField;
			style.font = font;
			string newString = EditorGUILayout.TextField(fieldNameLabel, (string)fieldValue);
			
			if (showChanges)
			{
				if (newString != (string) fieldValue)
				{
					if (!originalValues.ContainsKey(fieldIdentifier))
					{
						originalValues[fieldIdentifier] = fieldValue;
					}
				}
				else if (originalValues.ContainsKey(fieldIdentifier))
				{
					if ((string) originalValues[fieldIdentifier] == newString)
					{
						originalValues.Remove(fieldIdentifier);
					}
				}
			}
			
			
			return newString;
		}
		else if (fieldValue is Color)
		{
			Color newVal = EditorGUILayout.ColorField(fieldNameLabel, (Color)fieldValue);
			return newVal;
		}
		else if (fieldValue is long || fieldValue is ulong)
		{
			var timestamp = fieldValue is ulong ? (long)(ulong)fieldValue : (long)fieldValue;
			fieldName = fieldName + " - " + DateUtils.UnixMsToDateTime(timestamp).ToString("u");
			
			object returnVal = null;
			object nestedObj = FoldoutFields(fieldIdentifier, fieldName, fieldValue, hideNulls);
			returnVal = nestedObj;
			return returnVal;
		}
		else if (fieldValue.GetType().IsEnum)
		{
			var newVal = EditorGUILayout.EnumPopup(fieldNameLabel, (Enum)fieldValue);
			return newVal;
		}
		else
		{
			object returnVal = null;
		    fieldName = ConsiderModifyingFieldName(fieldName, fieldValue);
			object nestedObj = FoldoutFields(fieldIdentifier, fieldName, fieldValue, hideNulls);
			returnVal = nestedObj;
			return returnVal;
		}
	}

    protected virtual string ConsiderModifyingFieldName(string fieldName, object fieldValue)
    {
        return fieldName;
    }

    protected void ShowMethodsForObject(System.Object obj, string identifier)
	{
		Type type = obj.GetType();
		ShowMethodsForType(obj, type, identifier);
	}
	
	protected void ShowMethodsForType(object obj, Type type, string identifier)
	{
		BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
		if (obj == null)
		{
			flags = BindingFlags.Static | BindingFlags.Public;
		}
		
		Type baseType = type;
		
		while (baseType != null)
		{
			List<MethodInfo> allMethods =
				new List<MethodInfo>(
					baseType.GetMethods(flags));
			
			if (allMethods.Count > 0)
			{
				EditorGUILayout.LabelField("*** " + baseType.Name + ":");
				DoMethods(obj, baseType, identifier, allMethods);
			}
			
			baseType = baseType.BaseType;
		}
	}
	
	protected void DoMethods(object obj, Type type, string identifier, List<MethodInfo> allMethods)
	{
		List<MethodInfo> publicMethods = new List<MethodInfo>();
		List<MethodInfo> privateMethods = new List<MethodInfo>();
		
		foreach (var methodInfo in allMethods)
		{
			if (methodInfo.IsPublic)
			{
				publicMethods.Add(methodInfo);
			}
			else
			{
				privateMethods.Add(methodInfo);
			}
		}
		
		if (publicMethods.Count > 0)
		{
			EditorGUILayout.LabelField("Public Methods:");
			
			var methods = publicMethods.OrderBy(x => x.Name).ToList();
			
			DoMethodList(obj, type, identifier, methods);
		}
		
		if (privateMethods.Count > 0)
		{
			EditorGUILayout.LabelField("Non-Public Methods:");
			
			var privMethods = privateMethods.OrderBy(x => x.Name).ToList();
			
			DoMethodList(obj, type, identifier, privMethods);
		}
		
		EditorGUILayout.Separator();
	}
	
	protected void DoMethodList(object obj, Type type, string identifier, List<MethodInfo> methods)
	{
		foreach (MethodInfo method in methods)
		{
			if (method.IsSpecialName)
			{
				continue;
			}
			string methodName = method.Name + " (";
			int numParams = 0;
			
			foreach (ParameterInfo info in method.GetParameters())
			{
				Type paramType = info.ParameterType;
				if (numParams > 0)
				{
					methodName += ", ";
				}
				numParams++;
				methodName += paramType.Name +  "," + info.Name;
				
				if (paramType.IsByRef)
				{
					paramType = paramType.GetElementType();
				}
			}
			methodName += ") : " + method.ReturnType.Name;
			
			
			DoFoldout(identifier + ".method." + methodName, methodName,
			          delegate() { DisplayMethodContents(obj, type, method, identifier + ".method." + methodName); });
		}
	}
	
	protected void DisplayMethodContents(object obj, Type type, MethodInfo method, string identifier)
	{
		object[] parametersToUse = new object[method.GetParameters().Length];
		Dictionary<string, int> outParams = new Dictionary<string, int>();
		
		int index = 0;
		foreach (ParameterInfo info in method.GetParameters())
		{
			bool byRef = false;
			Type paramType = info.ParameterType;
			if (paramType.IsByRef)
			{
				paramType = paramType.GetElementType();
				byRef = true;
			}
			
			string id = type.Name + "." + method.Name + ":" + info.Name;

//			EditorGUILayout.LabelField(paramType+", isEnum "+paramType.IsEnum);

			if (paramType == typeof (int))
			{
				int val = 0;
				if (parameterLists.ContainsKey(id) && parameterLists[id] is int)
				{
					val = (int) parameterLists[id];
				}
				val = EditorGUILayout.IntField(info.Name, val);
				parameterLists[id] = val;
				parametersToUse[index] = val;
			}
			else if (paramType == typeof (double))
			{
				double val = 0;
				if (parameterLists.ContainsKey(id) && parameterLists[id] is double)
				{
					val = (double) parameterLists[id];
				}
				val = EditorGUILayout.FloatField(info.Name, (float) val);
				parameterLists[id] = val;
				parametersToUse[index] = val;
			}
			else if (paramType == typeof (float))
			{
				float val = 0;
				if (parameterLists.ContainsKey(id) && parameterLists[id] is float)
				{
					val = (float) parameterLists[id];
				}
				val = EditorGUILayout.FloatField(info.Name, val);
				parameterLists[id] = val;
				parametersToUse[index] = val;
			}
			else if (paramType == typeof (string))
			{
				string val = "";
				if (parameterLists.ContainsKey(id) && parameterLists[id] is string)
				{
					val = (string) parameterLists[id];
				}
				val = EditorGUILayout.TextField(info.Name, val);
				parameterLists[id] = val;
				parametersToUse[index] = val;
			}
			else if (paramType == typeof (bool))
			{
				bool val = false;
				if (parameterLists.ContainsKey(id) && parameterLists[id] is bool)
				{
					val = (bool) parameterLists[id];
				}
				val = EditorGUILayout.Toggle(info.Name, val);
				parameterLists[id] = val;
				parametersToUse[index] = val;
			}
			else if (paramType == typeof(Color))
			{
				Color val = new Color(1,1,1);
				if (parameterLists.ContainsKey(id) && parameterLists[id] is Color)
				{
					val = (Color)parameterLists[id];
				}
				val = EditorGUILayout.ColorField(info.Name, val);
				parameterLists[id] = val;
				parametersToUse[index] = val;
			}
			else if (paramType.IsEnum)
			{
				object val = Enum.ToObject(paramType,0);
				if (parameterLists.ContainsKey(id) && parameterLists[id].GetType() == paramType)
				{
					val = parameterLists[id] as object;
				}
				val = EditorGUILayout.EnumPopup(info.Name, val as Enum);
				parameterLists[id] = val;
				parametersToUse[index] = val;
			}
			else
			{
				parameterLists[id] = null;
				parametersToUse[index] = null;
				EditorGUILayout.LabelField(info.Name, "NULL");
			}
			
			if (byRef)
			{
				outParams[id] = index;
			}
			index++;
		}
		
		string methodId = type.Name + "." + method.Name;
		string buttonText = methodId + "(";
		foreach (object param in parametersToUse)
		{
			if (param == null)
			{
				buttonText += "NULL";
			}
			else
			{
				buttonText += param.ToString();
			}
			
			buttonText += ", ";
		}
		if (parametersToUse.Count() > 0)
		{
			buttonText = buttonText.Substring(0, buttonText.Length - 2);
		}
		buttonText += ")";
		
		if (GUILayout.Button(buttonText))
		{
			object returnVal = method.Invoke(obj, parametersToUse);
			if (returnVal != null)
			{
				returnLists[methodId] = returnVal;
			}
			
			foreach (string paramKey in outParams.Keys)
			{
				int paramIndex = outParams[paramKey];
				parameterLists[paramKey] = parametersToUse[paramIndex];
			}
		}
		
		if (returnLists.ContainsKey(methodId))
		{
			EditorGUILayout.LabelField("Returned: ", returnLists[methodId].ToString());
			{
				ShowFieldsForObject(returnLists[methodId], identifier + ".Return." + methodId);
			}
		}
		EditorGUILayout.Separator();
	}

	protected void ShowSelectedGameObjects()
	{
		if (Selection.gameObjects != null)
		{
			EditorGUILayout.Separator();

			if (Selection.gameObjects.Length > 0)
			{
				EditorGUILayout.LabelField("Selected Objects:");
			}

			int index = 0;
			foreach (var obj in Selection.gameObjects)
			{
				EditorGUILayout.Separator();
				ShowGameObject(obj, "SelectedObj"+index);
				EditorGUILayout.Separator();
				index++;
			}
			EditorGUILayout.Separator();
		}
	}

	private void ShowGameObject(GameObject obj, string index)
	{
		DoFoldout("SelectedGameObject" + index, obj.name, delegate()
		{
			DisplayGameObject(obj, index);

		});
	}

	private void DisplayGameObject(GameObject obj, string index)
	{
		FoldoutFields("SelectedGameObject" + index + "TopLevel", "GameObject", obj);

		int subIndex = 0;
		foreach (var component in obj.GetComponents(typeof(Component)))
		{
			subIndex++;
			FoldoutFields("SelectedGameObject" + index + ".Component." + subIndex, component.GetType().Name + " Component", component);
		}
	}
}

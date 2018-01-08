using UnityEditor;
using UnityEngine;

public class Nudger : Editor 
{
	[MenuItem ("GameObject/Nudge/Nudge 10px Left #4")]
	static void Nudge10pxLeft() 
	{	
		Nudge(new Vector3(-10f, 0f));
	}
	
	[MenuItem ("GameObject/Nudge/Nudge 10px Right #6")]
	static void Nudge10pxRight() 
	{	
		Nudge(new Vector3(10f, 0f));
	}
	
	[MenuItem ("GameObject/Nudge/Nudge 10px Up #8")]
	static void Nudge10pxUp() 
	{	
		Nudge(new Vector3(0f, 10f));
	}
	
	[MenuItem ("GameObject/Nudge/Nudge 10px Down #2")]
	static void Nudge10pxDown() 
	{	
		Nudge(new Vector3(0f, -10f));
	}
	
	[MenuItem ("GameObject/Nudge/Nudge Left &4")]
	static void NudgeLeft() 
	{	
		Nudge(new Vector3(-1f, 0f));
	}
	
	[MenuItem ("GameObject/Nudge/Nudge Right &6")]
	static void NudgeRight() 
	{	
		Nudge(new Vector3(1f, 0f));
	}
	
	[MenuItem ("GameObject/Nudge/Nudge Up &8")]
	static void NudgeUp() 
	{	
		Nudge(new Vector3(0, 1f));
	}
	
	[MenuItem ("GameObject/Nudge/Nudge Down &2")]
	static void NudgeDown() 
	{	
		Nudge(new Vector3(0, -1f));
	}

	static void Nudge(Vector3 offset)
	{
		if(Selection.gameObjects != null)
		{
			foreach(GameObject obj in Selection.gameObjects)
			{
				if (obj != null && obj.transform != null) 
				{
					Vector3 localPos = obj.transform.localPosition;
					localPos += offset;
					obj.transform.localPosition = localPos;
				}
			}
		}
	}
}
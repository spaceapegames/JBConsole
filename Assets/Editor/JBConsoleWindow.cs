using UnityEngine;
using UnityEditor;
using System.Collections;

public class JBConsoleWindow : EditorWindow
{
	
	void Start () {
	
	}
	
	[MenuItem ("Window/JunkByte Console")]
    static void OpenJunkByteConsoleWindow()
	{
    	EditorWindow.GetWindow(typeof(JBConsoleWindow), false, "JB Console");
    }
	
    void Update ()
	{
		Repaint();
    }
	
	void OnGUI()
	{
		JBConsole console = JBConsole.instance;
		if(console == null)
		{
			GUILayout.Label("Console will run in play mode...");
		}
		else
		{
			if(console.visible)
			{
				GUILayout.Label("JunkByte Console visible in game.");
				if(GUILayout.Button("Show"))
				{
					console.visible = false;
				}
			}
			else
			{
				console.DrawGUI(position.width, position.height);
			}
		}
	}
}

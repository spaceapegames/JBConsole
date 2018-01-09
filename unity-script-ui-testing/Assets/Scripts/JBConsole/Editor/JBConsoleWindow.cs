using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using com.spaceape.jbconsole;

public class JBConsoleWindow : EditorWindow
{
	const string ASSETPATH_FONTREFERENCE = "Assets/Resources/JBConsoleFontReference.asset";
	const string ASSETPATH_JBCONSOLECONFIG = "Assets/Resources/JBConsoleConfig.asset";

	void Start () {
	
	}
	
	[MenuItem ("Window/JunkByte Console")]
    static void OpenJunkByteConsoleWindow()
	{
    	EditorWindow.GetWindow(typeof(JBConsoleWindow), false, "JB Console");
    }

	private void OnEnable()
	{
		TrySetupData();
	}

	private static void TrySetupData()
	{
		// first remove the old font reference
		AssetDatabase.DeleteAsset(ASSETPATH_FONTREFERENCE);
		
		var consoleConfig = AssetDatabase.LoadAssetAtPath<JBConsoleConfig>(ASSETPATH_JBCONSOLECONFIG);
		if (!consoleConfig)
		{
			InstallConfig();
		}
	}
	
	[MenuItem("SpaceApe/JBConsole/Install Config")]
	static void InstallConfig()
	{
		var config = ScriptableObject.CreateInstance<JBConsoleConfig>();
		var selectConfig = false;
		var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Plugins/Nuget/SpaceApe.JBConsole/font/RobotoMono-Regular.ttf");
		if (font)
		{
			var useMonoSpace = EditorUtility.DisplayDialog("JBConsole Monospace Font",
				"Do you want to use the JBConsole monospace font?\n\t"
				+ (font.fontNames != null && font.fontNames.Length > 0 ? font.fontNames[0] : font.name)
				+ "\nIt would be added linked to your resources folder and available in the game.",
				"Yes, thanks so much!",
				"No, I will do it myself/ignore");
			if (useMonoSpace)
			{
				config.font = font;
			}
			else
			{
				selectConfig = true;
			}
		}

		var ui = AssetDatabase.LoadAssetAtPath<JBConsoleUI>("Assets/Plugins/Nuget/SpaceApe.JBConsole/ui/JBConsoleUI.prefab");
		if (ui)
		{
			config.consoleUI = ui.gameObject;
		}
		
		AssetDatabase.CreateAsset(config, ASSETPATH_JBCONSOLECONFIG);

		if (!selectConfig)
		{
			Selection.activeObject = config;
		}
	}

	void Update ()
	{
		Repaint();
    }
	
	void OnGUI()
	{
		if(!Application.isPlaying)
		{
			GUILayout.Label("Console will be available during play mode.");
			return;
		}
		if(JBConsole.Exists)
		{
			JBConsole console = JBConsole.instance;
			if(console.Visible)
			{
				GUILayout.Label("JunkByte Console Visible in game.");
				if(GUILayout.Button("Show"))
				{
					console.Visible = false;
				}
			}
			else
			{
				console.DrawGUI(position.width, position.height, 1, true);
			}
		}
		else
		{
			GUILayout.Label("Console not initialized...");
			if(GUILayout.Button("Initialize"))
			{
				JBConsole.Start();
				JBConsole.instance.Visible = true;
			}
		}
	}
}

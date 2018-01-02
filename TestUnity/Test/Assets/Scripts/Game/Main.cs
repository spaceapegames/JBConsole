using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
	private bool toggle1 = false;
	
	private void Start () 
	{
		JBConsole.AddMenu("Root B1", () =>
		{
			Logger.Debug("Root B1");
		});
		
		JBConsole.AddToggle("Root T1", delegate
		{
			toggle1 = !toggle1;
		}, delegate
		{
			return toggle1;
		});

		
		Logger.DebugCh("BLAH", "HELLO CHANNEL BLAH");
		Logger.WarnCh("WEEE", "HELLO CHANNEL WEEE");
		Logger.ErrorCh("MOOO", 0, "HELLO CHANNEL MOOO");
	}

}

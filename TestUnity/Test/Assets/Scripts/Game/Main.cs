using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour 
{
	private void Start () 
	{
		JBConsole.AddMenu("Root B1", () =>
		{
			Logger.Debug("Root B1");
		});
		
		JBConsole.AddMenu("Console/zoom--", delegate
		{
			JBConsole.instance.BaseDPI += 10;
		});

		JBConsole.AddMenu("Console/zoom++", delegate
		{
			JBConsole.instance.BaseDPI -= 10;
		});
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
	private bool autoLog = false;
	private List<bool> toggles = new List<bool>();
	private Coroutine autoLogCoroutine = null;
	
	private void Start ()
	{
		Application.targetFrameRate = 60;
		
		JBConsole.AddMenu("Root B1", () =>
		{
			Logger.Debug("Root B1");
		});
		
		JBConsole.AddToggle("Auto Log", delegate
		{
			autoLog = !autoLog;
			if (autoLog)
			{
				autoLogCoroutine = StartCoroutine(SlowLogger());
			}
			else if (autoLogCoroutine != null)
			{
				StopCoroutine(autoLogCoroutine);
			}
		}, delegate
		{
			return autoLog;
		});

		var toggleCount = 0;
		for (var i = 0; i < 20; i++)
		{
			var closedIndex = i;
			if (i % 2 == 0)
			{
				JBConsole.AddMenu("Root F1/"+"B"+(i+1), delegate
				{
					Logger.Debug("Root F1/"+(closedIndex+1).ToString());
				});
			}
			else
			{
				toggles.Add(false);
				var toggleIndex = toggleCount;
				JBConsole.AddToggle("Root F1/"+"T"+(i+1), delegate
				{
					toggles[toggleIndex] = !toggles[toggleIndex];
				}, delegate
				{
					return toggles[toggleIndex];
				});;
				toggleCount++;
			}
		}
		
		Logger.DebugCh("BLAH", "HELLO CHANNEL BLAH");
		Logger.WarnCh("WEEE", "HELLO CHANNEL WEEE");
		//Logger.ErrorCh("MOOO", 0, "HELLO CHANNEL MOOO");

		for (var i = 0; i < 500; i++)
		{
			var textIndex = Random.Range(0, texts.Length);
			var numRepeats = Random.Range(1, 1);
			var text = "";
			for (var j = 0; j < numRepeats; j++)
			{
				text += texts[textIndex];
			}
			Logger.DebugCh("BLAH", text);
		}
		
		
		//StartCoroutine(SlowLogger());
	}

	string[] texts = new[]
	{
		"Lots of stuff wee wee weeeee",
		"Some more text now here's a load of stuff",
		"blee blooo, wiggle giggle blonk blonk nibbles",
		"Eat your foot.",
		"I hate the green flashing light.",
		"Save a tree, eat a beaver.",
		"Ha ha! I don’t get it.",
		"I do whatever my Rice Crispies tell me to do",
		"Foaming At The Mouth",
		"An Arm and a Leg",
		"Sixty-Four comes asking for bread.",
		"The memory we used to share is no longer coherent."
	};
	
	IEnumerator SlowLogger()
	{
		int count = 0;
		while (true)
		{
			var type = Random.Range(0, 3);
			var text = texts[Random.Range(0, texts.Length)];
			switch (type)
			{
				case 0:
					Logger.DebugCh("TDebug", text);
					break;
				case 1:
					Logger.InfoCh("TInfo", text);
					break;
				case 2:
					Logger.WarnCh("TWarn", text);
					break;
						
			}
			yield return new WaitForSeconds(1.0f);
		}
	}
	
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.B))
		{
			Logger.DebugCh("SLOW", "LOG " + System.DateTime.Now.ToFileTime().ToString());
		}
	}
}

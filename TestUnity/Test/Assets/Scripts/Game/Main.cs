using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
	private bool toggle1 = false;
	private List<bool> toggles = new List<bool>();
	
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
		Logger.ErrorCh("MOOO", 0, "HELLO CHANNEL MOOO");

		var texts = new[]
		{
			"Lots of stuff wee wee weeeee",
			"Some more text now here's a load of stuff",
			"blee blooo, wiggle giggle blonk blonk nibbles"
		};
		for (var i = 0; i < 150; i++)
		{
			var textIndex = Random.Range(0, texts.Length);
			var numRepeats = Random.Range(1, 5);
			var text = "";
			for (var j = 0; j < numRepeats; j++)
			{
				text += texts[textIndex];
			}
			Logger.DebugCh("BLAH", text);
		}
	}

}

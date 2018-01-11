using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
	[SerializeField] private GameLogging gameLogging = null;

	private bool autoLog = false;
	private List<bool> toggles = new List<bool>();
	private Coroutine autoLogCoroutine = null;

	private IEnumerator DoDemo()
	{
		yield return new WaitForSeconds(1.0f);
		JBConsole.instance.Visible = true;
		yield return ShowStory(story1, 0);	
		gameLogging.StartUnityUIConsole();
		yield return ShowStory(story2, 1);
		
		yield return new WaitForSeconds(20.0f);

		Logger.Warn("UX PLEASE RESKIN ME!");
		
		yield return new WaitForSeconds(2.0f);

		Logger.Warn("OK BYE!");
		yield return new WaitForSeconds(2.0f);

		for (var i = 0; i < 50; i++)
		{
			if (i % 2 == 0)
			{
				Logger.Warn(".");				
			}
			else
			{
				Logger.Warn(" ");								
			}
			yield return new WaitForSeconds(0.2f);			
		}

	}

	private bool toggle = false;
	
	private void Start ()
	{
		Application.targetFrameRate = 60;

		StartCoroutine(DoDemo());

		JBConsole.AddMenu("There's Folders...", () =>
		{
			Logger.Debug("Root B1");
		});

		JBConsole.AddMenu("There's Folders.../And Buttons", () =>
		{
			Logger.Debug("Root B1");
		});

		JBConsole.AddMenu("There's Folders.../And Buttons", () =>
		{
			Logger.Debug("Root B1");
		});

		JBConsole.AddMenu("There's Folders.../And Buttons/I'm a button", () =>
		{
			Logger.Debug("Root B1");
		});

		JBConsole.AddMenu("There's Folders.../And Buttons/And Toggles", () =>
		{
			Logger.Debug("Root B1");
		});

		JBConsole.AddToggle("There's Folders.../And Buttons/And Toggles/I'm A Toggle", () => { toggle = !toggle; },
			() => { return toggle;});

		
		/*
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

		PooledList p = null;
		*/
	}

	IEnumerator ShowStory(string[] story, int output)
	{
		var index = 0;
		while (index < story.Length)
		{
			switch (output)
			{
				case 0:
				{
					Logger.Debug(story[index++]);					
				} break;
					
				case 1:
				{
					Logger.Info(story[index++]);					
				} break;

			}
			yield return new WaitForSeconds(2.0f);
		}
	}
	
	private string[] story1 = new[]
	{
		"Recognise this?...",
		"It's that console that's always just too small to be useful...",
		"...and man those buttons....who's fingers can actually touch those?",
		"well....",
		"hold onto your butts...."
	};
	
	private string[] story2 = new[]
	{
		"ZOMG etc.",
		"Look! Its all unity UI!!",
		"..."
	};

	
	
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

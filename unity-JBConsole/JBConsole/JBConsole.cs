using UnityEngine;
using System.Collections.Generic;
using System;
using com.spaceape.jbconsole;

public delegate void JBConsoleMenuHandler();
public delegate void JBCDrawBodyHandler(float width, float height, float scale = 1);
public delegate void JBCLogSelectedHandler(ConsoleLog log);

public struct JBConsoleStateMenuItem
{
	public enum VisualType
	{
		Button,
		Toggle,
		Folder,
		ParentFolder,
	}
	
	public string Text;
	public bool ToggleValue;
	public VisualType Visual;
	public int Index;
	public JBCustomMenu.MenuItem MenuItem;
}

public struct JBConsoleState
{
	public int CurrentToolbarIndex;
	public JBConsoleStateMenuItem[] Menu;
	public string SearchTerm;
	
	public ConsoleMenu? CurrentConsoleMenu
	{
		get
		{
			if (CurrentToolbarIndex == -1)
			{
				return null;
			}
			return (ConsoleMenu) CurrentToolbarIndex;
		}
	}
}

public class JBConsole : MonoBehaviour
{
	public static bool isEditor = false;

    public static ConsoleLog ToastLog;
    public static float ToastExpiry;

    private const int baseFontSize = 14;
    delegate void SubMenuHandler(int index);

	public static JBConsole Start(bool visible = true)
	{
		if(instance == null)
		{
			var go = new GameObject("JBConsole");
			instance = go.AddComponent<JBConsole>();			
			instance.Visible = visible;
			JBConsole.isEditor = Application.isEditor;
		}
		return instance;
	}

    public static JBConsole instance { get; private set; }

    public static bool Exists
	{
		get { return instance != null; }
    }

	public static void AddMenu(string name, Action callback)
    {
        if (!Exists) return;
		instance.Menu.AddButton(name, callback);
    }

    public static void RemoveMenu(string name)
    {
        if (!Exists) return;
		instance.Menu.Remove(name);
    }

	public static void AddToggle(string name, Action toggleCallback, Func<bool> getterCallback)
	{
		if (!Exists) return;
		instance.Menu.AddToggle(name, toggleCallback, getterCallback);
	}

    public static bool ActivateMenu(string name)
    {
        if (!Exists) return false;
        return instance.Menu.Activate(name);
    }

    private JBLogger logger;
	public Font Font { get; private set; }
    private JBCStyle _style;
    public JBCStyle style { get { if (_style == null) _style = new JBCStyle(Font ?? JBConsoleConfig.GetDefaultFont()); return _style; } }

	public void SetFont(Font font)
	{
		Font = font;
		if (_style != null) // style can only be created inside OnGUI :facepalm:
		{
			_style.SetFont(font);
		}
	}

	private List<JBConsoleExternalUI> externalUIs = null;
	
    public int menuItemWidth = 135;
    public int BaseDPI = 100;
    private bool _visible = true;
	public bool Visible
	{
		get { return _visible; }
		set
		{
			_visible = value;

			var state = State;
			ExternalUIAction((ui) => ui.SetActive(_visible, state));
	        
			if (OnVisiblityChanged != null) OnVisiblityChanged();
		}
	}
	
	public JBCustomMenu Menu {get; private set;}

	public event Action OnVisiblityChanged;

    JBCDrawBodyHandler DrawGUIBodyHandler;
    public JBCLogSelectedHandler OnLogSelectedHandler;
	
    string[] levels;
    string[] topMenu;

    ConsoleLevel viewingLevel = ConsoleLevel.Debug;
    string[] viewingChannels;

    int currentTopMenuIndex = -1;
    string[] currentTopMenu;
    string[] currentSubMenu;
    SubMenuHandler subMenuHandler;
	
	List<ConsoleLog> cachedLogs;

    string searchTerm = "";

	bool autoScrolling = true;
	
	Rect autoscrolltogglerect = new Rect(0, 0, 110, 22);

    Vector2 scrollPosition;
	Vector3 touchPosition;
	float scrollVelocity;
	bool scrolling;
	bool scrolled;

	int stateHash;

	void Awake ()
	{
		if(instance == null) instance = this;

	    gameObject.AddComponent<JBCInspector>();
		
		DontDestroyOnLoad(gameObject);

        logger = JBLogger.instance;
		if (logger != null)
		{
			logger.ChannelAdded += HandleChannelAdded;
		}
		
		Menu = new JBCustomMenu();
        levels = Enum.GetNames(typeof(ConsoleLevel));
        topMenu = currentTopMenu = Enum.GetNames(typeof(ConsoleMenu));
		Menu.AddButton("Clear", Clear);
	}

	private void OnDestroy()
	{
		if (logger != null)
		{
			logger.ChannelAdded -= HandleChannelAdded;
		}
	}

	void Clear()
	{
		if(logger != null)
		{
			scrolling = false;
			autoScrolling = true;
			logger.Clear();
		}
	}
	
	void Update ()
    {
		if (logger == null) return;
		
		if(autoScrolling && logger.stateHash != stateHash)
		{
			stateHash = logger.stateHash;
			clearCache();
		}

		if(Visible && _style != null && Input.GetMouseButton(0))
		{
			if(!scrolling && Input.mousePosition.y < Screen.height - GetMenuHeight(GetGuiScale()))
			{
				clearCache();
				if(autoScrolling) scrollPosition.y = int.MaxValue;
				autoScrolling = false;
				scrolling = true;
				scrolled = false;
				touchPosition = Input.mousePosition;
			}
		}
		else 
		{
			scrolling = false;
		}
		if(scrolling)
		{
			var touch = Input.mousePosition;
			scrollVelocity = (touch - touchPosition).y;
			scrollPosition.y += scrollVelocity;
			touchPosition = touch;
			scrolled |= Mathf.Abs(scrollVelocity) > 3f;
		}
	}
	
    void OnMenuSelection(int index)
    {
        if (currentTopMenuIndex == index)
        {
            index = -1;
        }
        switch (index)
        {
            case (int)ConsoleMenu.Channels:
	            currentSubMenu = null;
                UpdateChannelsSubMenu();
                subMenuHandler = OnChannelClicked;
                break;
            case (int)ConsoleMenu.Levels:
	            currentSubMenu = null;
                UpdateLevelsSubMenu();
                subMenuHandler = OnLevelClicked;
                break;
            case (int)ConsoleMenu.Search:
	            currentSubMenu = null;
                break;
            case (int)ConsoleMenu.Menu:
	            currentSubMenu = null;
				Menu.PopToRoot();
				subMenuHandler = Menu.OnCurrentLinkClicked;
                break;
            case (int)ConsoleMenu.Hide:
                Visible = !Visible;
                return;
			case -1:
				currentSubMenu = null;
				break;
        }

		EnsureScrollPosition();
        currentTopMenuIndex = index;
        currentTopMenu = SelectedStateArrayIndex(topMenu, index, true);

	    UpdateExternalUIState();
    }

    public void OnChannelClicked(int index)
    {
        string channel = logger.Channels[index];
        if (channel == JBLogger.allChannelsName)
        {
            viewingChannels = null;
        }
        else if (viewingChannels != null)
        {
            if (Array.IndexOf(viewingChannels, channel) >= 0)
            {
                if (viewingChannels.Length > 1) viewingChannels = JBConsoleUtils.StringsWithoutString(viewingChannels, channel);
                else viewingChannels = null;
            }
            else
            {
                JBConsoleUtils.AddToStringArray(ref viewingChannels, channel);
            }
        }
        else
        {
            viewingChannels = new string[] { channel };
        }
        UpdateChannelsSubMenu();
        clearCache();
	    LogRefreshNeeded();
	    UpdateExternalUIState();
    }

	private void HandleChannelAdded()
	{
		if (currentTopMenuIndex == (int) ConsoleMenu.Channels)
		{
			UpdateChannelsSubMenu();
			UpdateExternalUIState();
		}
	}

    public void OnLevelClicked(int index)
    {
        viewingLevel = (ConsoleLevel)Enum.GetValues(typeof(ConsoleLevel)).GetValue(index);
        UpdateLevelsSubMenu();
        clearCache();
	    LogRefreshNeeded();
	    UpdateExternalUIState();
    }

    void UpdateChannelsSubMenu()
    {
		var channels = logger.Channels.ToArray();
        currentSubMenu = new string[channels.Length];
        Array.Copy(channels, currentSubMenu, channels.Length);
        if (viewingChannels == null)
        {
            SelectedStateArrayIndex(currentSubMenu, 0, false);
        }
        else
        {
            string channel;
            for (int i = channels.Length - 1; i >= 0; i--)
            {
                channel = channels[i];
                if (Array.IndexOf(viewingChannels, channel) >= 0)
                {
                    SelectedStateArrayIndex(currentSubMenu, i, false);
                }
            }
        }
    }

    void UpdateLevelsSubMenu()
    {
        currentSubMenu = SelectedStateArrayIndex(levels, (int)viewingLevel, true);
    }

    string[] SelectedStateArrayIndex(string[] array, int index, bool copy)
    {
        if (index < 0) return array;
        string[] result;
        if (copy)
        {
            result = new string[array.Length];
            Array.Copy(array, result, array.Length);
        }
        else result = array;
		if (index >= result.Length) return result;
        result[index] = "["+ result[index]+"]";
        return result;
    }
	
	void OnGUI ()
	{
        if (HasExternalUI() || (!Visible && ToastLog == null)) return;            

        var depth = GUI.depth;
		GUI.depth = int.MaxValue - 10;
		
		var scale = GetGuiScale();
		style.SetScale(scale);

        if (ToastLog != null && !Visible)
        {
            ShowToast(Screen.width, Screen.height, scale);
        }
        else
        {
            if (!scrolling && Event.current.type == EventType.Layout)
            {
                scrollVelocity *= 0.95f;
                scrollPosition.y += scrollVelocity;
            }

            DrawGUI(Screen.width, Screen.height, scale);
        }

        GUI.depth = depth;
	}

	float GetGuiScale()
	{
		var scale = 1f;
		var dpi = Screen.dpi;
		if (JBConsole.isEditor)
		{
			if (dpi <= 0)
				dpi = 150;
		}
		else
		{
			if (dpi > 0 && BaseDPI > 0)
				scale = dpi / BaseDPI;
		}

		return scale;
	}

	float GetMenuHeight(float scale)
	{
		return style.MenuStyle.fontSize + (10 * scale);
	}
	
	public void DrawGUI(float width, float height, float scale = 1, bool showScrollBar = false)
    {
        GUILayout.BeginVertical(style.BoxStyle);

		var menuheight = GetMenuHeight(scale);

        int selection = GUILayout.Toolbar(-1, currentTopMenu, style.MenuStyle, GUILayout.MinWidth(320 * scale), GUILayout.MaxWidth(Screen.width), GUILayout.Height(menuheight));
        if (selection >= 0)
        {
            Defocus();
            OnMenuSelection(selection);
        }

		if(currentTopMenuIndex == (int)ConsoleMenu.Menu)
		{
			currentSubMenu = Menu.GetCurrentMenuLink();
		}
        
	    if (currentSubMenu != null)
        {
			if(currentSubMenu.Length == 0)
			{
				GUILayout.Label("No Custom Menus...");
			}
			else
			{
			    var count = (int) (width/(menuItemWidth*scale));
                var rows = Mathf.Ceil((float) currentSubMenu.Length / (float)count);
                selection = GUILayout.SelectionGrid(-1, currentSubMenu, count, style.MenuStyle, GUILayout.Height(menuheight * rows));
	            if (selection >= 0 && subMenuHandler != null)
	            {
	                Defocus();
	                subMenuHandler(selection);
	            }
			}
        }

        if (currentTopMenuIndex == (int)ConsoleMenu.Search)
        {
            GUI.SetNextControlName("SearchTF");
			string newTerm = GUILayout.TextField(searchTerm, style.SearchStyle);
			if(newTerm != searchTerm)
			{
                Defocus();
            	searchTerm = newTerm.ToLower();
				clearCache();
			}
            
            GUI.FocusControl("SearchTF");
        }

        if (DrawGUIBodyHandler == null)
        {
			DrawLogScroll(width, height, showScrollBar);
        }
        else
        {
            DrawGUIBodyHandler(width, height, scale);
        }

	    GUILayout.EndVertical();
	}

    void ShowToast(float width, float height, float scale)
    {
        var padding = 10*scale;
        var menuheight = GetMenuHeight(scale);

        var backStro = new GUIStyle(style.BoxStyle);
        var toastStyle = new GUIStyle();
        toastStyle.fontSize = (int) (14 * scale);
        toastStyle.normal.textColor = new Color(1f, 1f, 1f);
        toastStyle.wordWrap = true;
        toastStyle.alignment = TextAnchor.UpperLeft;
        backStro.stretchHeight = true;
        string textMsg1 = ToastLog.GetUnityLimitedMessage();
        if (textMsg1.Length > 200)
        {
            textMsg1 = textMsg1.Substring(0, 200) + "...";
        }

        var textHeight = toastStyle.CalcHeight(new GUIContent(textMsg1),width - (2*padding));

        GUILayout.BeginArea(new Rect(padding,padding,width-(2*padding),textHeight + menuheight), backStro);

        GUILayout.Label(textMsg1, toastStyle, new GUILayoutOption[] { GUILayout.MaxWidth(width) });

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("SHOW",style.MenuStyle))
        {
            ToastLog = null;
            Visible = true;
        }

        if (GUILayout.Button("DISMISS", style.MenuStyle))
        {
            ToastLog = null;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        if (Time.time > ToastExpiry)
        {
            ToastLog = null;
        }
    }

    void BeginScrollView(bool showScrollBar, GUILayoutOption options)
	{
		if(showScrollBar) scrollPosition = GUILayout.BeginScrollView(scrollPosition, options);
		else scrollPosition = GUILayout.BeginScrollView(scrollPosition, style.HiddenScrollBar, style.HiddenScrollBar, options);
	}

	private void DrawLogScroll(float width, float height, bool showScrollBar)
    {
        GUILayoutOption maxwidthscreen = GUILayout.MaxWidth(width);

        bool wasForcedBottom = scrollPosition.y == float.MaxValue;

        ConsoleLog clickedLog;
        if (autoScrolling)
        {
            if (cachedLogs == null && Event.current.type == EventType.Layout)
            {
                scrollPosition.y = float.MaxValue;
            }
			BeginScrollView(showScrollBar, maxwidthscreen);
            if (cachedLogs == null)
            {
                CacheBottomOfLogs(width, height);
            }
            clickedLog = PrintCachedLogs(maxwidthscreen);
        }
        else
        {
			BeginScrollView(showScrollBar, maxwidthscreen);
            if (cachedLogs == null)
            {
                CacheAllOfLogs();
            }
            clickedLog = PrintCachedLogs(maxwidthscreen);
        }
		bool hasLogs = cachedLogs.Count > 0;

		Rect lastContentRect = hasLogs ? GUILayoutUtility.GetLastRect() : new Rect();

        GUILayout.EndScrollView();

		if (!scrolling && hasLogs && Event.current.type == EventType.Repaint)
        {
			Rect scrollViewRect = GUILayoutUtility.GetLastRect();
            float maxscroll = lastContentRect.y + lastContentRect.height - scrollViewRect.height;

            bool atbottom = maxscroll <= 0 || scrollPosition.y > maxscroll - 4; // where 4 = scroll view's skin bound size?
            if (!autoScrolling && wasForcedBottom)
            {
                scrollPosition.y = maxscroll - 3;
            }
            else if (autoScrolling != atbottom)
            {
                autoScrolling = atbottom;
                scrollPosition.y = float.MaxValue;
                clearCache();
            }
		}

        if (!autoScrolling)
        {
            autoscrolltogglerect.x = width - autoscrolltogglerect.width;
            autoscrolltogglerect.y = height - autoscrolltogglerect.height;

            if (GUI.Button(autoscrolltogglerect, "Scroll to bottom"))
            {
				scrollVelocity = 0f;
                autoScrolling = true;
				clickedLog = null;
            }
		}

		if (clickedLog != null && OnLogSelectedHandler != null)
		{
			OnLogSelectedHandler(clickedLog);
			SetSelectedLogOnExternalUIs(clickedLog);
		}
    }
	
    public void Focus(JBCDrawBodyHandler drawBodyHandler)
    {
        DrawGUIBodyHandler = drawBodyHandler;
    }

    public void Defocus()
    {
        if(DrawGUIBodyHandler != null)
        {
            autoScrolling = true;
            scrollPosition.y = float.MaxValue;
            DrawGUIBodyHandler = null;
        }
    }

    void EnsureScrollPosition()
	{
		if(autoScrolling)
		{
			scrollPosition.y = float.MaxValue;
		}
	}
	
	ConsoleLog PrintCachedLogs(GUILayoutOption maxwidthscreen)
	{
		ConsoleLog log;
	    ConsoleLog clicked = null;
		for (int i = cachedLogs.Count - 1; i >= 0; i--)
		{
			log = cachedLogs[i];
			var repeats = log.GetRepeats();
			if(repeats > 0)
			{
                GUILayout.Label((repeats + 1) + "x " + log.GetUnityLimitedMessage(), style.GetStyleForLogLevel(log.level), maxwidthscreen);
			}
			else
			{

				GUILayout.Label(log.Time.ToLongTimeString() + "-" + log.GetUnityLimitedMessage(), style.GetStyleForLogLevel(log.level), maxwidthscreen);
            }
			if (!scrolled && Event.current.type == EventType.MouseUp && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                clicked = log;
            }
		}
	    return clicked;
	}
		
	bool ShouldShow(ConsoleLog log)
	{
		return (log.level >= viewingLevel 
			&& (viewingChannels == null || Array.IndexOf(viewingChannels, log.channel) >= 0)
	        && (searchTerm == "" || log.GetMessageLowercase().Contains(searchTerm.ToLower())));
	}
	
	void CacheBottomOfLogs(float width, float height)
	{
		//TODO, avoid needing to create new list.
		cachedLogs = new List<ConsoleLog>();
		List<ConsoleLog> logs = logger.Logs;
		ConsoleLog log;
		float lineHeight = style.GetLogLineHeight();
		for(int i = logs.Count - 1; i >= 0 && height > 0; i--)
		{
			log = logs[i];
			if (ShouldShow(log))
			{
				cachedLogs.Add(log);
				height -= lineHeight;
			}
		}
	}
	
	void CacheAllOfLogs()
	{
		//TODO, avoid needing to create new list.
		cachedLogs = new List<ConsoleLog>();
		List<ConsoleLog> logs = logger.Logs;
		ConsoleLog log;
		for(int i = logs.Count - 1; i >= 0; i--)
		{
			log = logs[i];
			if (ShouldShow(log))
			{
				cachedLogs.Add(log);
			}
		}
	}
	
	void clearCache()
    {
		cachedLogs = null;
	}


    public T RegisterPlugin<T>() where T : Component
	{
	    var comp = gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
	    return comp;
	}

	public bool HasExternalUI()
	{
		return externalUIs != null && externalUIs.Count > 0;
	}	
	
	private void ExternalUIAction(Action<JBConsoleExternalUI> action)
	{
		if (action != null && externalUIs != null)
		{
			foreach (var externalUI in externalUIs)
			{
				if (externalUI != null)
				{
					action(externalUI);
				}
			}				
		}
	}
	
	public void AddExternalUI(JBConsoleExternalUI externalUI)
	{
		if (externalUIs == null)
		{
			externalUIs = new List<JBConsoleExternalUI>();
		}

		if (!externalUIs.Contains(externalUI))
		{
			externalUIs.Add(externalUI);
			externalUI.AddToolbarButtonListener(ExternalUIToolbarButtonPressed);	
			externalUI.AddMenuButtonListener(ExternalUIMenuButtonPressed);
			externalUI.AddSearchTermChangedListener(ExternalUISearchTermChanged);
			externalUI.AddConsoleLogSelectedListener(ExternalUIConsoleLogSelected);
			externalUI.SetActive(_visible, State);
		}		
	}

	public void RemoveExternalUI(JBConsoleExternalUI externalUI)
	{
		if (externalUIs == null)
		{
			return;
		}

		if (externalUIs.Contains(externalUI))
		{
			externalUI.RemoveToolbarButtonListener(ExternalUIToolbarButtonPressed);		
			externalUI.RemoveMenuButtonListener(ExternalUIMenuButtonPressed);
			externalUI.RemoveSearchTermChangedListener(ExternalUISearchTermChanged);
			externalUI.AddConsoleLogSelectedListener(ExternalUIConsoleLogSelected);
			externalUIs.Remove(externalUI);
		}		
	}
	
	private void ExternalUIMenuButtonPressed(JBConsoleStateMenuItem menuItem)
	{
		switch (currentTopMenuIndex)
		{
			case (int)ConsoleMenu.Channels:
			{
				OnChannelClicked(menuItem.Index);
			} break;
				
			case (int)ConsoleMenu.Levels:
			{
				OnLevelClicked(menuItem.Index);
			} break;
			
			case (int)ConsoleMenu.Menu:
			{
				Menu.OnCurrentLinkClicked(menuItem.Index);
				UpdateExternalUIState();
			} break;
		}	
	}
	
	private void ExternalUIToolbarButtonPressed(ConsoleMenu? newConsoleMenu)
	{
		Defocus();
		int selectionIndex = -1;
		if (newConsoleMenu != null)
		{
			selectionIndex = (int) newConsoleMenu.Value;
		}
		OnMenuSelection(selectionIndex);	
	}

	private void ExternalUIConsoleLogSelected(ConsoleLog consoleLog)
	{
		if (consoleLog == null)
		{
			Defocus();
		}
		else
		{
			OnLogSelectedHandler(consoleLog);
		}
	}
	
	private void ExternalUISearchTermChanged(string searchTerm)
	{
		this.searchTerm = searchTerm;
		LogRefreshNeeded();
	}

	public void SetSelectedLogOnExternalUIs(ConsoleLog consoleLog)
	{
		ExternalUIAction((ui) => ui.LogSelected(consoleLog));
	}
	
	private void LogRefreshNeeded()
	{
		ExternalUIAction((ui) => ui.RefreshLog(viewingLevel, searchTerm, viewingChannels));
	}
	
	private void UpdateExternalUIState()
	{
		var state = State;
		ExternalUIAction((ui) => ui.StateChanged(state));
	}
	
	public JBConsoleStateMenuItem[] GetCurrentMenu()
	{
		JBConsoleStateMenuItem[] menu = null;
		switch (currentTopMenuIndex)
		{
			case (int)ConsoleMenu.Channels:
			{
				var channels = logger.Channels.ToArray();
				menu = new JBConsoleStateMenuItem[channels.Length];
				var hasViewingChannels = viewingChannels != null && viewingChannels.Length > 0;
				for (var i = 0; i < channels.Length; i++)
				{
					var currentlyViewingChannel = hasViewingChannels ? Array.IndexOf(viewingChannels, channels[i]) >= 0 : i == 0;

					var menuItem = new JBConsoleStateMenuItem()
					{
						Text = channels[i],
						ToggleValue = currentlyViewingChannel,
						Visual = JBConsoleStateMenuItem.VisualType.Toggle,
						Index = i,
					};
					menu[i] = menuItem;
				}				
			} break;
				
			case (int)ConsoleMenu.Levels:
			{
				menu = new JBConsoleStateMenuItem[levels.Length];
				for (var i = 0; i < levels.Length; i++)
				{
					var menuItem = new JBConsoleStateMenuItem()
					{
						Text = levels[i],
						ToggleValue = i == (int)viewingLevel,
						Visual = JBConsoleStateMenuItem.VisualType.Toggle,
						Index = i,
					};
					menu[i] = menuItem;
				}				
			} break;
			
			case (int)ConsoleMenu.Menu:
			{
				var currentMenuItem = Menu.GetCurrentMenuItem();
				if (currentMenuItem != null)
				{
					var menuItems = new List<JBConsoleStateMenuItem>();
					var startIndex = 0;
					if (currentMenuItem.HasParent())
					{
						menuItems.Add(new JBConsoleStateMenuItem()
						{
							Text = "<<",
							ToggleValue = false,
							Visual = JBConsoleStateMenuItem.VisualType.ParentFolder,
							Index = 0,
						});
						startIndex++;
					}
					if (currentMenuItem.Children != null)
					{
						for (var i = 0; i < currentMenuItem.Children.Count; i++)
						{
							var childItem = currentMenuItem.Children[i];
							var visualType = JBConsoleStateMenuItem.VisualType.Button;
							if (childItem.HasChildren())
							{
								visualType = JBConsoleStateMenuItem.VisualType.Folder;
							}
							else if (childItem.ItemType == JBCustomMenu.MenuItem.MenuItemType.Toggle)
							{
								visualType = JBConsoleStateMenuItem.VisualType.Toggle;
							}
							menuItems.Add(new JBConsoleStateMenuItem()
							{
								Text = childItem.GetRawLinkName(),
								ToggleValue = childItem.GetToggleValue(),
								Visual = visualType,
								Index = startIndex + i,
								MenuItem = childItem,
							});
						}
					}
					menu = menuItems.ToArray();
				}
				else
				{
					menu = new JBConsoleStateMenuItem[0];
				}
				/*
				var currentMenus = Menu.GetCurrentMenuLink();
				if (currentMenus != null)
				{
					menu = new JBConsoleStateMenuItem[currentMenus.Length];
					for (var i = 0; i < currentMenus.Length; i++)
					{
						var menuItem = new JBConsoleStateMenuItem()
						{
							Text = currentMenus[i],
							ToggleValue = false,
							Visual = JBConsoleStateMenuItem.VisualType.Button,
							Index = i,
						};
						menu[i] = menuItem;
					}
				}
				else
				{
					menu = new JBConsoleStateMenuItem[0];
				}
				*/
			} break;
		}
		return menu;	
	}
	
	public JBConsoleState State
	{
		get
		{
			JBConsoleStateMenuItem[] menu = GetCurrentMenu();
			return new JBConsoleState()
			{
				CurrentToolbarIndex = currentTopMenuIndex,
				SearchTerm = searchTerm,
				Menu = menu
			};
		}
	}
	
}

public enum ConsoleMenu
{
    Channels,
    Levels,
    Search,
    Menu,
    Hide
}
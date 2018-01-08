using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUI : MonoBehaviour, JBConsoleExternalUI
{
    [SerializeField] private JBConsoleUIToolbar toolbar = null;
    [SerializeField] private JBConsoleUILog log = null;
    [SerializeField] private JBConsoleUIMenuController menus = null;
    [SerializeField] private GameObject visibleRoot = null;
    [SerializeField] private JBConsoleUISearch search;
    [SerializeField] private JBConsoleUILogInspector logInspector = null;
    
    private ExternalUIToolbarButtonPressed OnToolbarButton = delegate { };
    private ExternalUIMenuButtonPressed OnMenuButton = delegate { };
    private ExternalUISearchTermChanged OnSearchTermChanged = delegate { };
    private ExternalUIConsoleLogSelected OnConsoleLogSelected = delegate { };
    
    private void Awake()
    {
        if (toolbar != null)
        {
            toolbar.OnToolbarChanged += ToolbarButtonSelected;
        }
        if (menus != null)
        {
            menus.OnMenuItemSelected += MenuItemSelected;
        }
        if (search != null)
        {
            search.searchChanged += SearchChanged;
        }
        if (logInspector != null)
        {
            logInspector.OnLogInspectorClosed += LogInspectorClosed;
        }
        if (log != null)
        {
            log.OnConsoleLogSelected += ConsoleLogSelected;
        }
    }

    private void OnDestroy()
    {
        if (toolbar != null)
        {
            toolbar.OnToolbarChanged -= ToolbarButtonSelected;
        }
        if (menus != null)
        {
            menus.OnMenuItemSelected -= MenuItemSelected;
        }
        if (search != null)
        {
            search.searchChanged -= SearchChanged;
        }
        if (logInspector != null)
        {
            logInspector.OnLogInspectorClosed -= LogInspectorClosed;
        }
        if (log != null)
        {
            log.OnConsoleLogSelected -= ConsoleLogSelected;
        }
    }

    public void StateChanged(JBConsoleState jbConsoleState)
    {
        if (toolbar != null)
        {
            toolbar.SetState(jbConsoleState);
        }
        if (menus != null)
        {
            menus.SetState(jbConsoleState);            
        }
        if (search != null)
        {
            search.SetState(jbConsoleState);
        }
    }
    
    public void SetActive(bool shouldEnable, JBConsoleState jbConsoleState)
    {
        if (visibleRoot != null)
        {
            visibleRoot.SetActive(shouldEnable);
        }
        StateChanged(jbConsoleState);
		if (log != null) 
		{
			log.SetActive (shouldEnable);
		}
    }

    public void RefreshLog(ConsoleLevel consoleLevel, string searchTerm, string[] visibleChannels)
    {
        if (log != null)
        {
            log.RefreshLog(consoleLevel, searchTerm, visibleChannels);
        }
    }
    
    public void AddToolbarButtonListener(ExternalUIToolbarButtonPressed listener)
    {
        OnToolbarButton += listener;
    }

    public void RemoveToolbarButtonListener(ExternalUIToolbarButtonPressed listener)
    {
        OnToolbarButton -= listener;        
    }

    public void AddMenuButtonListener(ExternalUIMenuButtonPressed listener)
    {
        OnMenuButton += listener;
    }

    public void RemoveMenuButtonListener(ExternalUIMenuButtonPressed listener)
    {
        OnMenuButton -= listener;
    }

    public void AddSearchTermChangedListener(ExternalUISearchTermChanged listener)
    {
        OnSearchTermChanged += listener;
    }

    public void RemoveSearchTermChangedListener(ExternalUISearchTermChanged listener)
    {
        OnSearchTermChanged -= listener;
    }
    
    public void AddConsoleLogSelectedListener(ExternalUIConsoleLogSelected listener)
    {
        OnConsoleLogSelected += listener;
    }

    public void RemoveConsoleLogSelectedListener(ExternalUIConsoleLogSelected listener)
    {
        OnConsoleLogSelected -= listener;
    }
    
    private void ToolbarButtonSelected(ConsoleMenu? consoleMenu)
    {
        OnToolbarButton(consoleMenu);            
    }
    
    private void MenuItemSelected(JBConsoleStateMenuItem menuItem)
    {
        OnMenuButton(menuItem);
    }
    
    private void SearchChanged(string searchTerm)
    {
        //Debug.Log("SearchChanged - " + searchTerm);
        OnSearchTermChanged(searchTerm);
    }

    public void LogSelected(ConsoleLog consoleLog)
    {
        if (logInspector != null)
        {
            logInspector.ShowForLog(consoleLog);       
        }
    }

    private void LogInspectorClosed()
    {
        OnConsoleLogSelected(null);
    }

    private void ConsoleLogSelected(ConsoleLog consoleLog)
    {
        LogSelected(consoleLog);
        OnConsoleLogSelected(consoleLog);
    }
}

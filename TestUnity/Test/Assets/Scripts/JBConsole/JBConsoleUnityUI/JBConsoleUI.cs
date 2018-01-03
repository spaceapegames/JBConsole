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
    
    private ExternalUIToolbarButtonPressed OnToolbarButton = delegate { };
    private ExternalUIMenuButtonPressed OnMenuButton = delegate { };
    private ExternalUISearchTermChanged OnSearchTermChanged = delegate { };
    
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
        Debug.Log("SearchChanged - " + searchTerm);
        OnSearchTermChanged(searchTerm);
    }
}

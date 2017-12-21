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
    [SerializeField] private JBConsoleUIMenus menus = null;
    [SerializeField] private GameObject visibleRoot = null;

    private ExternalUIToolbarChanged OnToolbarChanged = delegate { };

    private void Awake()
    {
        if (toolbar != null)
        {
            toolbar.OnToolbarChanged += ToolbarButtonSelected;
        }
    }

    private void OnDestroy()
    {
        if (toolbar != null)
        {
            toolbar.OnToolbarChanged -= ToolbarButtonSelected;
        }
    }

    public void StateChanged(JBConsoleState jbConsoleState)
    {
        if (toolbar != null)
        {
            toolbar.SetState(jbConsoleState);
        }        
    }
    
    public void SetActive(bool shouldEnable, JBConsoleState jbConsoleState)
    {
        if (visibleRoot != null)
        {
            visibleRoot.SetActive(shouldEnable);
        }
        if (toolbar != null)
        {
            toolbar.SetState(jbConsoleState);
        }

        if (shouldEnable)
        {
            menus.SetState(jbConsoleState);
        }
    }

    public void AddToolbarChangedListener(ExternalUIToolbarChanged listener)
    {
        OnToolbarChanged += listener;
    }

    public void RemoveToolbarChangedListener(ExternalUIToolbarChanged listener)
    {
        OnToolbarChanged -= listener;        
    }
    
    private void ToolbarButtonSelected(ConsoleMenu? consoleMenu)
    {
        OnToolbarChanged(consoleMenu);            
    }
    
}

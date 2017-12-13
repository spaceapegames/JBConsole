using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUI : MonoBehaviour
{
    [SerializeField] private JBConsoleUIToolbar toolbar = null;

    public Action<ConsoleMenu?> OnToolbarChanged = delegate { };

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

    public void Enable(bool shouldEnable, JBConsoleState jbConsoleState)
    {
        if (toolbar != null)
        {
            toolbar.Enable(shouldEnable, jbConsoleState);
        }
    }
    
    private void ToolbarButtonSelected(ConsoleMenu? consoleMenu)
    {
        OnToolbarChanged(consoleMenu);            
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUIToolbarToggleButton : JBConsoleUIToolbarChanger
{
    [SerializeField] private Toggle toggle;
    
    protected override void Awake()
    {
        base.Awake();
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(ToggleChanged);
        }        
    }
    
    private void ToggleChanged(bool toggled)
    {
        ConsoleMenu? nullableMenuType = null;
        if (toggled)
        {
            nullableMenuType = menuType;
        }
        OnToolbarButton(nullableMenuType);
    }
   
    public override bool IsActive
    {
        get
        {
            if (toggle != null)
            {
                return toggle.isOn;            
            }
            return false;            
        }
    }
    
    public override Toggle Toggle
    {
        get
        {
            return toggle;            
        }
    }

}

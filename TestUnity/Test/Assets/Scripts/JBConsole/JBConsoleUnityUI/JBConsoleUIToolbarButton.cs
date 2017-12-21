using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUIToolbarButton : JBConsoleUIToolbarChanger
{
    [SerializeField] protected Button button;
    
    protected override void Awake()
    {
        base.Awake();
        if (button != null)
        {
            button.onClick.AddListener(ButtonClicked);
        }
    }
    
    private void ButtonClicked()
    {
        OnToolbarButton(menuType);
    }

}

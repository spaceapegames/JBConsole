using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUIMenuButtonColor : JBConsoleUIMenuButton
{
    [SerializeField] private Image imageToColor = null;
    [SerializeField] private Color activeColor = Color.yellow;
    [SerializeField] private Color inactiveColor = Color.white;

    protected override void Awake()
    {
        base.Awake();
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (imageToColor != null)
        {
            imageToColor.color = toggleValue ? activeColor : inactiveColor;
        }
        if (button != null)
        {
            var colors = button.colors;
            colors.normalColor = toggleValue ? activeColor : inactiveColor;
            button.colors = colors;
        }
    }
    
    protected override void IsActiveChanged()
    {
        base.IsActiveChanged();
        UpdateColor();
    }

}

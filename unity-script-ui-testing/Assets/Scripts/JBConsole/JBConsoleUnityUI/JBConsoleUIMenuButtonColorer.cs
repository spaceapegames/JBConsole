using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUIMenuButtonColorer : MonoBehaviour
{
    [SerializeField] private Image[] imagesToColor = null;
    [SerializeField] private Color activeColor = Color.yellow;
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private JBConsoleUIMenuButton menuButton = null;
    
    protected void Awake()
    {
        if (menuButton == null)
        {
            menuButton = gameObject.GetComponent<JBConsoleUIMenuButton>();
        }

        if (menuButton != null)
        {
            menuButton.OnRefreshVisuals += UpdateColor;
            UpdateColor(menuButton);
        }

    }

    private void UpdateColor(JBConsoleUIMenuButton menuButton)
    {
        if (menuButton == null)
        {
            return;
        }
        
        if (imagesToColor != null)
        {
            for (var i = 0; i < imagesToColor.Length; i++)
            {
                if (imagesToColor[i] != null)
                {
                    imagesToColor[i].color = menuButton.ToggleValue ? activeColor : inactiveColor;
                }                
            }
        }
        
        if (menuButton.Button != null)
        {
            var colors = menuButton.Button.colors;
            colors.normalColor = menuButton.ToggleValue ? activeColor : inactiveColor;
            menuButton.Button.colors = colors;
        }
    }
    
}

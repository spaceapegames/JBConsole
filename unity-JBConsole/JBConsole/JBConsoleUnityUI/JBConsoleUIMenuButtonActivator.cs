using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUIMenuButtonActivator : MonoBehaviour
{
    [SerializeField] private GameObject[] turnOn = null;
    [SerializeField] private GameObject[] turnOff = null;
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
        
        if (turnOn != null)
        {
            for (var i = 0; i < turnOn.Length; i++)
            {
                if (turnOn[i] != null)
                {
                    turnOn[i].SetActive(menuButton.ToggleValue);
                }                
            }
        }
        if (turnOff != null)
        {
            for (var i = 0; i < turnOff.Length; i++)
            {
                if (turnOff[i] != null)
                {
                    turnOff[i].SetActive(!menuButton.ToggleValue);
                }                
            }
        }        

    }
    
}

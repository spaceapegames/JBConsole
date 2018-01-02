using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JBConsoleUIMenu : MonoBehaviour
{
    [SerializeField] private JBConsoleUIMenuButton buttonPrefab = null;

    private List<JBConsoleUIMenuButton> loadedButtons = new List<JBConsoleUIMenuButton>();
    
    public Action<JBConsoleStateMenuItem> OnMenuItemSelected = delegate { };

    private void Awake()
    {
        buttonPrefab.SetActive(false);
    }

    public void Setup(JBConsoleStateMenuItem[] menu)
    {
        buttonPrefab.SetActive(true);
        if (menu != null)
        {
            for (var i = 0; i < menu.Length; i++)
            {
                var buttonGO = Instantiate(buttonPrefab.gameObject);
                var button = buttonGO.GetComponent<JBConsoleUIMenuButton>();
                loadedButtons.Add(button);
                button.Setup(menu[i]);
                button.OnButton += ButtonPressed;
                buttonGO.transform.SetParent(buttonPrefab.transform.parent, false);
            }
        }
        buttonPrefab.SetActive(false);
    }

    public void Refresh(JBConsoleStateMenuItem[] menu)
    {
        var desiredNumButtons = menu != null ? menu.Length : 0;

        while (loadedButtons.Count > desiredNumButtons)
        {
            var lastIndex = loadedButtons.Count - 1;
            Destroy(loadedButtons[lastIndex].gameObject);
            loadedButtons.RemoveAt(lastIndex);
        }

        if (menu != null)
        {
            buttonPrefab.SetActive(true);

            for (var i = 0; i < menu.Length; i++)
            {
                if (i < loadedButtons.Count)
                {
                    loadedButtons[i].Setup(menu[i]);
                }
                else
                {
                    var buttonGO = Instantiate(buttonPrefab.gameObject);
                    var button = buttonGO.GetComponent<JBConsoleUIMenuButton>();
                    loadedButtons.Add(button);
                    button.Setup(menu[i]);
                    button.OnButton += ButtonPressed;
                    buttonGO.transform.SetParent(buttonPrefab.transform.parent, false);                    
                }
            }        
        }
        
        buttonPrefab.SetActive(false);
    }
    
    private void ButtonPressed(JBConsoleStateMenuItem menuItem)
    {
        OnMenuItemSelected(menuItem);
    }

}
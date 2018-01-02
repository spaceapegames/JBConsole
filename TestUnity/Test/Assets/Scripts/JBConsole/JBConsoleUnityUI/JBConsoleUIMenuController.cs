using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUIMenuController : MonoBehaviour
{
    [SerializeField] private JBConsoleUIMenu smallButtonMenuPrefab = null;
    [SerializeField] private JBConsoleUIMenu largeButtonMenuPrefab = null;
    
    public Action<JBConsoleStateMenuItem> OnMenuItemSelected = delegate { };

    private ConsoleMenu? currentConsoleMenu = null;

    private JBConsoleUIMenu loadedMenu = null;

    private void Awake()
    {
        if (smallButtonMenuPrefab != null)
        {
            smallButtonMenuPrefab.gameObject.SetActive(false);
        }
        if (largeButtonMenuPrefab != null)
        {
            largeButtonMenuPrefab.gameObject.SetActive(false);
        }
    }

    public void SetState(JBConsoleState jbConsoleState)
    {
        if (jbConsoleState.CurrentConsoleMenu == currentConsoleMenu)
        {
            if (currentConsoleMenu.HasValue)
            {
                switch (currentConsoleMenu.Value)
                {
                    case ConsoleMenu.Levels:
                    case ConsoleMenu.Channels:
                    case ConsoleMenu.Menu:
                        RefreshMenu(jbConsoleState.Menu);
                        break;
                }
            }            
        }
        else
        {
            RemoveCurrentConsoleMenu();

            currentConsoleMenu = jbConsoleState.CurrentConsoleMenu;
        
            if (currentConsoleMenu.HasValue)
            {
                switch (currentConsoleMenu.Value)
                {
                    case ConsoleMenu.Levels:
                    case ConsoleMenu.Channels:
                        loadedMenu = CreateMenu(smallButtonMenuPrefab, jbConsoleState.Menu);
                        break;

                    case ConsoleMenu.Menu:
                        loadedMenu = CreateMenu(largeButtonMenuPrefab, jbConsoleState.Menu);
                        break;
                }
            }            
        }        
    }

    private void RefreshMenu(JBConsoleStateMenuItem[] menu)
    {
        if (loadedMenu != null)
        {
            loadedMenu.Refresh(menu);
        }
    }
    
    private JBConsoleUIMenu CreateMenu(JBConsoleUIMenu menuPrefab, JBConsoleStateMenuItem[] menu)
    {
        if (menuPrefab == null)
        {
            return null;
        }
        
        menuPrefab.gameObject.SetActive(true);

        var menuGO = Instantiate(menuPrefab.gameObject);
        var menuUI = menuGO.GetComponent<JBConsoleUIMenu>();
        menuUI.Setup(menu);
        menuUI.OnMenuItemSelected += MenuItemSelected;
        menuGO.transform.SetParent(menuPrefab.transform.parent, false);
        
        menuPrefab.gameObject.SetActive(false);

        return menuUI;
    }
    
    private void RemoveCurrentConsoleMenu()
    {
        if (loadedMenu != null)
        {
            Destroy(loadedMenu.gameObject);
            loadedMenu = null;
        }
    }

    private void MenuItemSelected(JBConsoleStateMenuItem menuItem)
    {
        OnMenuItemSelected(menuItem);
    }
    
}

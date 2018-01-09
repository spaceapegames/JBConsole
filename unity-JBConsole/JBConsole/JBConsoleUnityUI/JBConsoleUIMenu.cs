using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JBConsoleUIMenu : MonoBehaviour
{
    [SerializeField] private JBConsoleUIMenuButton buttonPrefab = null;
    [SerializeField] private JBConsoleUIMenuButton toggleButtonPrefab = null;
    [SerializeField] private JBConsoleUIMenuButton folderButtonPrefab = null;

    private List<JBConsoleUIMenuButton> loadedButtons = new List<JBConsoleUIMenuButton>();
    private Dictionary<JBConsoleStateMenuItem.VisualType, List<JBConsoleUIMenuButton>> cachedButtons = new Dictionary<JBConsoleStateMenuItem.VisualType, List<JBConsoleUIMenuButton>>();
    private Transform activeButtonsParent = null;
    private GameObject cachedButtonsParent;
    public Action<JBConsoleStateMenuItem> OnMenuItemSelected = delegate { };
    
    private void Awake()
    {
        activeButtonsParent = FindTransformForActiveButtons();
        TurnOffButtonPrefabs();
    }

    private Transform FindTransformForActiveButtons()
    {
        if (buttonPrefab != null)
        {
            return buttonPrefab.transform.parent;
        }
        if (toggleButtonPrefab != null)
        {
            return toggleButtonPrefab.transform.parent;
        }
        if (folderButtonPrefab != null)
        {
            return folderButtonPrefab.transform.parent;
        }
        return transform;
    }
    
    private void TurnOffButtonPrefabs()
    {
        if (buttonPrefab != null)
        {
            buttonPrefab.SetActive(false);            
        }
        if (toggleButtonPrefab != null)
        {
            toggleButtonPrefab.SetActive(false);            
        }
        if (folderButtonPrefab != null)
        {
            folderButtonPrefab.SetActive(false);            
        }
    }
    
    private JBConsoleUIMenuButton GetPrefab(JBConsoleStateMenuItem.VisualType visualType)
    {
        var prefab = buttonPrefab;
        switch (visualType)
        {
            case JBConsoleStateMenuItem.VisualType.Toggle:
            {
                if (toggleButtonPrefab != null)
                {
                    prefab = toggleButtonPrefab;
                }
            } break;
            
            case JBConsoleStateMenuItem.VisualType.Folder:
            case JBConsoleStateMenuItem.VisualType.ParentFolder:
            {
                if (folderButtonPrefab != null)
                {
                    prefab = folderButtonPrefab;
                }
            } break;
        }
        return prefab;
    }
    
    public void Setup(JBConsoleStateMenuItem[] menu)
    {
        if (cachedButtonsParent == null)
        {
            cachedButtonsParent = new GameObject("Button Cache");
            cachedButtonsParent.transform.SetParent(transform, false);
            cachedButtonsParent.SetActive(false);            
        }
        Refresh(menu);
    }

    private void CacheButton(JBConsoleUIMenuButton button)
    {
        if (button != null)
        {
            button.SetActive(false);
            button.OnButton -= ButtonPressed;
            button.transform.SetParent(cachedButtonsParent.transform);

            List<JBConsoleUIMenuButton> buttons = null;
            if (!cachedButtons.TryGetValue(button.MenuItem.Visual, out buttons))
            {
                buttons = new List<JBConsoleUIMenuButton>();
                cachedButtons.Add(button.MenuItem.Visual, buttons);
            }
            
            buttons.Add(button);
        }
    }

    private JBConsoleUIMenuButton GetOrCreateButton(JBConsoleStateMenuItem menuItem)
    {
        List<JBConsoleUIMenuButton> buttons = null;
        if (cachedButtons.TryGetValue(menuItem.Visual, out buttons))
        {
            if (buttons.Count > 0)
            {
                var button = buttons[0];
                buttons.RemoveAt(0);
                return button;
            }
        }
        
        var prefab = GetPrefab(menuItem.Visual);
        if (prefab != null)
        {
            prefab.SetActive(true);
            var buttonGO = Instantiate(prefab.gameObject);
            var button = buttonGO.GetComponent<JBConsoleUIMenuButton>();
            prefab.SetActive(false);
            return button;
        }
        return null;
    }
    
    public void Refresh(JBConsoleStateMenuItem[] menu)
    {
        for (var i = 0; i < loadedButtons.Count; i++)
        {
            CacheButton(loadedButtons[i]);
        }
        loadedButtons.Clear();
        
        if (menu != null)
        {
            for (var i = 0; i < menu.Length; i++)
            {
                var button = GetOrCreateButton(menu[i]);
                loadedButtons.Add(button);
                button.transform.SetParent(activeButtonsParent, false);
                button.SetActive(true);
                button.Setup(menu[i]);
                button.OnButton += ButtonPressed;
            }        
        }
    }
    
    private void ButtonPressed(JBConsoleStateMenuItem menuItem)
    {
        OnMenuItemSelected(menuItem);
    }

}
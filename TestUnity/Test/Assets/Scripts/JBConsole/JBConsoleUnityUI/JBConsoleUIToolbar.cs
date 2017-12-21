using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUIToolbar : MonoBehaviour
{
    [SerializeField] private JBConsoleUIToolbarChanger[] premadeToolbarButtons = null;    
    [SerializeField] private JBConsoleUIToolbarChanger toolbarButtonPrefab = null;

    public Action<ConsoleMenu?> OnToolbarChanged = delegate { };
    private Dictionary<ConsoleMenu, JBConsoleUIToolbarChanger> createdToolbarButtonsDict = new Dictionary<ConsoleMenu, JBConsoleUIToolbarChanger>();
    private List<JBConsoleUIToolbarChanger> createdToolbarButtons = new List<JBConsoleUIToolbarChanger>();

    private ConsoleMenu? currentConsoleMenu = null;
    
    private void Awake()
    {
        if (premadeToolbarButtons != null)
        {
            foreach (var premadeButton in premadeToolbarButtons)
            {
                if (premadeButton != null)
                {
                    premadeButton.OnToolbarButton += ToolbarButtonSelected;
                }
            }
        }
        
        CreateTopBarButtons();
    }

    private bool HasPremadeToolbarButton(ConsoleMenu consoleMenu)
    {
        if (premadeToolbarButtons != null)
        {
            foreach (var premadeToolbarButton in premadeToolbarButtons)
            {
                if (premadeToolbarButton != null && premadeToolbarButton.ConsoleMenuType == consoleMenu)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void CreateTopBarButton(ConsoleMenu consoleMenu, JBConsoleUIToolbarChanger prefab)
    {
        if (prefab != null)
        {
            var toolbarButtonGO = Instantiate(prefab.gameObject, prefab.transform.parent, false) as GameObject;
            toolbarButtonGO.name = consoleMenu.ToString() + "_Toolbar";
            var toolbarButton = toolbarButtonGO.GetComponent<JBConsoleUIToolbarChanger>();
            toolbarButton.Setup(consoleMenu);
            toolbarButton.OnToolbarButton += ToolbarButtonSelected;
            createdToolbarButtons.Add(toolbarButton);
            createdToolbarButtonsDict.Add(consoleMenu, toolbarButton);
        }
    }

    private void RemoveTopBarButtons()
    {
        foreach (var toolbarButton in createdToolbarButtons)
        {
            if (toolbarButton != null)
            {
                toolbarButton.OnToolbarButton -= ToolbarButtonSelected;
                createdToolbarButtonsDict.Remove(toolbarButton.ConsoleMenuType);

                Destroy(toolbarButton.gameObject);
            }
        }
        createdToolbarButtons.Clear();
    }
    
    private void CreateTopBarButtons()
    {
        RemoveTopBarButtons();
        
        if (toolbarButtonPrefab != null)
        {
            toolbarButtonPrefab.SetActive(true);

            var enums = Enum.GetValues(typeof(ConsoleMenu));
            foreach (ConsoleMenu consoleMenu in enums)
            {
                if (!HasPremadeToolbarButton(consoleMenu))
                {
                    CreateTopBarButton(consoleMenu, GetPrefabForConsoleMenu(consoleMenu));
                }
            }
            
            toolbarButtonPrefab.SetActive(false);
        }
    }

    private JBConsoleUIToolbarChanger GetPrefabForConsoleMenu(ConsoleMenu consoleMenu)
    {
        return toolbarButtonPrefab;
    }
    
    private void OnDestroy()
    {
        if (premadeToolbarButtons != null)
        {
            foreach (var premadeButton in premadeToolbarButtons)
            {
                if (premadeButton != null)
                {
                    premadeButton.OnToolbarButton -= ToolbarButtonSelected;
                }
            }
        }

        RemoveTopBarButtons();
    }

    private void SetAllTogglesOff()
    {
        createdToolbarButtons.ForEach((b) =>
        {
            b.IsActive = false;
        });
        
        premadeToolbarButtons.ForEach((b) =>
        {
            b.IsActive = false;
        });
    }
    
    private void UpdateState(JBConsoleState jbConsoleState)
    {
        if (jbConsoleState.CurrentConsoleMenu == currentConsoleMenu)
        {
            return;
        }
        
        ConsoleMenu? currentConsoleMenuToLoad = jbConsoleState.CurrentConsoleMenu;

        JBConsoleUIToolbarChanger toolbarChanger = null;

        SetAllTogglesOff();
        
        // if we should have something loaded then lets turn it on
        if (currentConsoleMenuToLoad != null && createdToolbarButtonsDict.ContainsKey(currentConsoleMenuToLoad.Value))
        {
            createdToolbarButtonsDict[currentConsoleMenuToLoad.Value].IsActive = true;
        }

    }
    
    public void SetState(JBConsoleState jbConsoleState)
    {
        UpdateState(jbConsoleState);
    }
    
    private void ToolbarButtonSelected(ConsoleMenu? consoleMenu)
    {
        OnToolbarChanged(consoleMenu);            
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUIToolbar : MonoBehaviour
{
    [SerializeField] private ToggleGroup toolbarToggleGroup = null;
    [SerializeField] private JBConsoleUIToolbarChanger[] premadeToolbarButtons = null;    
    [SerializeField] private JBConsoleUIToolbarChanger toolbarToggleButtonPrefab = null;
    [SerializeField] private JBConsoleUIToolbarChanger toolbarButtonPrefab = null;

    public Action<ConsoleMenu?> OnToolbarChanged = delegate { };
    private Dictionary<ConsoleMenu, JBConsoleUIToolbarChanger> createdToolbarButtonsDict = new Dictionary<ConsoleMenu, JBConsoleUIToolbarChanger>();
    private List<JBConsoleUIToolbarChanger> createdToolbarButtons = new List<JBConsoleUIToolbarChanger>();

    private GameObject currentConsoleMenuUI = null;
    private ConsoleMenu? currentConsoleMenu = null;
    private bool manuallyChangingToggles = false;
    
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
        if (toolbarToggleButtonPrefab != null)
        {
            var toolbarButtonGO = Instantiate(prefab.gameObject, toolbarToggleButtonPrefab.transform.parent, false) as GameObject;
            var toolbarButton = toolbarButtonGO.GetComponent<JBConsoleUIToolbarChanger>();
            toolbarButton.Setup(consoleMenu);
            toolbarButton.OnToolbarButton += ToolbarButtonSelected;
            createdToolbarButtons.Add(toolbarButton);
            createdToolbarButtonsDict.Add(consoleMenu, toolbarButton);
            if (toolbarButton.Toggle != null)
            {
                toolbarToggleGroup.RegisterToggle(toolbarButton.Toggle);
            }
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
                if (toolbarButton.Toggle != null)
                {
                    toolbarToggleGroup.UnregisterToggle(toolbarButton.Toggle);
                }

                Destroy(toolbarButton.gameObject);
            }
        }
        createdToolbarButtons.Clear();
    }
    
    private void CreateTopBarButtons()
    {
        RemoveTopBarButtons();
        
        if (toolbarToggleButtonPrefab != null)
        {
            toolbarToggleButtonPrefab.SetActive(true);
            toolbarButtonPrefab.SetActive(true);

            var enums = Enum.GetValues(typeof(ConsoleMenu));
            foreach (ConsoleMenu consoleMenu in enums)
            {
                if (!HasPremadeToolbarButton(consoleMenu))
                {
                    CreateTopBarButton(consoleMenu, GetPrefabForConsoleMenu(consoleMenu));
                }
            }
            
            toolbarToggleButtonPrefab.SetActive(false);
            toolbarButtonPrefab.SetActive(false);
        }
    }

    private JBConsoleUIToolbarChanger GetPrefabForConsoleMenu(ConsoleMenu consoleMenu)
    {
        switch (consoleMenu)
        {
            case ConsoleMenu.Hide: return toolbarButtonPrefab;
            default: return toolbarToggleButtonPrefab;
        }
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

    public void Enable(bool shouldEnable, JBConsoleState jbConsoleState)
    {
        manuallyChangingToggles = true;
        gameObject.SetActive(shouldEnable);
        
        if (shouldEnable)
        {
            ConsoleMenu? currentConsoleMenuToLoad = jbConsoleState.CurrentConsoleMenu;

            var currentActiveToggles = new List<Toggle>(toolbarToggleGroup.ActiveToggles());

            JBConsoleUIToolbarChanger toolbarChanger = null;

            // if we should have something loaded then lets try
            if (currentConsoleMenuToLoad != null && createdToolbarButtonsDict.ContainsKey(currentConsoleMenuToLoad.Value))
            {
                toolbarChanger = createdToolbarButtonsDict[currentConsoleMenuToLoad.Value];
            }

            toolbarToggleGroup.SetAllTogglesOff();

            if (toolbarChanger != null && toolbarChanger.Toggle != null)
            {
                toolbarChanger.Toggle.isOn = true;
            }

            CreateUIForConsoleMenu(currentConsoleMenuToLoad);
        }
        else
        {
            DestroyCurrentConsoleMenu();
            toolbarToggleGroup.SetAllTogglesOff();
        }
        manuallyChangingToggles = false;
    }
    
    private void DestroyCurrentConsoleMenu()
    {
        if (currentConsoleMenuUI != null)
        {
            Destroy(currentConsoleMenuUI);
        }
        currentConsoleMenu = null;
    }
    
    private void CreateUIForConsoleMenu(ConsoleMenu? consoleMenu)
    {
        Debug.Log("CreateUIForConsoleMenu "+consoleMenu);        

        if (consoleMenu == currentConsoleMenu)
        {
            return;
        }
        DestroyCurrentConsoleMenu();
        
        // do creation
        currentConsoleMenu = consoleMenu;
    }
    
    private void ToolbarButtonSelected(ConsoleMenu? consoleMenu)
    {
        if (!manuallyChangingToggles)
        {
            CreateUIForConsoleMenu(consoleMenu);
            OnToolbarChanged(consoleMenu);            
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUISearch : MonoBehaviour
{
    [SerializeField] private InputField searchInputField = null;
    
    private ConsoleMenu? currentConsoleMenu = null;
    public Action<string> searchChanged = delegate {};
    
    private void Awake()
    {
        gameObject.SetActive(false);

        if (searchInputField != null)
        {
            searchInputField.onEndEdit.AddListener(InputFieldOnEndEdit);
        }
    }
    
    public void SetState(JBConsoleState jbConsoleState)
    {
        currentConsoleMenu = jbConsoleState.CurrentConsoleMenu;
        if (searchInputField != null)
        {
            searchInputField.text = jbConsoleState.SearchTerm;
        }
        var showSearch = currentConsoleMenu.HasValue && currentConsoleMenu.Value == ConsoleMenu.Search;
        gameObject.SetActive(showSearch);   
    }

    private void InputFieldOnEndEdit(string text)
    {
        searchChanged(text);
    }
}
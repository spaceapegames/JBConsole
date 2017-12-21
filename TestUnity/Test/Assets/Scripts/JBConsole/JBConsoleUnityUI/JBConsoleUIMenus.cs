using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUIMenus : MonoBehaviour
{
    [SerializeField] private GameObject smallButtonMenuPrefab = null;

    private void Awake()
    {
        if (smallButtonMenuPrefab != null)
        {
            smallButtonMenuPrefab.SetActive(false);
        }
    }

    public void SetState(JBConsoleState jbConsoleState)
    {
        
    }

    public void Reset()
    {
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUILogInspector : MonoBehaviour
{
    [SerializeField] private Button backButton = null;
    [SerializeField] private Button copyButton = null;
    [SerializeField] private Text messageTextField = null;
    [SerializeField] private Text callstackTextField = null;
    [SerializeField] private RectTransform scrollingContent = null;

    private ConsoleLog consoleLog = null;
    private string stack = null;
    
    public Action OnLogInspectorClosed = delegate { };

    private void Awake()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackButtonPressed);
        }
        if (copyButton != null)
        {
            copyButton.onClick.AddListener(CopyButtonPressed);
        }
        
        gameObject.SetActive(false);
    }
    
    public void ShowForLog(ConsoleLog consoleLog)
    {
        this.consoleLog = consoleLog;

        if (consoleLog == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            if (scrollingContent != null)
            {
                scrollingContent.anchoredPosition = new Vector2(0, 0);
            }
            
            stack = "";
            if (this.consoleLog.stackTrace != null)
            {
                int linenum;
                foreach (StackFrame stackFrame in this.consoleLog.stackTrace.GetFrames())
                {
                    linenum = stackFrame.GetFileLineNumber();
                    var filename = Path.GetFileNameWithoutExtension(stackFrame.GetFileName());
                    stack += filename + ":\t" + stackFrame.GetMethod() + (linenum > 0 ? " @ " + linenum : "") + "\n";
                }
            }
            else
            {
                stack = "Stack trace disabled";
            }
            
            if (messageTextField != null)
            {
                messageTextField.text = consoleLog.GetMessage();
                messageTextField.color = consoleLog.GetColorForLevel();
            }

            if (callstackTextField != null)
            {
                callstackTextField.text = stack;
            }
        
            gameObject.SetActive(true);
        }
    }

    private void BackButtonPressed()
    {
        CloseInspector();
    }

    private void CopyButtonPressed()
    {
        var str = consoleLog.GetMessage() + "\n" + stack;
        GUIUtility.systemCopyBuffer = str;
    }
    
    private void CloseInspector()
    {
        OnLogInspectorClosed();
        gameObject.SetActive(false);
    }

}

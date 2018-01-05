using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUILogItem : MonoBehaviour, iPooledListItem
{
    [SerializeField] private Text textField = null;
    [SerializeField] private VerticalLayoutGroup layoutGroup = null;
    [SerializeField] private Button clickButton = null;

    private RectTransform rectTransform = null;
    private ConsoleLog consoleLog = null;
    private TextGenerator textGenerator = null;
    private TextGenerationSettings textGenerationSettings;
    private float lastGenerationWidth = -1;

    public Action<JBConsoleUILogItem> OnItemRecycled = delegate { };
    public Action<JBConsoleUILogItem> OnItemClicked = delegate { };
        
    private void Awake()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
        if (clickButton != null)
        {
            clickButton.onClick.AddListener(LogItemClicked);
        }
    }

    public ConsoleLog Log
    {
        get { return consoleLog; }
    }
    
    private void LogItemClicked()
    {
        OnItemClicked(this);
    }
    
    public float GetPreferredHeight(ConsoleLog consoleLog, float widthOfAvailableSpace)
    {
        var widthAvailableToText = widthOfAvailableSpace;
        if (layoutGroup != null)
        {
            widthAvailableToText -= (layoutGroup.padding.left + layoutGroup.padding.right);            
        }
        
        if (widthAvailableToText != lastGenerationWidth)
        {
            lastGenerationWidth = widthAvailableToText;
            textGenerationSettings = textField.GetGenerationSettings(new Vector2(widthAvailableToText, -1));
        }

        if (textGenerator == null)
        {
            textGenerator = new TextGenerator();
        }
        var heightForComponent = textGenerator.GetPreferredHeight(consoleLog.GetMessageToShowInLog(false), textGenerationSettings);

        if (layoutGroup != null)
        {
            heightForComponent += layoutGroup.padding.top + layoutGroup.padding.bottom;            
        }
        return heightForComponent;
    }

    private void LogRepeatsChanged(ConsoleLog log)
    {
        if (textField != null)
        {
            textField.text = consoleLog.GetMessageToShowInLog();
        }
    }
    
    public void Setup(ConsoleLog consoleLog, float widthOfAvailableSpace)
    {
        if (consoleLog != null)
        {
            consoleLog.OnRepeatsChanged -= LogRepeatsChanged;
        }
        
        this.consoleLog = consoleLog;
        consoleLog.OnRepeatsChanged += LogRepeatsChanged;

        if (textField != null)
        {
            textField.text = this.consoleLog.GetMessageToShowInLog();
            textField.color = this.consoleLog.GetColorForLevel();
        }
        
        rectTransform.sizeDelta = new Vector2(0, GetPreferredHeight(consoleLog, widthOfAvailableSpace));
    }

    public RectTransform GetRectTransform()
    {
        return rectTransform;
    }

    public void DiscardObject()
    {
        consoleLog.OnRepeatsChanged -= LogRepeatsChanged;

        OnItemRecycled(this);
    }
}

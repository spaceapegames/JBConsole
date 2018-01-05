using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUILogItem : MonoBehaviour, iPooledListItem
{
    [SerializeField] private Text textField = null;
    [SerializeField] private VerticalLayoutGroup layoutGroup = null;

    private RectTransform rectTransform = null;
    private ConsoleLog consoleLog = null;
    private TextGenerator textGenerator = null;
    private TextGenerationSettings textGenerationSettings;
    private float lastGenerationWidth = -1;

    public Action<JBConsoleUILogItem> OnItemRecycled = delegate { };
    
    private void Awake()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
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
        var heightForComponent = textGenerator.GetPreferredHeight(consoleLog.GetMessage(), textGenerationSettings);

        if (layoutGroup != null)
        {
            heightForComponent += layoutGroup.padding.top + layoutGroup.padding.bottom;            
        }
        return heightForComponent;
    }

    public void Setup(ConsoleLog consoleLog, float widthOfAvailableSpace)
    {
        this.consoleLog = consoleLog;
        if (textField != null)
        {
            textField.text = consoleLog.GetMessage();
        }
        
        rectTransform.sizeDelta = new Vector2(0, GetPreferredHeight(consoleLog, widthOfAvailableSpace));
    }

    public RectTransform GetRectTransform()
    {
        return rectTransform;
    }

    public void DiscardObject()
    {
        OnItemRecycled(this);
    }
}

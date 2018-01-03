using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class ExtendedScrollRect : ScrollRect
{
    /*
    private RectTransform contentRectTransform = null;

    public System.Action OnContentMoved = delegate { };
    
    protected override void SetContentAnchoredPosition(Vector2 position)
    {
        if (contentRectTransform == null)
        {
            contentRectTransform = typeof(ScrollRect).GetField("m_Content", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this) as RectTransform;
        }       
        
        var prevPosition = contentRectTransform.anchoredPosition;
        base.SetContentAnchoredPosition(position);
        if (contentRectTransform.anchoredPosition != prevPosition)
        {
            OnContentMoved();
        }
    }
    */
}

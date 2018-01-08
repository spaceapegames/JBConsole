using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExtendedScrollRect : ScrollRect
{
    private bool isDragging = false;

    public Action<bool> onScrollRectDragging = delegate { };
    
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        isDragging = true;
        onScrollRectDragging(true);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        isDragging = false;
        onScrollRectDragging(false);
    }

    public bool IsDragging
    {
        get { return isDragging; }
    }

    public void StopMoving()
    {
        velocity = Vector2.zero;
    }
}

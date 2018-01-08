using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface iPooledListItem
{
    RectTransform GetRectTransform();
    void DiscardObject();
}

public interface iPooledListProvider
{
    int GetNumListItems();
    iPooledListItem GetListItem(int index);
    float GetListItemHeight(int index);
}

public class PooledList : MonoBehaviour
{
    [SerializeField] private float overhang = 0.5f; // percentage of visible window height that is overlapped at the top and the bottom (same for each)

    private ExtendedScrollRect scrollRect = null;
    private RectTransform scrollingContent = null;
    private iPooledListProvider listProvider = null;
    private List<float> listItemPositions = null;
    private float windowMin = 0;
    private float windowMax = 0;
    private float contentHeight = 0;
    private RectTransform rectTransform = null;
    private bool sizeChanged = false;
    private bool autoScroll = false;

    public Action<Vector2> listSizeChanged = delegate { };
    public Action onAutoScrollCancelled = delegate { };

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    private void EnsureComponentsCreated()
    {        
        if (scrollRect == null)
        {
            scrollRect = gameObject.AddComponent<ExtendedScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 1;

            scrollRect.onValueChanged.AddListener(ScrollRectChanged);
            scrollRect.onScrollRectDragging += ScrollRectIsDragging;
        }
        
        if (scrollingContent == null)
        {
            // top anchored
            var scrollingContentGO = new GameObject(gameObject.name + " Scrolling Content");
            scrollingContent = scrollingContentGO.AddComponent<RectTransform>();
            scrollingContent.pivot = new Vector2(0, 1);
            scrollingContent.anchorMin = new Vector2(0, 1);
            scrollingContent.anchorMax = new Vector2(1, 1);
            scrollingContent.offsetMin = new Vector2(0, 0);
            scrollingContent.offsetMax = new Vector2(0, 0);
            scrollingContentGO.transform.SetParent(scrollRect.transform, false);
            scrollRect.content = scrollingContent;

            scrollingContentGO.AddComponent<JBConsoleUITouchable>();

            // TMP ADD IMAGE
            //var bg = scrollingContentGO.AddComponent<Image>();
            //bg.color = Color.gray;
        }

    }

    private void UpdateAutoScroll()
    {
        if (rectTransform != null && autoScroll)
        {
            var visibleHeight = rectTransform.rect.height;
            if (visibleHeight > 0 && contentHeight > visibleHeight)
            {
                var anchoredPosition = scrollingContent.anchoredPosition;
                anchoredPosition.y = contentHeight - visibleHeight;
                scrollingContent.anchoredPosition = anchoredPosition;                
                //Debug.Log("Applying autoscroll, position = "+anchoredPosition+" contentHeight = "+contentHeight+" visibleHeight = "+visibleHeight);
            }
        }        
    }
    
    private void SetScrollingContentHeight(float height)
    {
        var sizeDelta = scrollingContent.sizeDelta;
        sizeDelta.y = height;
        scrollingContent.sizeDelta = sizeDelta;
        contentHeight = height;

        UpdateAutoScroll();
    }
    
    public void Initialise(iPooledListProvider listProvider, bool autoScroll, int maxListSize = 0)
    {
        this.listProvider = listProvider;
        this.autoScroll = autoScroll;
        listItemPositions = new List<float>(maxListSize);
        EnsureComponentsCreated();

        UpdateVisibleWindow();
    }

    public void ItemRemoved(int itemIndex)
    {
        // remove last from positions and recalculate from the changed point
        listItemPositions.RemoveAt(listItemPositions.Count - 1);
        var numListItems = listProvider.GetNumListItems();

        var yPosition = listItemPositions[itemIndex];
        for (var i = itemIndex; i < numListItems; i++)
        {
            listItemPositions[i] = yPosition;
            yPosition += listProvider.GetListItemHeight(i);
        }
        
        SetScrollingContentHeight(yPosition);

        topMenuItemIndex = -1;
        DestroyAllListItems();
        RefreshVisibleListItems();        
    }
    
    public void Refresh()
    {
        listItemPositions.Clear();
        var numListItems = listProvider.GetNumListItems();
        float yPosition = 0;
        for (var i = 0; i < numListItems; i++)
        {
            listItemPositions.Add(yPosition);
            yPosition += listProvider.GetListItemHeight(i);
        }
        
        SetScrollingContentHeight(yPosition);
        
        topMenuItemIndex = -1;
        DestroyAllListItems();
        RefreshVisibleListItems();
    }

    public void ItemAddedToEnd()
    {
        var numListItems = listProvider.GetNumListItems();
        float yPosition = 0;
        if (listItemPositions.Count > 0)
        {
            var lastIndex = listItemPositions.Count - 1;
            yPosition = listItemPositions[lastIndex];            
            yPosition += listProvider.GetListItemHeight(lastIndex);
        }
        for (var i = listItemPositions.Count; i < numListItems; i++)
        {
            listItemPositions.Add(yPosition);
            yPosition += listProvider.GetListItemHeight(i);
        }
        
        SetScrollingContentHeight(yPosition);
        
        RefreshVisibleListItems();        
    }

    private int topMenuItemIndex = -1;
    private List<iPooledListItem> visibleMenuItems = new List<iPooledListItem>();

    private void DestroyAllListItems()
    {
        for (var i = 0; i < visibleMenuItems.Count; i++)
        {
            visibleMenuItems[i].DiscardObject();
        }
        visibleMenuItems.Clear();
    }

    private int FindClosestMenuItemIndexForYPosition(float yPosition)
    {
        if (listItemPositions != null && listItemPositions.Count > 0)
        {
            if (yPosition < listItemPositions[0])
            {
                return 0;
            }
            else if (yPosition > contentHeight)
            {
                return listItemPositions.Count - 1;
            }
            else
            {
                for (var i = 0; i < listItemPositions.Count - 1; i++)
                {
                    if (yPosition < listItemPositions[i + 1])
                    {
                        return i;
                    }
                }
                return listItemPositions.Count - 1;
            }
        }
        
        return -1;
    }

    private iPooledListItem CreateMenuItem(int index)
    {
        var menuItem = listProvider.GetListItem(index);
        if (menuItem != null)
        {
            var itemRectTransform = menuItem.GetRectTransform();
            itemRectTransform.SetParent(scrollingContent, false);
            var itemPosition = itemRectTransform.anchoredPosition;
            itemPosition.y = -listItemPositions[index];
            itemRectTransform.anchoredPosition = itemPosition;
        }
        return menuItem;
    }

    private float FindMenuItemEndPosition(int index)
    {
        var endY = 0f;
        if (index + 1 < listItemPositions.Count)
        {
            endY = listItemPositions[index + 1];
        }
        else
        {
            endY = contentHeight;
        }
        return endY;
    }
    
    private void RefreshVisibleListItems()
    {
        // find the location and build items if need a full refresh
        if (topMenuItemIndex == -1 || visibleMenuItems.Count == 0)
        {
            DestroyAllListItems();

            var windowHeight = windowMax - windowMin;
            
            if (listProvider.GetNumListItems() > 0 && windowHeight > 0)
            {
                topMenuItemIndex = FindClosestMenuItemIndexForYPosition(windowMin);
                var indexToLoad = topMenuItemIndex;
                var maxNumItems = listProvider.GetNumListItems();

                if (topMenuItemIndex >= 0)
                {
                    var yPosition = listItemPositions[indexToLoad];
                    while (yPosition < windowMax)
                    {
                        var menuItem = CreateMenuItem(indexToLoad);
                        if (menuItem != null)
                        {
                            visibleMenuItems.Add(menuItem);
                        }
                        
                        indexToLoad++;
                        if (indexToLoad >= maxNumItems)
                        {
                            break;
                        }

                        yPosition = listItemPositions[indexToLoad];
                    }
                }
            }
        }
        // otherwise adjust and load any needed
        else
        {
            // add any menuItems to beginning
            var startIndexToTry = topMenuItemIndex - 1;
            var maxNumItems = listProvider.GetNumListItems();

            while (startIndexToTry >= 0)
            {
                var startY = listItemPositions[startIndexToTry];
                var endY = FindMenuItemEndPosition(startIndexToTry);

                if (endY > windowMin  && startY < windowMax)
                //if (startY > windowMin || (startY < windowMin && endY > windowMax))
                {
                    var menuItem = CreateMenuItem(startIndexToTry);
                    if (menuItem != null)
                    {
                        visibleMenuItems.Insert(0, menuItem);
                    }
                    topMenuItemIndex = startIndexToTry;
                }
                else
                {
                    break;
                }
                
                startIndexToTry--;
            }
            
            // remove any menuitems from beginning
            startIndexToTry = topMenuItemIndex;
            var maxIndexToTry = startIndexToTry + visibleMenuItems.Count;
            for (var i = startIndexToTry; i < maxIndexToTry; i++)
            {
                var startY = listItemPositions[i];
                var endY = FindMenuItemEndPosition(i);
                if (endY < windowMin || startY > windowMax)
                {
                    var itemToRemove = visibleMenuItems[0];
                    itemToRemove.DiscardObject();
                    visibleMenuItems.RemoveAt(0);

                    if (visibleMenuItems.Count > 0)
                    {
                        topMenuItemIndex++;
                    }
                    else
                    {
                        topMenuItemIndex = -1;
                    }
                }
                else
                {
                    break;
                }                
            }
            
            // add any menuItems to end
            var endIndexToTry = topMenuItemIndex + visibleMenuItems.Count;

            while (endIndexToTry < maxNumItems)
            {
                var startY = listItemPositions[endIndexToTry];
                var endY = FindMenuItemEndPosition(endIndexToTry);

                if (startY < windowMax && endY > windowMin)
                {
                    var menuItem = CreateMenuItem(endIndexToTry);
                    if (menuItem != null)
                    {
                        visibleMenuItems.Add(menuItem);
                    }
                }
                else
                {
                    break;
                }
                endIndexToTry++;
            }
            
            // remove any menuitems from end
            endIndexToTry = topMenuItemIndex + visibleMenuItems.Count - 1;
            for (var i = endIndexToTry; i >= topMenuItemIndex; i--)
            {
                var startY = listItemPositions[i];
                var endY = FindMenuItemEndPosition(i);
                if (endY < windowMin || startY > windowMax)
                {
                    var itemToRemove = visibleMenuItems[visibleMenuItems.Count - 1];
                    itemToRemove.DiscardObject();
                    visibleMenuItems.RemoveAt(visibleMenuItems.Count - 1);

                    if (visibleMenuItems.Count <= 0)
                    {
                        topMenuItemIndex = -1;
                    }
                }
                else
                {
                    break;
                }
                
            }

        }
    }
    
    private void OnRectTransformDimensionsChange()
    {
        sizeChanged = true;
    }

    private void LateUpdate()
    {
        if (sizeChanged)
        {
            sizeChanged = false;
            UpdateVisibleWindow();
            UpdateAutoScroll();
        }
    }

    private void UpdateVisibleWindow()
    {
        if (scrollingContent == null || rectTransform == null)
        {
            return;
        }

        var visibleHeight = rectTransform.rect.height;
        
        windowMin = scrollingContent.anchoredPosition.y - (visibleHeight * overhang);
        windowMax = windowMin + visibleHeight + visibleHeight;

        if (rectTransform != null)
        {
            listSizeChanged(rectTransform.rect.size);            
        }
    }

    public void RefreshItems()
    {
        RefreshVisibleListItems();

    }
    
    private void ScrollRectChanged(Vector2 scroll)
    {
        UpdateVisibleWindow();
        RefreshItems();
    }

    private void ScrollRectIsDragging(bool isDragging)
    {
        var visibleHeight = rectTransform.rect.height;
        var scrollingContentHeight = scrollingContent.rect.height;
        var needsScrolling = visibleHeight > 0 && scrollingContentHeight > 0 && scrollingContentHeight > visibleHeight;
        
        if (needsScrolling && isDragging && autoScroll)
        {
            autoScroll = false;
            onAutoScrollCancelled();
        }            
    }

    public void ScrollToTop()
    {
        var anchoredPosition = scrollingContent.anchoredPosition;
        anchoredPosition.y = 0;
        scrollingContent.anchoredPosition = anchoredPosition;
        scrollRect.StopMoving();
    }

    public void EnableAutoScrolling()
    {
        autoScroll = true;
        UpdateAutoScroll();
        scrollRect.StopMoving();
    }
    
}

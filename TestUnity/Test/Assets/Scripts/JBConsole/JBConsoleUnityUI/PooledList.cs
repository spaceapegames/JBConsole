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

    public Action<Vector2> listSizeChanged = delegate { };

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
            
            // TMP ADD IMAGE
            var bg = scrollingContentGO.AddComponent<Image>();
            bg.color = Color.gray;
        }

    }

    private void SetScrollingContentHeight(float height)
    {
        var sizeDelta = scrollingContent.sizeDelta;
        sizeDelta.y = height;
        scrollingContent.sizeDelta = sizeDelta;
        contentHeight = height;
    }
    
    public void Initialise(iPooledListProvider listProvider, int maxListSize = 0)
    {
        this.listProvider = listProvider;
        listItemPositions = new List<float>(maxListSize);
        EnsureComponentsCreated();

        UpdateVisibleWindow();
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
            itemPosition.y = (contentHeight * 0.5f);
            var yPosition = listItemPositions[index];
            itemPosition.y -= yPosition;
            itemPosition.y -= (itemRectTransform.rect.height * 0.5f);
            itemRectTransform.anchoredPosition = itemPosition;
        }
        return menuItem;
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
                var endY = 0f;
                if (startIndexToTry + 1 < listItemPositions.Count)
                {
                    endY = listItemPositions[startIndexToTry + 1];
                }
                else
                {
                    endY = contentHeight;
                }
                if (startY > windowMin || (startY < windowMin && endY > windowMax))
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
            
            // add any menuItems to end
            var endIndexToTry = topMenuItemIndex + visibleMenuItems.Count;

            while (endIndexToTry < maxNumItems)
            {
                var startY = listItemPositions[endIndexToTry];
                if (startY < windowMax)
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

        }
    }

    private bool sizeChanged = false;
    
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

        //Debug.Log("UpdateVisibleWindow "+" windowMin="+windowMin+" windowMax="+windowMax + " anchoredPosition = "+scrollingContent.anchoredPosition+ " size = "+scrollingContent.sizeDelta);
    }
    
    private void ScrollRectChanged(Vector2 scroll)
    {
        UpdateVisibleWindow();
        RefreshVisibleListItems();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            RefreshVisibleListItems();
        }
    }
}

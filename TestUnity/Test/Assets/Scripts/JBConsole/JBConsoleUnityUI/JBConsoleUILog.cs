using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUILog : MonoBehaviour, iPooledListProvider
{
    private class ConsoleLogDetails
    {
        public ConsoleLog log;
        public float? height;

        public void ClearHeight()
        {
            height = null;
        }

        public void SetHeight(float height)
        {
            this.height = height;
        }
    }
    
    [SerializeField] private PooledList logUI = null;
    [SerializeField] private JBConsoleUILogItem logItemPrefab = null;

    private List<ConsoleLogDetails> logs;
    private Vector2 logUISize = Vector2.zero;
    private List<JBConsoleUILogItem> itemPool = new List<JBConsoleUILogItem>();
    private GameObject itemPoolParent = null;
    private ConsoleLevel consoleLevel = ConsoleLevel.Debug;
    private string searchTermLowercase = null;
    private HashSet<string> visibleChannels = new HashSet<string>();

    private void Awake()
    {
        var logger = JBLogger.instance;
        logger.OnLogRemoved += LogRemoved;
        logger.OnLogAdded += LogAdded;
        logger.OnLogsCleared += LogsCleared;
        
        logs = new List<ConsoleLogDetails>(logger.maxLogs);
        
        if (logUI != null)
        {
            logUI.listSizeChanged += LogUISizeChanged;
            logUI.Initialise(this);
        }

        if (logItemPrefab != null)
        {
            logItemPrefab.gameObject.SetActive(false);
        }

        GenerateItemPool();
        
        PopulateList();
    }

    private void OnDestroy()
    {
        var logger = JBLogger.instance;
        logger.OnLogRemoved -= LogRemoved;
        logger.OnLogAdded -= LogAdded;
        logger.OnLogsCleared -= LogsCleared;
    }

    private void GenerateItemForPool()
    {
        var instantiatedItem = Instantiate(logItemPrefab);
        instantiatedItem.OnItemRecycled += LogItemRecycled;
        instantiatedItem.gameObject.SetActive(false);
        instantiatedItem.transform.SetParent(itemPoolParent.transform, false);
        itemPool.Add(instantiatedItem);        
    }
    
    private void GenerateItemPool()
    {
        itemPoolParent = new GameObject("ItemPool");
        itemPoolParent.AddComponent<RectTransform>();
        itemPoolParent.transform.SetParent(transform, false);
        logItemPrefab.gameObject.SetActive(true);
        for (var i = 0; i < 5; i++)
        {
            GenerateItemForPool();
        }
        logItemPrefab.gameObject.SetActive(false);
    }

    private bool ShouldShow(ConsoleLog log)
    {
        return log.level >= consoleLevel 
            && (visibleChannels == null || visibleChannels.Count == 0 || visibleChannels.Contains(log.channel)) 
            && (string.IsNullOrEmpty(searchTermLowercase) || log.GetMessageLowercase().Contains(searchTermLowercase.ToLower()));
    }
    
    private bool VisibleChannelsChanged(string[] newVisibleChannels)
    {
        bool newVisibleChannelsEmpty = newVisibleChannels == null || newVisibleChannels.Length == 0;
        bool currentVIsibleChannelsEmpty = visibleChannels == null || visibleChannels.Count == 0;
        
        // if both empty then same
        if (newVisibleChannelsEmpty && currentVIsibleChannelsEmpty)
        {
            return false;
        }
        // if one empty then changed
        else if (newVisibleChannelsEmpty ^ currentVIsibleChannelsEmpty)
        {
            return true;
        }
        // both not empty
        else
        {
            if (visibleChannels.Count != newVisibleChannels.Length)
            {
                return true;
            }
            else
            {
                for (var i = 0; i < newVisibleChannels.Length; i++)
                {
                    if (!visibleChannels.Contains(newVisibleChannels[i]))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
    
    public void RefreshLog(ConsoleLevel consoleLevel, string searchTerm, string[] visibleChannels)
    {
        bool changed = false;
        var searchTermLower = searchTerm.ToLower();
        if (searchTermLowercase != searchTermLower)
        {
            changed = true;
        }
        if (this.consoleLevel != consoleLevel)
        {
            changed = true;
        }
        if (VisibleChannelsChanged(visibleChannels))
        {
            changed = true;
        }

        if (!changed)
        {
            return;
        }
        
        if (visibleChannels == null || visibleChannels.Length == 0)
        {
            this.visibleChannels.Clear();
        }
        else
        {
            this.visibleChannels.Clear();
            for (var i = 0; i < visibleChannels.Length; i++)
            {
                this.visibleChannels.Add(visibleChannels[i]);
            }
        }

        searchTermLowercase = searchTermLower;
        this.consoleLevel = consoleLevel;
        
        PopulateList();
    }
    
    private void PopulateList()
    {
        logs.Clear();
        var loggerLogs = JBLogger.instance.Logs;
        if (loggerLogs != null)
        {
            for (var i = 0; i < loggerLogs.Count; i++)
            {
                if (ShouldShow(loggerLogs[i]))
                {
                    logs.Add(new ConsoleLogDetails()
                    {
                        log = loggerLogs[i],
                        height = null
                    });                    
                }
            }            
        }
        
        RefreshLogUI();        
    }
    
    private void LogUISizeChanged(Vector2 logUISize)
    {
        if (logUISize != this.logUISize)
        {
            this.logUISize = logUISize;

            for (var i = 0; i < logs.Count; i++)
            {
                logs[i].ClearHeight();
            }
            
            RefreshLogUI();
        }
    }
    
    private void RefreshLogUI()
    {
        if (logUI != null)
        {
            logUI.Refresh();
        }        
    }
    
    private void LogRemoved(int logIndex)
    {
        logs.RemoveAt(logIndex);

        Debug.Log("LogRemoved " + logIndex);

        if (logUI != null)
        {
            logUI.ItemRemoved(logIndex);
        }
        RefreshLogUI();
    }

    private void LogsCleared()
    {
        Debug.Log("LogsCleared");

        PopulateList();
    }

    private void LogAdded(ConsoleLog consoleLog)
    {
        if (ShouldShow(consoleLog))
        {
            logs.Add(new ConsoleLogDetails()
            {
                log = consoleLog,
                height = null
            });
        }
        
        Debug.Log("LogAdded "+consoleLog.GetMessage());

        if (logUI != null)
        {
            logUI.ItemAddedToEnd();
        }
    }

    public int GetNumListItems()
    {
        return logs.Count;
    }

    public iPooledListItem GetListItem(int index)
    {
        var item = GetPooledItem();
        if (item != null)
        {
            item.Setup(logs[index].log, logUISize.x);            
        }
        return item;
    }

    public float GetListItemHeight(int index)
    {
        if (!logs[index].height.HasValue)
        {
            logs[index].SetHeight(logItemPrefab.GetPreferredHeight(logs[index].log, logUISize.x));
        }
        return logs[index].height.Value;
    }

    private JBConsoleUILogItem GetPooledItem()
    {
        JBConsoleUILogItem pooledItem = null;
        
        if (itemPool.Count <= 0)
        {
            logItemPrefab.gameObject.SetActive(true);
            GenerateItemForPool();
            logItemPrefab.gameObject.SetActive(false);
        }

        if (itemPool.Count > 0)
        {
            var lastIndex = itemPool.Count - 1;
            pooledItem = itemPool[lastIndex];
            itemPool.RemoveAt(itemPool.Count - 1);
            pooledItem.gameObject.SetActive(true);
        }

        return pooledItem;
    }
    
    private void LogItemRecycled(JBConsoleUILogItem logItem)
    {
        logItem.gameObject.SetActive(false);
        logItem.transform.SetParent(itemPoolParent.transform, false);
        itemPool.Add(logItem);
    }

}

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

//    private List<ConsoleLog> logs;
    private List<ConsoleLogDetails> logs;
    private Vector2 logUISize = Vector2.zero;
    
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
        
        PopulateList();
    }

    private void OnDestroy()
    {
        var logger = JBLogger.instance;
        logger.OnLogRemoved -= LogRemoved;
        logger.OnLogAdded -= LogAdded;
        logger.OnLogsCleared -= LogsCleared;
    }

    private void PopulateList()
    {
        logs.Clear();
        var loggerLogs = JBLogger.instance.Logs;
        if (loggerLogs != null)
        {
            for (var i = 0; i < loggerLogs.Count; i++)
            {
                logs.Add(new ConsoleLogDetails()
                {
                    log = loggerLogs[i],
                    height = null
                });
            }            
        }
        
        // calculate the sizes of the logs

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
        logs.Add(new ConsoleLogDetails()
        {
            log = consoleLog,
            height = null
        });
        
        Debug.Log("LogAdded "+consoleLog.message);

        if (logUI != null)
        {
            logUI.ItemAddedToEnd();
//            logUI.Refresh();
        }
    }

    public int GetNumListItems()
    {
        return logs.Count;
    }

    public iPooledListItem GetListItem(int index)
    {
        logItemPrefab.gameObject.SetActive(true);
        
        var instantiatedItem = Instantiate(logItemPrefab);
        instantiatedItem.OnItemRecycled += LogItemRecycled;
        instantiatedItem.Setup(logs[index].log, logUISize.x);
        
        logItemPrefab.gameObject.SetActive(false);
        return instantiatedItem;
    }

    public float GetListItemHeight(int index)
    {
        if (!logs[index].height.HasValue)
        {
            logs[index].SetHeight(logItemPrefab.GetPreferredHeight(logs[index].log, logUISize.x));
        }
        return logs[index].height.Value;
    }

    private void LogItemRecycled(JBConsoleUILogItem logItem)
    {
        Destroy(logItem.gameObject);
    }

}

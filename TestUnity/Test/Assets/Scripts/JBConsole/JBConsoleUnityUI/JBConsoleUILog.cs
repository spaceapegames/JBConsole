using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUILog : MonoBehaviour, iPooledListProvider
{
    [SerializeField] private PooledList logUI = null;
    [SerializeField] private JBConsoleUILogItem logItemPrefab = null;

    private List<ConsoleLog> logs;
    private Vector2 logUISize = Vector2.zero;

    private void Awake()
    {
        var logger = JBLogger.instance;
        logger.OnLogRemoved += LogRemoved;
        logger.OnLogAdded += LogAdded;
        logger.OnLogsCleared += LogsCleared;
        
        logs = new List<ConsoleLog>(logger.maxLogs);
        
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
        logs.AddRange(JBLogger.instance.Logs);
        
        // calculate the sizes of the logs

        RefreshLogUI();        
    }
    
    private void LogUISizeChanged(Vector2 logUISize)
    {
        if (logUISize != this.logUISize)
        {
            this.logUISize = logUISize;
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

        RefreshLogUI();
    }

    private void LogsCleared()
    {
        Debug.Log("LogsCleared");

        PopulateList();
    }

    private void LogAdded(ConsoleLog consoleLog)
    {
        logs.Add(consoleLog);
        Debug.Log("LogAdded "+consoleLog.message);
        
        RefreshLogUI();
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
        instantiatedItem.Setup(logs[index], logUISize.x);
        
        logItemPrefab.gameObject.SetActive(false);
        return instantiatedItem;
    }

    public float GetListItemHeight(int index)
    {
        return logItemPrefab.GetPreferredHeight(logs[index], logUISize.x);
    }

    private void LogItemRecycled(JBConsoleUILogItem logItem)
    {
        Destroy(logItem.gameObject);
    }

}

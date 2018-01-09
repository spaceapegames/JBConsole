using UnityEngine;
using System.Collections.Generic;
using com.spaceape.jbconsole;
using Debug = UnityEngine.Debug;

public class GameLogging : MonoBehaviour
{    
    private LogstashLogger LogstashLogger { get; set;}

    private JBPasswordEntry passwordEntry;
    
    private GameObject jbConsoleUIGO = null;

    private void Awake()
    {
        StartConsole();

        var loggers = new List<Loggable>()
        {
                new GameJBConsoleLogger(),
#if DEBUG
				new ConsoleLogger(),
#endif
        };
        if (Application.isEditor)
        {
            loggers.Add(new DebugToUnityLog());
        }

        Logger.Init(loggers.ToArray());
        
        
        Application.logMessageReceived -= HandleUnityLog;
        Application.logMessageReceived += HandleUnityLog;

    }

    public static void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        Debug.Log("HandleUnityLog - "+logString);
        var fullLogString = CreateUnityLogString(logString, stackTrace, type);

        if (type == LogType.Error || type == LogType.Exception)
        {
            var errorCode = Logger.CreateErrorHash(fullLogString);
            Logger.Error(errorCode, fullLogString);
        }
        else if (type == LogType.Warning)
        {
            Logger.Warn(fullLogString);
        }
        else
        {
            //Logger.Info(fullLogString);
        }
    }
    
    static string CreateUnityLogString(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            return logString + "\n" + stackTrace;
        }
        else
        {
            return logString;
        }
    }
    

    private void OnDestroy()
    {
        if (jbConsoleUIGO != null)
        {
            var jbConsoleExternalUI = jbConsoleUIGO.GetComponent<JBConsoleExternalUI>();
            if (JBConsole.Exists)
            {
                JBConsole.instance.RemoveExternalUI(jbConsoleExternalUI);                
            }
            Destroy(jbConsoleUIGO);
            jbConsoleUIGO = null;
        }
    }

    private void ApplyJBPrefs()
    {
        var savedDpi = PlayerPrefs.GetInt("console_dpi", 150);
        JBConsole.instance.BaseDPI = savedDpi;
    }

    public bool IsConsoleUnlocked()
    {
        return passwordEntry == null || passwordEntry.Accepted;
    }

    public void StartConsole()
    {
        if (JBConsole.instance)
        {
            ApplyJBPrefs();
            return;
        }

        JBConsole.Start(false);
        JBConsole.instance.menuItemWidth = 100;
        JBConsole.instance.Visible = false;
        ApplyJBPrefs();

        jbConsoleUIGO = Instantiate(JBConsoleConfig.GetExternalUIPrefab());
        //jbConsoleUIGO = Instantiate(Resources.Load("JBConsoleUI")) as GameObject;
        var jbConsoleExternalUI = jbConsoleUIGO.GetComponent<JBConsoleExternalUI>();
        JBConsole.instance.AddExternalUI(jbConsoleExternalUI);
        
        #if UNITY_EDITOR
        JBCToggleOnKey.RegisterToConsole();
        #endif

        JBLogger.instance.RecordStackTrace = Application.isEditor || Debug.isDebugBuild;
        
        JBConsole.instance.OnVisiblityChanged += HandleOnVisiblityChanged;
        JBCVisibleOnPress.RegisterToConsole();
        var jbcVisibleOnPress = JBConsole.instance.GetComponent<JBCVisibleOnPress>();
        if (jbcVisibleOnPress)
        {
            jbcVisibleOnPress.pressArea = new Rect(0, 0, Screen.width / 10f, Screen.height / 15f);
        }

        var emailcomp = JBCEmail.RegisterToConsole("", "Insert Bug Name Here");
        emailcomp.Inverted = true;
    }

    string PostFormatter(string body)
    {
        string header = GetDeviceHeaderInfo();

        body = header + "\n" + body;
        return body;
    }

    void HandleOnVisiblityChanged()
    {
        
    }
    
    string GetDeviceHeaderInfo()
    {
        string header = "";

        try
        {
            header += "Device Type: " + SystemInfo.deviceType + "\n";
            header += "Device Name: " + SystemInfo.deviceName + "\n";
            header += "Device OS: " + SystemInfo.operatingSystem + "\n";
            header += "*****";
        }
        catch (System.Exception)
        {
            header += "FAILED TO GET DEVICE INFO.\n";
        }

        return header;
    }

    public static bool IsIgnoredError(string message)
    {
        return message.StartsWith("Stale touch detected") || message.Contains("invalid seek position");// || message.StartsWith("Fabric:");
    }

    public static bool IsAppIntegrityError(string message)
    {
        return message.StartsWith("Inflate Error:");
    }

    public static bool IsEglError(string message)
    {
        return message.Contains("EGL_BAD_NATIVE_WINDOW") || message.Contains("[EGL]");
    }

    public static bool IsFabricError(string message)
    {
        return message.StartsWith("Fabric:");
    }

    public static bool IsWriteError(string message)
    {
        return message.StartsWith("IOException") || message.StartsWith("UnauthorizedAccessException") || message.StartsWith("Failed to open file at path");
    }
}

public class GameJBConsoleLogger : JBConsoleLogger
{
    public override void Init()
    {
        // we will start console later.
    }
}



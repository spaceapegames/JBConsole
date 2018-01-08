using System;
using UnityEngine;

public abstract class SAErrorHandling
{
	public bool RestartOnError = true; // you can turn this off in Editor mode.

	public ErrorIncident QueuedIncident {get; private set;} // PopQueued() to remove.

	public int CriticalCount;
	public int ForceQuitAfterCriticalCount; // <= 0 = never;

	protected bool unityLogThrowOnExceptionOnly;

	public virtual void ListenUnityError(bool throwOnExceptionOnly = false) // if throwOnExceptionOnly, it will not throw on LogType.Error.
	{
		Application.logMessageReceived -= HandleUnityLog;
		Application.logMessageReceived += HandleUnityLog;
	}
	
	protected virtual void HandleUnityLog(string logString, string stackTrace, LogType type)
	{
		if(type == LogType.Exception || (type == LogType.Error && !unityLogThrowOnExceptionOnly))
		{
			ThrowUnityError(logString, stackTrace, type);
		}
	}

	public virtual void ThrowUnityError(string logString, string stackTrace, LogType type)
	{
		var message = logString + "\n" + stackTrace;
		var code = Logger.CreateErrorHash (message);
		var incident = new ErrorIncident (code, null, message);
		incident.LogAsError = false; // Normally, unity's Debug.Error would have already cause a Logger.Error() call, so there is no need to log again.
		Throw (incident);
	}
	
	public void ThrowNonCritical(string localString, string buttonText = null)
	{
		var incident = new ErrorIncident(UnityErrorCodes.NotCritical, localString);
		incident.FriendlyButtonText = buttonText;
		incident.IsCritical = false;
		incident.LogAsError = false;
		Throw (incident);
	}

	public void Throw(ErrorIncident incident)
	{
		if (incident != null && QueuedIncident == null) // only one error at a time for now.
		{
			LogIncident (incident);
			HandleIncident (incident);
		}
	}

	protected virtual void LogIncident(ErrorIncident incident)
	{
		var errorLog = incident.GetLogString ();
		if (incident.LogAsError)
		{
			Logger.Error(incident.ErrorCode, "Error incident: " + errorLog);
		}
		else
		{
			Logger.Warn("Error incident: " + errorLog);
		}
	}
	
	protected virtual void HandleIncident(ErrorIncident incident)
	{
		if(incident.IsCritical)
		{
			CriticalCount++;
		}
		if(RestartOnError && !incident.CanContinue)
		{
			QueuedIncident = incident;
			RestartDueToQueuedIncident();
			
			if(ForceQuitAfterCriticalCount > 0 && CriticalCount >= ForceQuitAfterCriticalCount)
			{
				// note, only doing this after restart code , incase Application.Quit() didn't work due to platform issues.
				Logger.Info("Force quit as it has exceded max error count " + ForceQuitAfterCriticalCount);
				Application.Quit();
			}
		}
		else
		{
			ShowIncident(incident);
		}
	}
	
	public abstract void RestartDueToQueuedIncident (); // after app restart, try call PopQueued() to get incident and show that incident.
	public abstract void ShowIncident (ErrorIncident incident); // implement showing dialog 
	
	public ErrorIncident PopQueued()
	{
		var i = QueuedIncident;
		QueuedIncident = null;
		return i;
	}
}

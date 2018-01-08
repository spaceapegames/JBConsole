
public class ErrorIncident
{
	public int ErrorCode; // Error code, 0 = not error and will only be warned in logging.
	public string FriendlyMessage; // Mesage normal users see (has default)
	public string FriendlyButtonText; // Button text (has default)
	public string DebugMessage; // Message for debug (debug users and log stash)

	public bool CanContinue; // if Set, it will 'show' the incident but won't call restart.
	public bool IsCritical = true; // e.g. having no internet connection is not considered critical and should not be logged as error.
	public bool LogAsError = true; // Logger.Error() vs Logger.Info(). (e.g. you may not want to log as error again when it comes from Debug.LogError() as it would be already logged.

	public ErrorIncident (int errorCode, string friendlyMessage = null, string debugMessage = null)
	{
		ErrorCode = errorCode;
		FriendlyMessage = friendlyMessage;
		DebugMessage = debugMessage;
		IsCritical = ErrorCode != UnityErrorCodes.NotCritical;
	}

	public virtual string GetLogString()
	{
		return FriendlyMessage + " [errorCode= " + ErrorCode + ", DebugMessage= " +DebugMessage+"]";
	}

	/*
	public virtual void Show()
	{
		var text = FriendlyMessage;
		string codePrefix = "";
		if(string.IsNullOrEmpty (FriendlyMessage))
		{
			var key = "error-" + ErrorCode;
			if(Lang.HasTran(key))
			{
				text = Lang.Tran(key, "code", ErrorCode.ToString());
			}
			else if(ReleatedToAppIntegrity)
			{
				codePrefix = "A";
				var defaultLang = "Something went wrong...\nPlease contact support if you continue to have this issue.";
				text = Lang.Tran("error-dialog-integritymessage", defaultLang);
			}
			else if(ReleatedToWriteProblem)
			{
				codePrefix = "S";
				var defaultLang = "Something went wrong...\nPlease make sure you have enough free space on your device.";
				text = Lang.Tran("error-dialog-storagemessage", defaultLang);
			}
			else
			{
				text = Lang.Tran("error-dialog-message", "Something went wrong...");
			}
		}
		
		Logger.InfoCh(LogChannels.UI, "Show error incident dialog: " + text + "\n" + ErrorCode+ "\n" + DebugMessage);
		
		if(ErrorCode != 0)
		{
			text += "\n("+codePrefix + ErrorCode +")";
		}
#if !FULL_RELEASE
		text += "\n" + DebugMessage;
#endif
		var btnText = string.IsNullOrEmpty(FriendlyButtonText) ? Lang.Tran("error-dialog-button", "Continue") : FriendlyButtonText;

		var data = new BasicDialog.DialogData();
		data.PlaySounds = false;
		data.Body = text;
		data.Buttons = new Dictionary<string, Action>(){
			{btnText, delegate()
				{
					App.Instance.Restart();
				}
			}};
		// special dialog where it does not rely Config. as we might throw error due to lack of config.
		data.Prefab = Resources.Load("UI/SystemErrorDialog") as GameObject;
		BasicDialog.Show(data);
	}*/
}
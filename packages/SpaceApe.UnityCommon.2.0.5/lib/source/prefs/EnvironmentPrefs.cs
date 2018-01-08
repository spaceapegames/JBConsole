using System;
using UnityEngine;

/** Wrapper around Unity's PlayerPrefs but segmented by environment. */
public static class EnvironmentPrefs
{
	private static string _environment = null;

	public static string environment {
		get {
			if (_environment == null) {
				//Yeah this AppConfig.DEFAULT_ENVIRONMENT is defined in the project, it's part of the build process. SOZLOL - 2p
				_environment = PlayerPrefs.GetString ("EnvironmentPrefs.environment", "Dev"/*AppConfig.DEFAULT_ENVIRONMENT*/);
			}
			return _environment;
		}
		set {
			_environment = value;
			if (_environment != null) {
				PlayerPrefs.SetString ("EnvironmentPrefs.environment", _environment);
			}
		}
	}

	public static void DeleteKey (string key)
	{
		PlayerPrefs.DeleteKey (GetKey (key));
	}

	public static float GetFloat (string key, float defaultValue = 0.0f)
	{
		return PlayerPrefs.GetFloat (GetKey (key), defaultValue);
	}

	public static int GetInt (string key, int defaultValue = 0)
	{
		return PlayerPrefs.GetInt (GetKey (key), defaultValue);
	}

	public static string GetString (string key, string defaultValue = "")
	{
		return PlayerPrefs.GetString (GetKey (key), defaultValue);
	}

	public static long GetLong (string key, long defaultValue = 0L)
	{
		long result;
		string storedValue = PlayerPrefs.GetString (GetKey (key), defaultValue.ToString ());
		if (Int64.TryParse (storedValue, out result)) {
			return result;
		}
		return defaultValue;
	}

	public static bool HasKey (string key)
	{
		return PlayerPrefs.HasKey (GetKey (key));
	}

	public static void SetFloat (string key, float value)
	{
		PlayerPrefs.SetFloat (GetKey (key), value);
	}

	public static void SetInt (string key, int value)
	{
		PlayerPrefs.SetInt (GetKey (key), value);
	}

	public static void SetString (string key, string value)
	{
		PlayerPrefs.SetString (GetKey (key), value);
	}

	public static void SetLong (string key, long value)
	{
		PlayerPrefs.SetString (GetKey (key), value.ToString ());
	}

	public static void Save ()
	{
		PlayerPrefs.Save ();
	}

	private static string GetKey (string key)
	{
        return "env." + environment + "." + key;
	}
}


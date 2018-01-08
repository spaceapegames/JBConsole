using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class UnityStringUtils
{
	public static Vector2 GetFloatRangeFromString(string rangeString) // "12.34-56.78" => new Vector2(12.34f, 56.78f)
	{
		if(rangeString != null)
		{
			var match = Regex.Match(rangeString, @"(\d*\.?\d*)\s*\-\s*(\d*\.?\d*)"); // basic float range matching
			if(match != null && match.Groups.Count == 3)
			{
				var num1 = match.Groups[1].ToString();
				var num2 = match.Groups[2].ToString();
				if(num1 != "" && num1 != "." && num2 != "" && num2 != ".")
				{
					return new Vector2(float.Parse(num1), float.Parse(num2));
				}
			}
		}
		return Vector2.zero;
	}
	
	public static Color GetColorFromHexString(string colourStr)
	{
		if (colourStr == null) return Color.clear;
		colourStr = colourStr.Trim();
		if (colourStr.Length == 0) return Color.clear;

		var hasAlpha = colourStr.Length > 6;
		uint colInt = 0;
		if (!uint.TryParse(colourStr, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out colInt))
		{
			colInt = 0xffff00ff;
		}
		
		return new Color(
			((colInt >> 16)&255)/255.0f,
			((colInt >> 8)&255)/255.0f,
			((colInt >> 0)&255)/255.0f,
			hasAlpha ? ((colInt >> 24)&255)/255.0f : 1.0f
			);
	}

	public static Color GetColor32FromHexString(string colourStr)
	{
		if (colourStr == null) return Color.clear;
		colourStr = colourStr.Trim();
		if (colourStr.Length == 0) return Color.clear;

		var hasAlpha = colourStr.Length > 6;

		Color32 col = new Color32();
		uint colInt;
		if (uint.TryParse(colourStr, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out colInt))
		{
			col = new Color32(unchecked((byte)(colInt>>16)), unchecked((byte)(colInt>>8)),unchecked((byte)(colInt>>0)),hasAlpha ? unchecked((byte)(colInt>>24)) : (byte)255);
		}
		return col;
	}

	
	public static string GetHexFromColor(Color color)
	{
		return  ((int)(color.r * 255)).ToString("X2") + ((int)(color.g * 255)).ToString("X2") + ((int)(color.b * 255)).ToString("X2");
	}
}
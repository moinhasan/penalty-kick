using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScreenFactors
{
	// Considering 16:9 screen size ratios
	private const int _baseScreenWidth = 540;
	private const int _baseScreenHeight = 960;

	private static int _screenWidth;
	private static int _screenHeight;

	public static float ConvertBaseToActual(float value)
    {
		_screenHeight = Screen.height;
		_screenWidth = Screen.width;

		float actualValue = (value * _screenHeight) / _baseScreenHeight; // 1/16 of the height

		Debug.Log("Screen height = " + _screenHeight
			+ "\t Screen width = " + _screenWidth
			+ "\t Actual Value = " + actualValue);

		return actualValue;
	}

	public static float ConvertActualToBase(float value)
	{
		_screenHeight = Screen.height;
		_screenWidth = Screen.width;

		float baseValue = (value * _screenHeight) / _baseScreenHeight; // 1/16 of the height

		Debug.Log("Screen height = " + _screenHeight
			+ "\t Screen width = " + _screenWidth
			+ "\t Base Value = " + baseValue);

		return baseValue;
	}
}

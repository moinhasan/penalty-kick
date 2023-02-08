using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScreenFactors
{
	// Considering 16:9 screen size ratios
	private const int baseScreenWidth = 540;
	private const int baseScreenHeight = 960;

	private static int screenWidth;
	private static int screenHeight;

	public static float ConvertBaseToActual(float value)
    {
		screenHeight = Screen.height;
		screenWidth = Screen.width;

		float actualValue = (value * screenHeight) / baseScreenHeight; // 1/16 of the height

		Debug.Log("Screen height = " + screenHeight
			+ "\t Screen width = " + screenWidth
			+ "\t Actual Value = " + actualValue);

		return actualValue;
	}

	public static float ConvertActualToBase(float value)
	{
		screenHeight = Screen.height;
		screenWidth = Screen.width;

		float baseValue = (value * screenHeight) / baseScreenHeight; // 1/16 of the height

		Debug.Log("Screen height = " + screenHeight
			+ "\t Screen width = " + screenWidth
			+ "\t Base Value = " + baseValue);

		return baseValue;
	}
}

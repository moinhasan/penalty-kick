using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class GameDataManager //GameDataProvider
{
	private const string _fileName = "gameData.json";
	public static PlayerData Player { get; private set; }
	public static LevelData Level { get; private set; }

	/// <summary>
	/// Load JSON data from Streaming Assets Path and converts to serialized object 
	/// </summary>
	/// <param name="OnGameDataLoaded"></param>
	/// <returns></returns>
	static IEnumerator LoadDataFromJson(Action OnGameDataLoaded)
	{
		string filePath = Path.Combine(Application.streamingAssetsPath + "/", _fileName);
		string dataAsJson = "";

		// No file access is available on WebGL, Android uses a compressed .apk file. These platforms return a URL
		if (filePath.Contains("://") || filePath.Contains(":///"))
		{
			Debug.Log("UNITY filePath:" + System.Environment.NewLine + filePath);
			UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
			yield return www.SendWebRequest();
			dataAsJson = www.downloadHandler.text;
		}
		else if (File.Exists(filePath))
		{
			dataAsJson = File.ReadAllText(filePath);
		}
		else
		{
			Debug.LogError("Cannot find file! named " + _fileName);
		}

		if (!string.IsNullOrEmpty(dataAsJson))
		{
			GameData loadedData = JsonUtility.FromJson<GameData>(dataAsJson);
			Player = loadedData.playerData;
			Level = loadedData.levelData;
			OnGameDataLoaded?.Invoke();
		}
	}

	/// <summary>
    /// Load game data
    /// </summary>
    /// <param name="OnGameDataLoaded"></param>
	public static void LoadGameData(Action OnGameDataLoaded)
	{
		GameManager.Instance.StartCoroutine(LoadDataFromJson(OnGameDataLoaded));
	}
}

[System.Serializable]
public class GameData
{
	public LevelData levelData;
	public PlayerData playerData;
}

[System.Serializable]
public class LevelData
{
	public int goalPoint;
	public int shots;
	public int minTargets;
	public int maxTargets;
	public int minTargetPoints;
	public int maxTargetPoints;
}

[System.Serializable]
public class PlayerData
{
	public int id;
	public int level;
	public int xp;
	public Skill skill;
}

[System.Serializable]
public class Skill
{
	public float speed;
	public float accuracy;
	public float curve;
}
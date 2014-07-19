﻿using UnityEngine;
using System.Collections;

public class SaveData
{
	private string playerName;
	private int index;

	//Creates a new instance with given data
	public SaveData(string pPlayerName, int pIndex)
	{
		index = pIndex;
		playerName = pPlayerName;
	}

	//Creates a new instance from file saved at index
	public SaveData(int pIndex)
	{
		index = pIndex;
		playerName = PlayerPrefs.GetString("PlayerName" + pIndex.ToString());
	}

	public void save()
	{
		PlayerPrefs.SetString("PlayerName" + index.ToString(), playerName);
	}

	public string getPlayerName()
	{
		return playerName;
	}

	public int getIndex()
	{
		return index;
	}
}

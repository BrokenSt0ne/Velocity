﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DemoRecord : MonoBehaviour
{
	private List<DemoTick> tickList;
	private bool recording = false;
	private Demo completeDemo;
	private string playerName;
	private string levelName;
	private float startPlayTime;

	void FixedUpdate()
	{
		if(recording)
		{
            DemoTick tick = new DemoTick((decimal)(Time.time - startPlayTime), transform.position, transform.FindChild("Camera").rotation);
			tickList.Add(tick);
		}
	}

	public void startDemo(string pPlayerName)
	{
		startPlayTime = Time.time;
		tickList = new List<DemoTick>();
		playerName = pPlayerName;
		levelName = Application.loadedLevelName;
		recording = true;
	}

	public void stopDemo()
	{
		recording = false;
		completeDemo = new Demo(tickList, playerName, levelName);
	}

	public Demo getDemo()
	{
		return completeDemo;
	}
}
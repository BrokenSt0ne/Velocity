﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCollisionEventTrigger : EventTrigger
{
	void OnTriggerEnter(Collider col)
	{
		if(col.tag.Equals("Player"))
		{
			foreach(Event eComp in eventComponent)
			{
				fireEvent(eComp, new object[0]);
			}
		}
	}
}

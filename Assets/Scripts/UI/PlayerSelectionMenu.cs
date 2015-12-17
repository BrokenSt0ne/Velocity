﻿using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerSelectionMenu : MainSubMenu
{
    public EventHandler LoginFinished;

    [SerializeField]
    private InputField nameField;

    [SerializeField]
    private InputField passField;

    public void OnLoginClick()
    {
        GameInfo.info.CurrentSave = new SaveData(nameField.text);
        GameInfo.info.CurrentSave.Account.StartLogin(passField.text);
        GameInfo.info.CurrentSave.Account.OnLoginFinished += (sender, e) =>
        {
            if (!e.Error)
                LoginFinished(sender, e);
        };
    }

    public void OnOfflineClick()
    {
        GameInfo.info.CurrentSave = new SaveData(nameField.text);
        if (LoginFinished != null)
            LoginFinished(this, null);
        else
            throw new InvalidOperationException("MainMenu should always register on the LoginFinished event");
    }
}
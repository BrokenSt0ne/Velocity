﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class MainMenu : MonoBehaviour
{
    public static event System.EventHandler SettingsOpened;

    //General stuff
	public List<string> mapNames = new List<string>();
	public List<string> mapAuthors = new List<string>();
    public GameObject[] menuObjects;
    public GameObject[] settingObjects;
    public Toggle[] settingTitles;

    //References to specific things
    public GameObject gameSelectionContentPanel;
    public GameObject demoContentPanel;
    public GameObject mapPanelPrefab;
    public GameObject demoPanelPrefab;
    public GameObject editPanelPrefab;
    public InputField newPlayerNameField;
    public Text blogText;

    private int newPlayerSelectedIndex = 0;

    private MenuState currentState;
    public MenuState currentMenuState
    {
        get { return currentState; }
    }

	public enum MenuState
    {
        MainMenu,
        GameSelection,
        PlayerSelection,
        NewPlayer,
        Demos,
        Settings
    }

    public enum GameSelectionContent
    {
        PlayableMapList,
        ServerList,
        NewServerSettings,
        NewMapSettings,
        EditableMapList
    }

    void Awake()
    {
        SetMenuState(MenuState.MainMenu);
        GameInfo.info.setMenuState(GameInfo.MenuState.othermenu);
        loadLastPlayer();

        WWW www = new WWW("http://theasuro.de/Velocity/feed/");
        StartCoroutine(WaitForBlogEntry(www));
    }

    //Load the last player that was logged in, returns false if loading failed
    private bool loadLastPlayer()
    {
        if (!PlayerPrefs.HasKey("lastplayer"))
            return false;

        int index = PlayerPrefs.GetInt("lastplayer");
        loadPlayerAtIndex(index);
        return true;
    }

    private void loadPlayerAtIndex(int index)
    {
        GameInfo.info.setCurrentSave(new SaveData(index));
    }

    public void OnPlayButtonPress()
    {
        SaveData sd = GameInfo.info.getCurrentSave();
        if(sd == null || sd.getPlayerName().Equals(""))
        {
            //Create a new player if no player is selected
            SetMenuState(MenuState.PlayerSelection);
        }
        else
        {
            SetMenuState(MenuState.GameSelection);
        }
    }

    public void OnLoadButtonPress(int index)
    {
        //Check if a player exists, create new player if not
        SaveData sd = new SaveData(index);
        if (sd.getPlayerName().Equals(""))
        {
            newPlayerSelectedIndex = index;
            SetMenuState(MenuState.NewPlayer);
        }
        else
        {
            loadPlayerAtIndex(index);
            SetMenuState(MenuState.MainMenu);
        }
    }

    public void OnCreatePlayerOK()
    {
        CreateNewPlayer(newPlayerSelectedIndex, newPlayerNameField.text);
    }

    private void CreateNewPlayer(int index, string name)
    {
        SaveData sd = new SaveData(index, name);
        sd.save();
        loadPlayerAtIndex(index);
        SetMenuState(MenuState.MainMenu);
        ReplaceUiText.UpdateSaveInfo();
    }

    public void DeletePlayerAtIndex(int index)
    {
        SaveData sd = new SaveData(index);
        sd.deleteData(mapNames);
        ReplaceUiText.UpdateSaveInfo();

        //Log out from current player if we deleted that one
        if (GameInfo.info.getCurrentSave() != null && GameInfo.info.getCurrentSave().getIndex() == index)
            GameInfo.info.setCurrentSave(null);
    }

    public void SetMenuState(int stateID)
    {
        SetMenuState((MenuState)stateID);
    }

    public void SetMenuState(MenuState newState)
    {
        //Disable all menu groups
        foreach (GameObject menuObj in menuObjects)
        {
            menuObj.SetActive(false);
        }

        //Enable the selected group
        menuObjects[(int)newState].SetActive(true);

        //Do menu-specific preparations
        switch(newState)
        {
            case MenuState.GameSelection:
                SetGameSelectionContent(GameSelectionContent.PlayableMapList);
                break;
            case MenuState.Demos:
                LoadDemoPanels();
                break;
            case MenuState.Settings:
                if (SettingsOpened != null) { SettingsOpened(this, null); }
                break;
        }

        currentState = newState;
    }

    public void SetGameSelectionContent(int contentID)
    {
        SetGameSelectionContent((GameSelectionContent)contentID);
    }

    private void SetGameSelectionContent(GameSelectionContent newContent)
    {
        //Clear all children
        foreach(Transform child in gameSelectionContentPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //Create new children
        switch (newContent)
        {
            case GameSelectionContent.PlayableMapList:
                LoadPlayableMaps(); break;
            case GameSelectionContent.EditableMapList:
                LoadEditableMaps(); break;
            default:
                print("todo"); break;
        }
    }

    private void LoadPlayableMaps()
    {
        int mapCount = Mathf.Min(mapNames.Count, mapAuthors.Count);
        ((RectTransform)gameSelectionContentPanel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * mapCount + 10f);

        for (int i = 0; i < mapCount; i++)
        {
            CreateMapPanel(i, mapNames[i], mapAuthors[i]);
        }
    }

    private void LoadEditableMaps()
    {
        string[] mapFiles = Directory.GetFiles(Application.dataPath, "*.vlvl");
        print(mapFiles.Length);
        for (int i = 0; i < mapFiles.Length; i++)
        {
            mapFiles[i] = Path.GetFileNameWithoutExtension(mapFiles[i]);
        }

        ((RectTransform)gameSelectionContentPanel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * mapFiles.Length + 10f);

        for (int i = 0; i < mapFiles.Length; i++)
        {
            CreateEditPanel(i, mapFiles[i]);
        }
    }

    public void SetSettingGroup(int groupID)
    {
        SetMenuState(MenuState.Settings);

        foreach (GameObject obj in settingObjects)
        {
            obj.SetActive(false);
        }

        settingObjects[groupID].SetActive(true);
        settingTitles[groupID].isOn = true;
    }

    private GameObject CreatePanel(int slot, GameObject prefab, Transform parent)
    {
        GameObject panel = (GameObject)GameObject.Instantiate(prefab);
        RectTransform t = (RectTransform)panel.transform;
        t.SetParent(parent);
        t.offsetMin = new Vector2(5f, 0f);
        t.offsetMax = new Vector2(-5f, 0f);
        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f);
        float heightOffset = ((RectTransform)parent.transform).rect.height;
        t.localPosition = new Vector3(t.localPosition.x, -42.5f - slot * 75f + heightOffset, 0f);
        return panel;
    }

    private void CreateMapPanel(int slot, string name, string author)
    {
        Transform t = CreatePanel(slot, mapPanelPrefab, gameSelectionContentPanel.transform).transform;

        t.FindChild("Name").GetComponent<Text>().text = name;
        t.FindChild("Author").GetComponent<Text>().text = author;
        t.FindChild("Button").GetComponent<Button>().onClick.AddListener(delegate { OnPlayableMapClick(name); }); //Internet magic
    }

    private void CreateEditPanel(int slot, string fileName)
    {
        Transform t = CreatePanel(slot, editPanelPrefab, gameSelectionContentPanel.transform).transform;

        t.FindChild("Name").GetComponent<Text>().text = fileName;
        t.FindChild("Button").GetComponent<Button>().onClick.AddListener(delegate { LoadEditorWithLevel(fileName); });
    }

    private void CreateDemoPanel(int slot, string map, string time, string player)
    {
        Transform t = CreatePanel(slot, demoPanelPrefab, demoContentPanel.transform).transform;

        t.FindChild("Map").GetComponent<Text>().text = map;
        t.FindChild("Time").GetComponent<Text>().text = time;
        t.FindChild("Player").GetComponent<Text>().text = player;
    }

    private void LoadDemoPanels()
    {
        //Clear all children
        foreach (Object child in demoContentPanel.transform)
        {
            if (child.GetType().Equals(typeof(GameObject)))
                GameObject.Destroy(child);
        }

        //Create a list of all playable maps
        Demo[] allDemos = DemoInfo.GetAllDemos();
        ((RectTransform)demoContentPanel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * allDemos.Length + 10f);

        for (int i = 0; i < allDemos.Length; i++)
        {
            CreateDemoPanel(i, allDemos[i].getLevelName(), allDemos[i].getTime().ToString(), allDemos[i].getPlayerName());
        }
    }

    private void OnPlayableMapClick(string mapName)
    {
        GameInfo.info.loadLevel(mapName);
    }

    public void OnSettingTitleStatusChange(int group)
    {
        if(settingTitles[group].isOn)
        {
            SetSettingGroup(group);
        }
    }

    public void SaveSettings()
    {
        GameInfo.info.savePlayerSettings();
    }

    public void DeleteSettings()
    {
        PlayerPrefs.DeleteKey("fov");
        PlayerPrefs.DeleteKey("mouseSpeed");
        PlayerPrefs.DeleteKey("invertY");
        PlayerPrefs.DeleteKey("volume");
        PlayerPrefs.DeleteKey("aniso");
        PlayerPrefs.DeleteKey("aa");
        PlayerPrefs.DeleteKey("textureSize");
        PlayerPrefs.DeleteKey("lighting");
        PlayerPrefs.DeleteKey("vsync");

        GameInfo.info.loadPlayerSettings();
        if (SettingsOpened != null)
            SettingsOpened(this, null);
    }

    public IEnumerator WaitForBlogEntry(WWW www)
    {
        yield return www;
        XmlDocument doc = new XmlDocument();
        string fixedStr = www.text.Substring(www.text.IndexOf("<?xml"));
        doc.LoadXml(fixedStr);
        XmlNode node = doc.GetElementsByTagName("item")[0];
        string title = "";
        string content = "";
        foreach(XmlNode subNode in node)
        {
           if (subNode.Name.Equals("title"))
                title = subNode.InnerText;

            if (subNode.Name.Equals("content:encoded"))
                content = subNode.InnerText;
        }
        blogText.text = title + "\n" + stripHtml(content);
    }

    private string stripHtml(string text)
    {
        return text.Replace("<p>","").Replace("</p>", "\n");
    }

    public void LoadEditorWithLevel(string levelName)
    {
        GameInfo.info.editorLevelName = levelName;
        GameInfo.info.loadLevel("LevelEditor");
    }
}

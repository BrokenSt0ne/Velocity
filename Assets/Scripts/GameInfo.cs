﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameInfo : MonoBehaviour
{
	public static GameInfo info;
	public delegate string InfoString();
	public GUISkin skin;
	public string secretKey = "NotActuallySecret";
	
	//Gamestates
	private bool showDebug = false;
	private bool gamePaused = false;
	private bool showIntro = false;
	private bool showEscMenu = false;
	private bool showEndLevel = false;
	private MenuState menuState = MenuState.closed;
	private bool viewLocked = false;
	public bool menuLocked = false;

	//Sound
	public List<string> soundNames;
	public List<AudioClip> soundClips;

	//Save file stuff
	private SaveData currentSave;
	private bool savedLastDemo = false;
	
	//Debug window (top-left corner, toggle with f8)
	private List<string> linePrefixes = new List<string>();
	private List<InfoString> windowLines = new List<InfoString>();

	//Game settings
	public float mouseSpeed = 1f;
	public float fov = 90f;
	public bool showHelp = true;
	public float volume = 0.5f;

	//References
	private GameObject playerObj;
	private DemoRecord recorder;
	private MouseLook mouseLook;
	private Console myConsole;
	private Server myServer;
	private Client myClient;

	public enum MenuState
	{
		closed = 0,
		escmenu = 1,
		intro = 2,
		inactive = 3,
		demo = 4,
		endlevel = 5
	}
	
	void Awake()
	{
		if(GameInfo.info == null)
		{
			info = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}

		myServer = gameObject.GetComponent<Server>();
		myClient = gameObject.GetComponent<Client>();
		
		Screen.lockCursor = true;
		setMenuState(MenuState.intro);
	}

	void Start()
	{
		loadPlayerSettings();
	}

	//Don't do movement stuff here, do it in FixedUpdate()
	void Update()
	{
		if(Input.GetButtonDown("Debug"))
		{
			showDebug = !showDebug;
		}
		
		if(Input.GetButtonDown("Menu"))
		{
			toggleEscMenu();
		}
	}
	
	//Draw the HUD
	void OnGUI()
	{
		//Debug info in the top-left corner
		if(showDebug && playerObj != null)
		{
			Rect rect = new Rect(0f, 0f, 150f, 200f);

			GUILayout.BeginArea(rect, skin.box);

			for(int i = 0; i < windowLines.Count; i++)
			{
				GUILayout.Label(linePrefixes[i] + windowLines[i](), skin.label);
			}

			GUILayout.EndArea();
		}
		
		//Esc Menu buttons
		if(showEscMenu)
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 50f, Screen.height / 2f - 75f, 100f, 150f), skin.box);

			if(GUILayout.Button("Continue", skin.button)) { setMenuState(MenuState.closed); }
			if(GUILayout.Button("Main Menu", skin.button)) { loadLevel("MainMenu"); }
			if(GUILayout.Button("Help", skin.button)) { setMenuState(MenuState.intro); }
			if(GUILayout.Button("Quit", skin.button)) { Application.Quit(); }

			GUILayout.EndArea();
		}
		
		//Into text
		if(showIntro)
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 100f, Screen.height / 2f - 50f, 200f, 100f));

			string infoText = "Press ESC to toggle the menu.\nPress F8 to toggle debug info.\nPress (or hold) space to jump.\nPress E to grab.\nPress R to respawn.\nPress F1 to reset.";
			GUILayout.Box(infoText, skin.box);

			GUILayout.EndArea();
		}

		if(showEndLevel)
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 50f, Screen.height / 2f - 75f, 100f, 150f), skin.box);

			string saveDemoText;
			if(!savedLastDemo)
			{
				saveDemoText = "Save Demo";
			}
			else
			{
				saveDemoText = "Saved!";
			}

			if(GUILayout.Button("Main Menu", skin.button)) { loadLevel("MainMenu"); }
			if(GUILayout.Button("Play Demo", skin.button)) { menuLocked = false; setMenuState(MenuState.demo); playLastDemo(); }
			if(GUILayout.Button(saveDemoText, skin.button)) { saveLastDemo(); }
			if(GUILayout.Button("Restart", skin.button)) { menuLocked = false; reset(); }

			GUILayout.EndArea();
		}
	}

	//Lock cursor after loosing and gaining focus
	void OnApplicationFocus(bool focusStatus)
	{
		if(getMenuState() == MenuState.closed && focusStatus)
		{
			Screen.lockCursor = true;
		}
	}

	//Set menustate according to current level's worldinfo settings
	void OnLevelWasLoaded(int level)
	{
		removeAllWindowLines();
		loadPlayerSettings();
		menuLocked = false;
		WorldInfo wInfo = WorldInfo.info;
		if(wInfo != null)
		{
			setMenuState(wInfo.beginState);
		}
		else
		{
			setMenuState(MenuState.inactive);
		}
	}

	//Load a level, but inform other player if this is a server
	public void loadLevel(string name)
	{
		if(myServer.isRunning())
		{
			//TODO
		}
		Application.LoadLevel(name);
	}

	//Plays a sound at the player position
	public void playSound(string name)
	{
		for(int i = 0; i < soundNames.Count; i++)
		{
			if(soundNames[i] == name)
			{
				playerObj.audio.clip = soundClips[i];
				playerObj.audio.Play();
			}
		}
	}

	//Start a new multiplayer server
	public void startServer(string password)
	{
		myServer.StartServer(2, 42069, true, password);
	}

	//Connect to a multiplayer server
	public void connectToServer(string ip, int port, string password)
	{
		if(currentSave != null)
		{
			myClient.ConnectToServer(ip, port, password);
		}
		else
		{
			writeToConsole("You can only connect with a loaded save!");
		}
	}

	//Disconnect from current server
	public void disconnectFromServer()
	{
		myClient.DisconnectFromServer();
	}

	//Stop current server
	public void stopServer()
	{
		myServer.StopServer();
	}

	//Reset everything in the world to its initial state
	public void reset()
	{
		stopDemo();
		WorldInfo.info.reset();
		savedLastDemo = false;
		playerObj.GetComponent<PlayerEffects>().stopMoveToPos();
		Movement.movement.spawnPlayer(WorldInfo.info.getFirstSpawn());
		setMenuState(MenuState.closed);
		startDemo();
	}

	//Menu state manager
	public void setMenuState(MenuState state)
	{
		if(!menuLocked)
		{
			//Reset all states
			setGamePaused(true);
			showIntro = false;
			showEscMenu = false;
			showEndLevel = false;
			Screen.lockCursor = false;

			switch(state)
			{
				case MenuState.closed:
					setGamePaused(false);
					Screen.lockCursor = true;
					break;
				case MenuState.escmenu:
					showEscMenu = true;
					break;
				case MenuState.intro:
					showIntro = true;
					break;
				case MenuState.inactive:
					setGamePaused(false);
					break;
				case MenuState.demo:
					setGamePaused(false);
					setMouseView(false);
					Screen.lockCursor = true;
					break;
				case MenuState.endlevel:
					setGamePaused(false);
					setMouseView(false);
					showEndLevel = true;
					menuLocked = true;
					break;
			}

			menuState = state;
		}
	}

	private void toggleEscMenu()
	{
		if(menuState == MenuState.closed)
		{
			setMenuState(MenuState.escmenu);
		}
		else
		{
			setMenuState(MenuState.closed);
		}
	}

	public MenuState getMenuState()
	{
		return menuState;
	}
	
	//Draws some info in the debug window, add a prefix and a function that returns a string
	public void addWindowLine(string prefix, InfoString stringFunction)
	{
		linePrefixes.Add(prefix);
		windowLines.Add(stringFunction);
	}

	private void removeAllWindowLines()
	{
		linePrefixes.Clear();
		windowLines.Clear();
	}
	
	private void setGamePaused(bool value)
	{
		gamePaused = value;
	
		if(value)
		{
			setMouseView(false);
			Time.timeScale = 0f;
		}
		else
		{
			setMouseView(true);
			Time.timeScale = 1f;
		}
	}

	public void setCurrentSave(SaveData data)
	{
		currentSave = data;
	}

	public SaveData getCurrentSave()
	{
		return currentSave;
	}

	public void save()
	{
		if(currentSave != null)
		{
			currentSave.save();
		}
		else
		{
			writeToConsole("Tried to save, but there is no current save file :o");
		}
	}

	public void setConsole(Console pConsole)
	{
		myConsole = pConsole;
	}

	public void writeToConsole(string text)
	{
		myConsole.writeToConsole(text);
	}

	private void applySettings()
	{
		if(mouseLook != null)
		{
			mouseLook.sensitivityX = mouseSpeed;
			mouseLook.sensitivityY = mouseSpeed;
		}
		
		foreach(Camera cam in Camera.allCameras)
		{
			cam.fieldOfView = fov;
		}

		if(playerObj != null && playerObj.audio != null)
		{
			playerObj.audio.volume = volume;
		}
	}

	public void savePlayerSettings()
	{
		PlayerPrefs.SetFloat("fov", fov);
		PlayerPrefs.SetFloat("mouseSpeed", mouseSpeed);
		PlayerPrefs.SetFloat("volume", volume);

		applySettings();
	}

	public void loadPlayerSettings()
	{
		fov = PlayerPrefs.GetFloat("fov");
		mouseSpeed = PlayerPrefs.GetFloat("mouseSpeed");
		volume = PlayerPrefs.GetFloat("volume");

		if(fov == 0f) { fov = 60f; }
		if(mouseSpeed == 0f) { mouseSpeed = 1f; }

		applySettings();
	}
	
	public bool getGamePaused()
	{
		return gamePaused;
	}

	public void setPlayerObject(GameObject player)
	{
		playerObj = player;
		recorder = playerObj.GetComponent<DemoRecord>();
		mouseLook = playerObj.GetComponentInChildren<MouseLook>();
	}

	public GameObject getPlayerObject()
	{
		return playerObj;
	}

	public void startDemo()
	{
		recorder.startDemo(currentSave.getPlayerName());
	}

	public void stopDemo()
	{
		recorder.stopDemo();
	}

	public void playLastDemo()
	{
		recorder.playDemo(recorder.getDemo(), demoPlayEnded);
	}

	private void demoPlayEnded()
	{
		setMenuState(MenuState.endlevel);
	}

	public void saveLastDemo()
	{
		#if UNITY_STANDALONE_WIN
		recorder.getDemo().saveToFile(Application.dataPath);
		savedLastDemo = true;
		#endif
	}

	public void setMouseView(bool value)
	{
		if(!viewLocked)
		{
			if(mouseLook != null)
			{
				mouseLook.enabled = value;
			}
		}
	}

	//MouseLook is locked to given value, even if menu states change
	public void lockMouseView(bool value)
	{
		if(mouseLook != null)
		{
			mouseLook.enabled = value;
		}
		viewLocked = true;
	}

	//MouseLook can be changed by menu again
	public void unlockMouseView()
	{
		viewLocked = false;
	}

	public void sendLeaderboardEntry(string name, float time, string map)
	{
		WWWForm form = new WWWForm();
		form.AddField("PlayerName", name);
		form.AddField("MapTime", time.ToString());
		form.AddField("MapName", map);
		string hash = Md5Sum(name + time.ToString() + map + secretKey);
		form.AddField("Hash", hash);
		WWW www = new WWW("http://gmanserver.info/random/something.php", form);
		StartCoroutine(WaitForRequest(www));
	}

	private IEnumerator WaitForRequest(WWW www)
	{
		yield return www;

		// check for errors
		if(www.error == null)
		{
			Debug.Log("WWW Ok!: " + www.text);
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}
	}

	public  string Md5Sum(string strToEncrypt)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(strToEncrypt);
	
		// encrypt bytes
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);
	
		// Convert the encrypted bytes back to a string (base 16)
		string hashString = "";
	
		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}
	
		return hashString.PadLeft(32, '0');
	}
}

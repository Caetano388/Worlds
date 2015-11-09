﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class StartGuiManagerScript : MonoBehaviour {
	
	public Button LoadButton;

	public LoadFileDialogPanelScript LoadFileDialogPanelScript;
	public DialogPanelScript MainMenuDialogPanelScript;
	public ProgressDialogPanelScript ProgressDialogPanelScript;
	public TextInputDialogPanelScript MessageDialogPanelScript;
	public WorldCustomizationDialogPanelScript SetSeedDialogPanelScript;
	public WorldCustomizationDialogPanelScript CustomizeWorldDialogPanelScript;
	
	private bool _preparingWorld = false;
	
	private string _progressMessage = null;
	private float _progressValue = 0;
	
	private PostPreparationOperation _postPreparationOp = null;

	// Use this for initialization
	void Start () {

		Manager.UpdateMainThreadReference ();

		LoadFileDialogPanelScript.SetVisible (false);
		ProgressDialogPanelScript.SetVisible (false);
		SetSeedDialogPanelScript.SetVisible (false);
		MessageDialogPanelScript.SetVisible (false);
		CustomizeWorldDialogPanelScript.SetVisible (false);
		MainMenuDialogPanelScript.SetVisible (true);
		
		LoadButton.interactable = HasFilesToLoad ();
	}
	
	// Update is called once per frame
	void Update () {
		
		Manager.ExecuteTasks (100);
		
		if (_preparingWorld) {
		
			if (_progressMessage != null) ProgressDialogPanelScript.SetDialogText (_progressMessage);
			
			ProgressDialogPanelScript.SetProgress (_progressValue);
		}
		
		if (!Manager.WorldReady) {
			return;
		}
		
		if (_preparingWorld) {
			
			if (_postPreparationOp != null) 
				_postPreparationOp ();

			_preparingWorld = false;
			
			Application.LoadLevel ("WorldView");
		}
	}
	
	private bool HasFilesToLoad () {
		
		string dirPath = Manager.SavePath;
		
		string[] files = Directory.GetFiles (dirPath, "*.PLNT");
		
		return files.Length > 0;
	}
	
	public void LoadWorld () {
		
		MainMenuDialogPanelScript.SetVisible (false);
		
		LoadFileDialogPanelScript.SetVisible (true);
		
		LoadFileDialogPanelScript.SetLoadAction (LoadAction);
	}
	
	public void LoadAction () {
		
		LoadFileDialogPanelScript.SetVisible (false);
		
		ProgressDialogPanelScript.SetVisible (true);
		
		ProgressUpdate (0, "Loading World...", true);
		
		string path = LoadFileDialogPanelScript.GetPathToLoad ();
		
		Manager.LoadWorldAsync (path, ProgressUpdate);
		
		Manager.WorldName = Path.GetFileNameWithoutExtension (path);
		
		_preparingWorld = true;
	}
	
	public void CancelLoadAction () {
		
		LoadFileDialogPanelScript.SetVisible (false);
		
		MainMenuDialogPanelScript.SetVisible (true);
	}
	
	public void SetGenerationSeed () {
		
		MainMenuDialogPanelScript.SetVisible (false);
		
		int seed = Random.Range (0, int.MaxValue);
		
		SetSeedDialogPanelScript.SetSeedString (seed.ToString());
		
		SetSeedDialogPanelScript.SetVisible (true);
		
	}
	
	public void CancelGenerateAction () {
		
		SetSeedDialogPanelScript.SetVisible (false);
		CustomizeWorldDialogPanelScript.SetVisible (false);

		MainMenuDialogPanelScript.SetVisible (true);
	}
	
	public void CloseSeedErrorMessageAction () {
		
		MessageDialogPanelScript.SetVisible (false);
		
		SetGenerationSeed ();
	}
	
	public void GenerateWorldWithCustomSeed () {
		
		SetSeedDialogPanelScript.SetVisible (false);
		
		int seed = 0;
		string seedStr = SetSeedDialogPanelScript.GetSeedString ();
		
		if (!int.TryParse (seedStr, out seed)) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		if (seed < 0) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		GenerateWorldInternal (seed);
	}
	
	public void GenerateWorldWithCustomParameters () {

		CustomizeWorldDialogPanelScript.SetVisible (false);
		
		Manager.TemperatureOffset = CustomizeWorldDialogPanelScript.TemperatureOffset;
		Manager.RainfallOffset = CustomizeWorldDialogPanelScript.RainfallOffset;
		Manager.SeaLevelOffset = CustomizeWorldDialogPanelScript.SeaLevelOffset;
		
		int seed = 0;
		string seedStr = CustomizeWorldDialogPanelScript.GetSeedString ();

		if (!int.TryParse (seedStr, out seed)) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		if (seed < 0) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}

		GenerateWorldInternal (seed);
	}

	private void GenerateWorldInternal (int seed) {

		ProgressDialogPanelScript.SetVisible (true);
		
		ProgressUpdate (0, "Generating World...", true);
		
		_preparingWorld = true;
		
		Manager.GenerateNewWorldAsync (seed, ProgressUpdate);
		
		_postPreparationOp = () => {
			
			Manager.WorldName = "world_" + Manager.CurrentWorld.Seed;
			
			_postPreparationOp = null;
		};
	}
	
	public void CustomizeGeneration () {
		
		SetSeedDialogPanelScript.SetVisible (false);

		string seedStr = SetSeedDialogPanelScript.GetSeedString ();
		
		CustomizeWorldDialogPanelScript.SetVisible (true);

		CustomizeWorldDialogPanelScript.SetSeedString (seedStr);

		CustomizeWorldDialogPanelScript.SetTemperatureOffset(Manager.TemperatureOffset);
		CustomizeWorldDialogPanelScript.SetRainfallOffset(Manager.RainfallOffset);
		CustomizeWorldDialogPanelScript.SetSeaLevelOffset(Manager.SeaLevelOffset);
	}
	
	public void ProgressUpdate (float value, string message = null, bool reset = false) {
		
		if (reset || (value >= _progressValue)) {

			if (message != null) 
				_progressMessage = message;

			_progressValue = value;
		}
	}
	
	public void Exit () {
		
		Application.Quit();
	}
}
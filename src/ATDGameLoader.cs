using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Directory = System.IO.Directory;
using Thread = System.Threading.Thread;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.Cryptography;
using SystemFile = System.IO.File;
using System.Threading;

public class ATDGameLoader : Node2D {
	private const string ATDPathConfig = "application/config/atd_path";
	public Label loadInfo, loadInfoFiles;
	public FileDialog selectATDPath;
	public AcceptDialog directoryInvalidDialog;

	static bool isInEditor; //TODO:Refractor this to the game class!
	static bool otherDataLoaded;
	static bool startLoad = false, delay = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {

		loadInfo = GetNode<Label>("LoadInfo");
		loadInfoFiles = GetNode<Label>("LoadInfoFiles");

		GFXLibrary.pathToAirlineTycoonD = (string)SettingsManager.GetSetting(ATDPathConfig, "");
		GD.Print(GFXLibrary.pathToAirlineTycoonD);

		isInEditor = IsInEditor();

		//Is the current GFXLibrary.pathToAirlineTycoonD  folder, the correct folder?
		if (!IsOriginalGamePath(GFXLibrary.pathToAirlineTycoonD)) {
			SetNewGamePath();//Correct the error!
			return; //We decide later, whether the files need to be updated!
		}


		//If the current path is correct, load all files
		LoadFiles();
	}

	public static bool IsOriginalGamePath(string dir) {
		try {
			if (!Directory.Exists(dir))
				throw new System.IO.DirectoryNotFoundException(dir);

			GFXLibrary.pathToAirlineTycoonD = dir;

			ATFile.FindFolder("room");
			ATFile.FindFile("glbasis.gli");

			return true;
		} catch (Exception e) {
			GD.Print(e.Message);
			return false;
		}
	}
	private static string FindFolder(string folderName, string basePath = "") {
		basePath = basePath == "" ? GFXLibrary.pathToAirlineTycoonD : basePath;
		return Directory.GetDirectories(basePath, folderName, System.IO.SearchOption.AllDirectories).First();
	}

	private void LoadFiles() {
		LoadOtherData();
		ATDataLoader.LoadImageData();
		ATDataLoader.CreateImageResources();
	}

	private bool IsInEditor() {
		return SystemFile.Exists("default_env.tres") && SystemFile.Exists("default_bus_layout.tres"); //They should not exists outside of the editor
	}

	private void SetNewGamePath() {
		selectATDPath = GetNode<FileDialog>("FileDialog"); //Get the path to the ATD install
		selectATDPath.Connect("dir_selected", this, nameof(ChoseFile));
		selectATDPath.GetCancel().Connect("button_down", this, nameof(ExitGame));
		selectATDPath.PopupCentered(new Vector2(500, 500));

		directoryInvalidDialog = GetNode<AcceptDialog>("DirectoryInvalid");
		directoryInvalidDialog.Connect("confirmed", this, nameof(AcceptDialog));
	}

	public override void _Process(float delta) {
		loadInfo.SetText("Loading " + (((OS.GetTicksMsec() / 500) % 3 == 0) ? "." : ((OS.GetTicksMsec() / 500) % 3 == 1) ? ".." : "..."));

		if (otherDataLoaded) {
			//LoadAllFiles();
			GetTree().ChangeScene("res://scenes/base.tscn");
		}

	}

	public void ExitGame() {
		selectATDPath.GetCloseButton().
		GetTree().Quit();
	}

	public void ChoseFile(string dir) {
		if (!IsOriginalGamePath(dir)) { //Basic check to see if we are inside the ATD folder
			GD.PrintErr("INVALID PATH CHOSEN, CAN'T FIND glbasis.gli OR THE FOLDER room!");
			directoryInvalidDialog.PopupCentered();
			return;
		}
		//ValidateFiles();

		SettingsManager.SetSetting(ATDPathConfig, dir);
		GFXLibrary.pathToAirlineTycoonD = dir;
		LoadFiles();
	}

	public void AcceptDialog() {
		selectATDPath.PopupCentered(new Vector2(500, 500)); //try try and try again
	}

	public static Exception TryAction(Action action) {
		try {
			action();
		} catch (Exception e) {
			return e;
		}

		return null;
	}

	public void LoadOtherData() {
		ATDataLoader.LoadMusicData();
		ATDataLoader.LoadCSVData();
		SettingsManager.LoadSavedData();

		otherDataLoaded = true;
	}
}

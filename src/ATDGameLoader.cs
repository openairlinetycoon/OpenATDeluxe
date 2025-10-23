using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using DirAccess = System.IO.Directory;
using Thread = System.Threading.Thread;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.Cryptography;
using SystemFile = System.IO.File;
using System.Threading;
using System.Text;

public partial class ATDGameLoader : Node2D {
	private const string ATDPathConfig = "application/config/atd_path";
	public Label loadInfo, loadInfoFiles;
	public FileDialog selectATDPath;
	public AcceptDialog directoryInvalidDialog;

	static bool isInEditor; //TODO:Refractor this to the game class!
	static bool otherDataLoaded;
	static bool startLoad = false, delay = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Register the code pages provider to enable Windows-1252 encoding
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		loadInfo = GetNode<Label>("LoadInfo");
		loadInfoFiles = GetNode<Label>("LoadInfoFiles");

		GFXLibrary.pathToAirlineTycoonD = SettingsManager.GetSetting<string>(ATDPathConfig, "");
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
			GD.Print("Validating ATD path: " + dir);
			
			if (string.IsNullOrEmpty(dir)) {
				GD.PrintErr("Path is null or empty");
				return false;
			}
			
			if (!DirAccess.Exists(dir)) {
				GD.PrintErr("Directory does not exist: " + dir);
				throw new System.IO.DirectoryNotFoundException(dir);
			}

			// Temporarily set the path for validation
			string oldPath = GFXLibrary.pathToAirlineTycoonD;
			GFXLibrary.pathToAirlineTycoonD = dir;
			GD.Print("Temporarily set GFXLibrary.pathToAirlineTycoonD to: " + dir);

			try {
				GD.Print("Looking for 'room' folder...");
				string roomPath = ATFile.FindFolder("room");
				GD.Print("Found room folder at: " + roomPath);
				
				GD.Print("Looking for 'glbasis.gli' file...");
				string gliPath = ATFile.FindFile("glbasis.gli");
				GD.Print("Found glbasis.gli at: " + gliPath);
				
				GD.Print("Validation succeeded!");
				return true;
			} catch (Exception ex) {
				// Restore old path if validation fails
				GFXLibrary.pathToAirlineTycoonD = oldPath;
				GD.PrintErr("Validation failed: " + ex.Message);
				throw;
			}
		} catch (Exception e) {
			GD.PrintErr("IsOriginalGamePath exception: " + e.Message);
			return false;
		}
	}
	private static string FindFolder(string folderName, string basePath = "") {
		basePath = basePath == "" ? GFXLibrary.pathToAirlineTycoonD : basePath;
		return DirAccess.GetDirectories(basePath, folderName, System.IO.SearchOption.AllDirectories).First();
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
		selectATDPath.Connect("dir_selected", new Callable(this, nameof(ChoseFile)));
		selectATDPath.GetCancelButton().Connect("button_down", new Callable(this, nameof(ExitGame)));
		selectATDPath.PopupCentered(new Vector2I(500, 500));

		directoryInvalidDialog = GetNode<AcceptDialog>("DirectoryInvalid");
		directoryInvalidDialog.Connect("confirmed", new Callable(this, nameof(AcceptDialog)));
	}

	public override void _Process(double delta) {
		loadInfo.Text = "Loading " + (((Time.GetTicksMsec() / 500) % 3 == 0) ? "." : ((Time.GetTicksMsec() / 500) % 3 == 1) ? ".." : "...");

		if (otherDataLoaded) {
			//LoadAllFiles();
			GetTree().ChangeSceneToFile("res://scenes/base.tscn");
		}

	}

	public void ExitGame() {
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
		selectATDPath.PopupCentered(new Vector2I(500, 500)); //try try and try again
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
		LocalizationManager.LoadLocalizationData();
		SettingsManager.LoadSavedData();

		otherDataLoaded = true;
	}
}

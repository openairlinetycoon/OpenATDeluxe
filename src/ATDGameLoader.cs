using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Directory = System.IO.Directory;
using Thread = System.Threading.Thread;
using System.Diagnostics;
using System.Threading.Tasks;

public class ATDGameLoader : Node2D {
	private const int FileSystemVersion = 1; //What version is the current file decrypter. For forced rebuilds on changes
	private const string PackFile = "ATDFiles.pck";
	private const string ImagesPath = "res://Images";
	private const string ATDPathConfig = "application/config/atd_path";
	public Label loadInfo;
	public FileDialog selectATDPath;
	public AcceptDialog directoryInvalidDialog;

	static Dictionary<string, List<GFXLibrary>> libraryFolders;

	[Export]
	bool forceRebuild = false;
	bool isInEditor;

	static bool otherDataLoaded;

	ResourceInteractiveLoader gameLoader;

	static volatile string otherLoading;
	static volatile string currentFolder;
	static volatile string currentLibrary;
	static volatile string currentFile;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		loadInfo = GetNode<Label>("LoadInfo");

		GFXLibrary.pathToAirlineTycoonD = (string)SettingsManager.GetSetting(ATDPathConfig, "");
		GD.Print(GFXLibrary.pathToAirlineTycoonD);

		isInEditor = OS.IsDebugBuild();

		if (isInEditor) {
			File f = new File();

			File.ModeFlags flag = File.ModeFlags.ReadWrite;

			if (!f.FileExists("res://Images/version.txt"))
				flag = File.ModeFlags.WriteRead;

			f.Open("res://Images/version.txt", (int)flag);

			int currentVersion = f.Get32();

			forceRebuild = forceRebuild ? forceRebuild : (currentVersion != FileSystemVersion);

			f.Store32(FileSystemVersion);

			f.Close();
		}

		if (forceRebuild || (!System.IO.File.Exists(PackFile) && !Directory.Exists(ProjectSettings.GlobalizePath(ImagesPath)))) {
			selectATDPath = GetNode<FileDialog>("FileDialog"); //Get the path to the ATD install
			selectATDPath.Connect("dir_selected", this, nameof(ChoseFile));
			selectATDPath.GetCancel().Connect("button_down", this, nameof(ExitGame));
			selectATDPath.PopupCentered(new Vector2(500, 500));

			directoryInvalidDialog = GetNode<AcceptDialog>("DirectoryInvalid");
			directoryInvalidDialog.Connect("confirmed", this, nameof(AcceptDialog));
		} else { //If there is no need for a rebuild, just skip asking
			if (!isInEditor) {
				ProjectSettings.LoadResourcePack(PackFile); //If we are not in the editor: we need to add the ATDFiles
			}
			LoadOtherData();

			SetProcess(true);
			gameLoader = ResourceLoader.LoadInteractive("res://scenes/base.tscn");

			//GetTree().ChangeScene("res://scenes/base.tscn");
		}


	}

	public override void _Process(float delta) {
		loadInfo.SetText("Loading: " + currentFolder + "/" + currentLibrary + "/" + currentFile);


		if (gameLoader == null) {
			//SetProcess(false);
			return;
		}

		if (GetNode<Sprite>("Title").GetTexture() == null) {
			GetNode<Sprite>("Title").SetTexture((Texture)ResourceLoader.Load("res://Images/room/titel/TITEL.res"));
		}

		int time = OS.GetTicksMsec();
		while (OS.GetTicksMsec() < time + 100) {
			Error state = gameLoader.Poll();

			if (state == Error.FileEof && AllDataLoaded()) {
				Resource newScene = gameLoader.GetResource();
				gameLoader = null;

				var root = GetTree().GetRoot();
				root.GetChild(root.GetChildCount() - 1).QueueFree();
				Node node = ((PackedScene)newScene).Instance();
				root.AddChild(node);
				return;
				//ChangeScene("res://scenes/base.tscn");

			} else if (state == Error.FileEof) {
				loadInfo.SetText("Loading " + otherLoading + "...");
			} else if (state == Error.Ok) {
				int progress = 100 / gameLoader.GetStageCount() * gameLoader.GetStage();
				loadInfo.SetText("Loading scene: " + progress + "%");
			}
		}


	}

	private bool AllDataLoaded() {
		return otherDataLoaded;
	}

	public void ExitGame() {
		selectATDPath.GetCloseButton().
		GetTree().Quit();
	}

	public void ChoseFile(string dir) {
		if (!System.IO.File.Exists(dir + "/gli/glbasis.gli")) { //Basic check to see if we are inside the ATD folder
			GD.PrintErr("INVALID PATH CHOSEN, CAN'T FIND glbasis.gli IN SUBFOLDER gli!");
			directoryInvalidDialog.PopupCentered();
			return;
		}

		SettingsManager.SetSetting(ATDPathConfig, dir);
		GFXLibrary.pathToAirlineTycoonD = dir;

		Thread t = new Thread(CreateData);
		t.Name = "DataLoader";

		t.Start();
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
		Task.Run(() =>
		{
			List<Exception> exceptions = new List<Exception>();

			exceptions.Add(TryAction(() =>
			{
				otherLoading = "Music";
				string[] midFiles = System.IO.Directory.GetFiles(GFXLibrary.pathToAirlineTycoonD + "/sound/", "*.mid");
				string[] oggFiles = System.IO.Directory.GetFiles(GFXLibrary.pathToAirlineTycoonD + "/sound/", "*.ogg");

				if (oggFiles.Length != 0) { //OGG Files are preffered, as there currently is only a buggy Midi player available
					MusicController.musicFiles = Song.CreateFromFiles(oggFiles, Song.SongTypes.Ogg);
					MusicController.isOgg = true;
				} else {
					MusicController.musicFiles = Song.CreateFromFiles(oggFiles, Song.SongTypes.Mid);
				}
			}));

			exceptions.Add(TryAction(() =>
			{
				otherLoading = "Clan CSV";
				ClanCSVFile clanFile = new ClanCSVFile(GFXLibrary.pathToAirlineTycoonD + "/data/clan.csv");
			}));

			exceptions.Add(TryAction(() =>
			{
				otherLoading = "Brick CSV";
				CSVFileDecoder decoder = new CSVFileDecoder(GFXLibrary.pathToAirlineTycoonD + "/data/brick.csv");
			}));



			exceptions.Add(TryAction(() =>
			{
				otherLoading = "Settings";
				AudioServer.SetBusVolumeDb(2, Convert.ToSingle(SettingsManager.GetSetting("audio/bus2vol")));
			}));
			foreach (Exception e in exceptions) {
				if (e != null)
					GD.Print("Error loading!\n" + e.Message + "\n" + e.StackTrace);
			}

			otherLoading = "";
			otherDataLoaded = true;


			//otherLoading = "Localization Data";
			//LocalizationManager.LoadLocalizationData();
			//This is only there to compile the localization.csv, we don't need it every run.

		});
	}

	public void CreateData() {

		// Code to decrypt the .csv files -- Not needed for now!
		// BaseFileDecoder d = new BaseFileDecoder();
		// File n = new File();
		// string[] files = System.IO.Directory.GetFiles(GFXLibrary.pathToAirlineTycoonD + "/data/");
		// foreach(string f in files){
		// 	n.Open(f, (int)File.ModeFlags.Read);
		// 	System.IO.File.WriteAllBytes(f + "s",d.ReadFile(n));
		// 	n.Close();
		// }

		LoadOtherData();

		LoadImages();
	}


	/// <summary>
	/// Loads, Saves (when neccessary) and packes all .gli files in "room" and "gli"
	/// </summary>
	private void LoadImages() {
		libraryFolders = new Dictionary<string, List<GFXLibrary>>(); //A list of graphic folders of the game
		libraryFolders.Add("room", new List<GFXLibrary>());
		libraryFolders.Add("gli", new List<GFXLibrary>());

		foreach (var folderKVP in libraryFolders) {
			string folderName = folderKVP.Key;
			List<GFXLibrary> folderGFXList = folderKVP.Value;

			currentFolder = folderName;

			//Load up all GFX files in the directory
			string[] files = System.IO.Directory.GetFiles(GFXLibrary.pathToAirlineTycoonD + "/" + folderName);
			foreach (string f in files) {
				if (f.ToLower().EndsWith(".gli")) {
					var lib = new GFXLibrary(GFXLibrary.pathToAirlineTycoonD + "/" + folderName + "/" + System.IO.Path.GetFileName(f));
					lib.GetFilesInLibrary();

					folderGFXList.Add(lib);
				}
			}
		}

		Debug.Assert(GetNode<Sprite>("Title") != null, "Invalid scene structure! Title child node missing!");
		Debug.Assert(libraryFolders["room"].Find((lib) => lib.name == "titel") != null, "Missing room gli library folder! Check your files for completion! (title.gli)");

		Debug.Assert(
			libraryFolders["room"].Find((lib) => lib.name == "titel").files.Find((f) => f.name.Trim('\0') == "TITEL") != null,
			 "Missing TITLE room file in GFXLibrary titel from room!");

		GetNode<Sprite>("Title").SetTexture(
			libraryFolders["room"]
				.Find((lib) => lib.name == "titel").files
				.Find((f) => f.name.Trim('\0') == "TITEL").GetTexture());


		//Write them to disk
		ExportImages(!isInEditor);

		//Should only be needed when we are not in the editor, but adding it doesnt hurt us, even if the file isn't present
		bool s = ProjectSettings.LoadResourcePack(PackFile);
		GetTree().ChangeScene("res://scenes/base.tscn"); //No need for the interactive loader, everything is already in memory!
	}

	/// <summary>
	/// Saves all gli files in the libraryFolders dictionary to the "ImagesPath" folder
	/// </summary>
	/// <param name="packFiles">Should the files be packed into a .pck file</param>
	private void ExportImages(bool packFiles) {
		PCKPacker p = new PCKPacker();


		if (packFiles) //Should we pack them up? Only needed when we are not inside the editor
			p.PckStart(PackFile, 4);


		string basePath = ProjectSettings.GlobalizePath(ImagesPath); //Get an absolute Path to our project/executable folder
		string baseGodotPath = ImagesPath;

		if (!Directory.Exists(basePath))
			Directory.CreateDirectory(basePath);


		foreach (var folderKVP in libraryFolders) {
			List<GFXLibrary> librarys = folderKVP.Value;
			string libraryName = folderKVP.Key;

			SaveLibrarysInList(packFiles, p, basePath + "/" + libraryName, baseGodotPath + "/" + libraryName, librarys);
		}
		if (packFiles)
			p.Flush(true);
	}

	/// <summary>
	/// Saves every file inside a List&lt;GFXLibrary&gt;
	/// </summary>
	/// <param name="packFiles">Should the files be packed into a .pck file</param>
	/// <param name="p">The PCKPacker for the packing, can be null when packFiles is false</param>
	/// <param name="basePath">An absolute path to the base folder</param>
	/// <param name="baseGodotPath">A relative path to the base folder, in the "res://" space</param>
	/// <param name="librarys">List of GFXLibrarys to save</param>
	private static void SaveLibrarysInList(bool packFiles, PCKPacker p, string basePath, string baseGodotPath, List<GFXLibrary> librarys) {
		foreach (var lib in librarys) {
			string libPath = basePath + "/" + lib.name;
			string libGodotPath = baseGodotPath + "/" + lib.name;

			currentLibrary = lib.name;

			if (!Directory.Exists(libPath))
				Directory.CreateDirectory(libPath);

			SaveFiles(packFiles, p, lib, libPath, libGodotPath);
		}
	}

	/// <summary>
	/// Saves every file inside a GFXLibrary
	/// </summary>
	/// <param name="packFiles">Should the files be packed into a .pck file</param>
	/// <param name="p">The PCKPacker for the packing, can be null when packFiles is false</param>
	/// <param name="lib">The GFXLibrary to save</param>
	/// <param name="libPath">The absolute path to save the files to</param>
	/// <param name="libGodotPath">The relative path to save the files to, in "res://" space</param>
	private static void SaveFiles(bool packFiles, PCKPacker p, GFXLibrary lib, string libPath, string libGodotPath) {
		foreach (GFXLibrary.GFXFile file in lib.files) {
			string fileName = file.name.Trim('\0') + ".res";
			string filePath = libPath + "/" + fileName;
			string fileGodotPath = libGodotPath + "/" + fileName;

			currentFile = fileName;

			//GD.Print(filePath);
			//GD.Print(fileGodotPath);

			Texture resource = file.GetTexture();
			if (resource == null)
				continue;

			ResourceSaver.Save(filePath, resource, (int)ResourceSaver.SaverFlags.Compress);

			if (packFiles)
				p.AddFile(fileGodotPath, filePath);

		}
	}
}

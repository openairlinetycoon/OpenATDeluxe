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
	private const string FileSystemVersion = "b529608a055d4db2eb8089d4a6fe5e9e3827332e"; //What version/checksum is the current file decryptor. For forced rebuilds on changes. CURRENTLY UNTESTED CHECKSUM!
	private const string PackFile = "ATDFiles.pck";
	private const string ImagesPath = "res://Images";
	private const string ATDPathConfig = "application/config/atd_path";
	public Label loadInfo, loadInfoFiles;
	public FileDialog selectATDPath;
	public AcceptDialog directoryInvalidDialog;

	static Dictionary<string, List<GFXLibrary>> libraryFolders;

	[Export]
	bool forceRebuild = false;
	static bool isInEditor; //TODO:Refractor this to the game class!

	static bool otherDataLoaded;

	ResourceInteractiveLoader gameLoader;

	static volatile string otherLoading;
	static volatile string currentFolder;
	static volatile string currentLibrary;
	public static volatile string currentFile;

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

		//If the current path is correct, check for the "correctness" of our files
		ValidateFiles();
	}

	public static bool IsOriginalGamePath(string dir) {
		if (!Directory.Exists(dir))
			return false;
		return SystemFile.Exists(FindFolder("gli", dir) + "/glbasis.gli");
	}

	private void ValidateFiles() {
		StartImageRebuild();
		// if (forceRebuild || (IsDataVersionWrong() && !isInEditor) || (isInEditor && !Directory.Exists(ProjectSettings.GlobalizePath(ImagesPath)))) {
		// 	StartImageRebuild(); //Files are corrupt/wrong/missing!
		// } else { //If there is no need for a rebuild, just load the data and leave
		// 	if (!isInEditor) {
		// 		ProjectSettings.LoadResourcePack(PackFile); //If we are not in the editor: add the ATDFiles
		// 	}
		// 	LoadOtherData();

		// 	SetProcess(true);
		// 	gameLoader = ResourceLoader.LoadInteractive("res://scenes/base.tscn");

		// 	//GetTree().ChangeScene("res://scenes/base.tscn");
		// }
	}

	private static string FindFolder(string folderName, string basePath = "") {
		basePath = basePath == "" ? GFXLibrary.pathToAirlineTycoonD : basePath;
		return Directory.GetDirectories(basePath, folderName, System.IO.SearchOption.AllDirectories).First();
	}

	private void StartImageRebuild() {
		GD.Print($"Rebuilding images... Are we in the editor? {isInEditor}");

		//State possible reasons, we are not here to judge, we just execute
		if (forceRebuild)
			GD.Print("Forced rebuild active!");
		if (!SystemFile.Exists(PackFile))
			GD.Print($"{PackFile} does not exist!");
		if (!Directory.Exists(ProjectSettings.GlobalizePath(ImagesPath)))
			GD.Print($"{ProjectSettings.GlobalizePath(ImagesPath)} does not exist!");

		Thread t = new Thread(CreateData); //Task?
		t.Name = "DataLoader";

		t.Start();
	}

	private bool IsInEditor() {
		return SystemFile.Exists("default_env.tres") && SystemFile.Exists("default_bus_layout.tres"); //They should not exists outside of the editor
	}

	private bool IsDataVersionWrong() {
		if (!SystemFile.Exists(PackFile)) {
			return true;
		}

		loadInfo.SetText("Creating checksum...");

		using (SHA1 currentFileChecksum = SHA1.Create()) {
			byte[] hash = currentFileChecksum.ComputeHash(SystemFile.Open(PackFile, System.IO.FileMode.Open));
			string stringHash = BitConverter.ToString(hash).Replace("-", String.Empty).ToLowerInvariant();
			GD.Print($"Your hash: \"{ stringHash}\", correct hash: \"{FileSystemVersion}\"");
			return stringHash != FileSystemVersion;
		}
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
		//SetProcess(false);
		loadInfoFiles.SetText("Loading: " + currentLibrary);
		loadInfo.SetText("Loading " + otherLoading + (((OS.GetTicksMsec() / 500) % 3 == 0) ? "." : ((OS.GetTicksMsec() / 500) % 3 == 1) ? ".." : "..."));
		// 	
		// //loadInfo.SetText("Loading: " + currentFolder + "/" + currentLibrary + "/" + currentFile);

		// if (GetNode<Sprite>("Title").GetTexture() == null) {
		// 	GetNode<Sprite>("Title").SetTexture((Texture)ResourceLoader.Load("res://Images/room/titel/TITEL.res"));
		// }

		// int time = OS.GetTicksMsec();
		// while (OS.GetTicksMsec() < time + 100) {
		// 	Error state = gameLoader.Poll();

		// 	if (state == Error.FileEof && AllDataLoaded()) {
		// 		Resource newScene = gameLoader.GetResource();
		// 		gameLoader = null;

		// 		var root = GetTree().GetRoot();
		// 		root.GetChild(root.GetChildCount() - 1).QueueFree();
		// 		Node node = ((PackedScene)newScene).Instance();
		// 		root.AddChild(node);
		// 		return;
		// 		//ChangeScene("res://scenes/base.tscn");

		// 	} else if (state == Error.FileEof) {
		// 		loadInfo.SetText("Loading " + otherLoading + (((OS.GetTicksMsec() / 500) % 3 == 0) ? "." : ((OS.GetTicksMsec() / 500) % 3 == 1) ? ".." : "..."));
		// 	} else if (state == Error.Ok) {
		// 		int progress = 100 / gameLoader.GetStageCount() * gameLoader.GetStage();
		// 		loadInfo.SetText("Loading scene: " + progress + "%");
		// 	}
		// }


	}

	private bool AllDataLoaded() {
		return otherDataLoaded;
	}

	public void ExitGame() {
		selectATDPath.GetCloseButton().
		GetTree().Quit();
	}

	public void ChoseFile(string dir) {
		if (!IsOriginalGamePath(dir)) { //Basic check to see if we are inside the ATD folder
			GD.PrintErr("INVALID PATH CHOSEN, CAN'T FIND glbasis.gli IN SUBFOLDER gli!");
			directoryInvalidDialog.PopupCentered();
			return;
		}
		SettingsManager.SetSetting(ATDPathConfig, dir);
		GFXLibrary.pathToAirlineTycoonD = dir;

		ValidateFiles();
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
		List<Exception> exceptions = new List<Exception>();
		string dataFolder = FindFolder("data") + "/";
		string soundFolder = FindFolder("sound") + "/";

		exceptions.Add(TryAction(() => {
			otherLoading = "Music";
			string[] midFiles = System.IO.Directory.GetFiles(soundFolder, "*.mid");
			string[] oggFiles = System.IO.Directory.GetFiles(soundFolder, "*.ogg");

			if (oggFiles.Length != 0) { //OGG Files are preferred, as there currently is only a buggy Midi player available
				MusicController.musicFiles = Song.CreateFromFiles(oggFiles, Song.SongTypes.Ogg);
				MusicController.isOgg = true;
			} else {
				MusicController.musicFiles = Song.CreateFromFiles(oggFiles, Song.SongTypes.Mid);
			}
		}));

		exceptions.Add(TryAction(() => {
			otherLoading = "Clan CSV";
			ClanCSVFile clanFile = new ClanCSVFile(dataFolder + "clan.csv");
		}));

		exceptions.Add(TryAction(() => {
			otherLoading = "Brick CSV";
			CSVFileDecoder decoder = new CSVFileDecoder(dataFolder + "brick.csv");
		}));

		exceptions.Add(TryAction(() => {
			otherLoading = "Settings";
			SettingsManager.LoadSavedData();
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
		LoadImages();
		LoadOtherData();

		GetTree().ChangeScene("res://scenes/base.tscn");
		//CallDeferred(nameof(Start)); //No need for the interactive loader, everything is already in memory!
	}
	//10.92
	//


	/// <summary>
	/// Loads, Saves (when neccessary) and packes all .gli files in "room" and "gli"
	/// </summary>
	private void LoadImages() {
		SetProcess(true);

		SetFolders();

		Debug.Assert(GetNode<Sprite>("Title") != null, "Invalid scene structure! Title child node missing!");
		Debug.Assert(libraryFolders[FindFolder("room")].Find((lib) => lib.name == "titel") != null, "Missing room gli library folder! Check your files for completion! (title.gli)");

		Debug.Assert(
			libraryFolders[FindFolder("room")].Find((lib) => lib.name == "titel").files.Find((f) => f.name.Trim('\0') == "TITEL") != null,
			 "Missing TITLE room file in GFXLibrary titel from room!");

		GetNode<Sprite>("Title").SetTexture(
			libraryFolders[FindFolder("room")]
				.Find((lib) => lib.name == "titel").files
				.Find((f) => f.name.Trim('\0') == "TITEL").GetTexture());


		//Write them to disk
		ImportImages();

		//Should only be needed when we are not in the editor, but adding it doesnt hurt us, even if the file isn't present
		//bool s = ProjectSettings.LoadResourcePack(PackFile);
	}

	public static void SetFolders(string folder = "") {
		libraryFolders = new Dictionary<string, List<GFXLibrary>>(); //A list of graphic folders of the game
		libraryFolders.Add(FindFolder("room", folder), new List<GFXLibrary>());
		libraryFolders.Add(FindFolder("gli", folder), new List<GFXLibrary>());

		foreach (var folderKVP in libraryFolders) {
			string folderName = folderKVP.Key;
			List<GFXLibrary> folderGFXList = folderKVP.Value;

			//currentFolder = folderName;

			//Load up all GFX files in the directory
			string[] files = System.IO.Directory.GetFiles(folderName);
			foreach (string f in files) {
				if (f.ToLower().EndsWith(".gli")) {
					var lib = new GFXLibrary(folderName + "/" + System.IO.Path.GetFileName(f));
					lib.GetFilesInLibrary();
					folderGFXList.Add(lib);
				}
			}
		}
	}

	public void Start() {
		GetTree().ChangeScene("res://scenes/base.tscn");
	}

	/// <summary>
	/// Saves all gli files in the libraryFolders dictionary to the "ImagesPath" folder
	/// </summary>
	/// <param name="packFiles">Should the files be packed into a .pck file</param>
	public static void ImportImages(bool saveFiles = false, CancellationToken cancelation = default(CancellationToken)) {
		string basePath = ProjectSettings.GlobalizePath(ImagesPath); //Get an absolute Path to our project/executable folder
		string baseGodotPath = ImagesPath;

		if (!Directory.Exists(basePath) && saveFiles)
			Directory.CreateDirectory(basePath);

		foreach (var folderKVP in libraryFolders) {
			List<GFXLibrary> libraries = folderKVP.Value;
			string libraryName = Directory.GetParent(folderKVP.Key + "/").Name;

			string libbasePath = basePath + "/" + libraryName;
			string libbaseGodotPath = baseGodotPath + "/" + libraryName;

			//List<Task> loader = new List<Task>();

			Parallel.ForEach(libraries, (lib) => {
				string libPath = libbasePath + "/" + lib.name;
				string libGodotPath = libbaseGodotPath + "/" + lib.name;


				if (!Directory.Exists(libPath) && saveFiles)
					Directory.CreateDirectory(libPath);

				cancelation.ThrowIfCancellationRequested();

				currentLibrary = lib.name;
				lib.Open();
				ImportFile(lib, libPath, libGodotPath, saveFiles, cancelation);
				lib.Close();
			});

			// foreach (var lib in libraries) {
			// 	string libPath = libbasePath + "/" + lib.name;
			// 	string libGodotPath = libbaseGodotPath + "/" + lib.name;


			// 	if (!Directory.Exists(libPath) && saveFiles)
			// 		Directory.CreateDirectory(libPath);

			// 	cancelation.ThrowIfCancellationRequested();

			// 	currentLibrary = lib.name;
			// 	lib.Open();
			// 	ImportFile(lib, libPath, libGodotPath, saveFiles, cancelation);
			// 	lib.Close();
			// 	//t.Start();

			// 	//loader.Add(t);
			// }

			//Task.WaitAll(loader.ToArray());
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
	private static void ImportFile(GFXLibrary lib, string libPath, string libGodotPath, bool saveFiles = false, CancellationToken cancelation = default(CancellationToken)) {
		Stopwatch getTexture = new Stopwatch();
		Stopwatch fileExists = new Stopwatch();
		Stopwatch takeOver = new Stopwatch();

		foreach (GFXLibrary.GFXFile file in lib.files) {
			cancelation.ThrowIfCancellationRequested();

			string fileName = file.name.Trim('\0') + ".res";
			string filePath = libPath + "/" + fileName;
			string fileGodotPath = libGodotPath + "/" + fileName;

			currentFile = fileName;

			//currentFile = fileName;
			//GD.Print(filePath);
			//GD.Print(fileGodotPath);
			getTexture.Start();
			Texture resource = file.GetTexture();
			getTexture.Stop();
			if (resource == null)
				continue;

			if (saveFiles) {
				Error e = ResourceSaver.Save(filePath, resource, (int)ResourceSaver.SaverFlags.Compress);
				if (e != 0) {
					GD.Print($"Error: " + e.ToString());
				}
				//GD.Print($"Texture: {file.name.Trim('\n', '\t', '\0')} not found in the editor! Now adding...");
			} else {
				takeOver.Start();
				resource.TakeOverPath(fileGodotPath);
				takeOver.Stop();
			}
		}

		GD.Print($"Loaded {lib.name}! getTextures took {getTexture.ElapsedMilliseconds}ms, fileExists took {fileExists.ElapsedMilliseconds}ms, takeOver took {takeOver.ElapsedMilliseconds}ms,");
	}
}

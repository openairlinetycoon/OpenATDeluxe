using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Directory = System.IO.Directory;
using Thread = System.Threading.Thread;
using System.Diagnostics;

public class ATDGameLoader : Node2D {
	private const string PackFile = "ATDFiles.pck";
	private const string ImagesPath = "res://Images";
	public Label loadInfo;
	public FileDialog selectATDPath;
	public AcceptDialog directoryInvalidDialog;

	static Dictionary<string, List<GFXLibrary>> libraryFolders;

	[Export]
	bool forceRebuild = false;
	bool isInEditor;

	ResourceInteractiveLoader gameLoader;

	static volatile string currentFolder;
	static volatile string currentLibrary;
	static volatile string currentFile;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		loadInfo = GetNode<Label>("LoadInfo");

		isInEditor = Engine.EditorHint;

		if (forceRebuild || (!System.IO.File.Exists(PackFile) && !Directory.Exists(ProjectSettings.GlobalizePath(ImagesPath)))) {
			selectATDPath = GetNode<FileDialog>("FileDialog"); //Get the path to the ATD install
			selectATDPath.Connect("dir_selected", this, nameof(ChoseFile));
			selectATDPath.PopupCentered(new Vector2(500, 500));

			directoryInvalidDialog = GetNode<AcceptDialog>("DirectoryInvalid");
			directoryInvalidDialog.Connect("confirmed", this, nameof(AcceptDialog));
		} else { //If there is no need for a rebuild, just skip asking
			if (!isInEditor) {
				ProjectSettings.LoadResourcePack(PackFile); //If we are not in the editor: we need to add the ATDFiles
			}

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

			if (state == Error.FileEof) {
				Resource newScene = gameLoader.GetResource();
				gameLoader = null;

				var root = GetTree().GetRoot();
				root.GetChild(root.GetChildCount() - 1).QueueFree();
				root.AddChild(((PackedScene)newScene).Instance());
				return;
				//ChangeScene("res://scenes/base.tscn");

			} else if (state == Error.Ok) {
				int progress = 100 / gameLoader.GetStageCount() * gameLoader.GetStage();
				loadInfo.SetText("Loading scene: " + progress + "%");
			}
		}


	}

	public void ChoseFile(string dir) {
		if (!System.IO.File.Exists(dir + "/gli/glbasis.gli")) { //Basic check to see if we are inside the ATD folder
			GD.PrintErr("INVALID PATH CHOSEN, CAN'T FIND glbasis.gli IN SUBFOLDER gli!");
			directoryInvalidDialog.PopupCentered();
			return;
		}

		GFXLibrary.pathToAirlineTycoonD = dir;

		Thread t = new Thread(LoadData);
		t.Name = "DataLoader";

		t.Start();
	}

	public void AcceptDialog() {
		selectATDPath.PopupCentered(new Vector2(500, 500)); //try try and try again
	}

	public void LoadData() {
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
		GetTree().ChangeScene("res://scenes/base.tscn");
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

			GD.Print(filePath);
			GD.Print(fileGodotPath);

			ResourceSaver.Save(filePath, file.GetTexture(), (int)ResourceSaver.SaverFlags.Compress);

			if (packFiles)
				p.AddFile(fileGodotPath, filePath);

		}
	}
}

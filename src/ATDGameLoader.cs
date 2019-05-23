using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Directory = System.IO.Directory;
using Thread = System.Threading.Thread;
using System.Diagnostics;

public class ATDGameLoader : Node {
	public Label loadInfo;
	public FileDialog selectATDPath;
	public AcceptDialog directoryInvalidDialog;

	static Dictionary<string, List<GFXLibrary>> libraryFolders;

	[Export]
	bool forceRebuild = false;
	bool isInEditor;

	static volatile string currentFolder;
	static volatile string currentLibrary;
	static volatile string currentFile;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		loadInfo = GetNode<Label>("LoadInfo");

		isInEditor = Engine.EditorHint;

		if (forceRebuild || (!System.IO.File.Exists("ATDFiles.pck") && !Directory.Exists(ProjectSettings.GlobalizePath("res://Images/")))) {
			selectATDPath = GetNode<FileDialog>("FileDialog"); //Get the path to the ATD install
			selectATDPath.Connect("dir_selected", this, nameof(ChoseFile));
			selectATDPath.PopupCentered(new Vector2(500, 500));

			directoryInvalidDialog = GetNode<AcceptDialog>("DirectoryInvalid");
			directoryInvalidDialog.Connect("confirmed", this, nameof(AcceptDialog));
		} else { //If there is no need for a rebuild, just skip asking
			if (!isInEditor) {
				ProjectSettings.LoadResourcePack("ATDFiles.pck"); //If we are not in the editor: we need to add the ATDFiles
			}
			GetTree().ChangeScene("res://scenes/base.tscn");
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

	public override void _Process(float delta) {
		loadInfo.SetText("Loading: " + currentFolder + "/" + currentLibrary + "/" + currentFile);
	}

	private void LoadData() {
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
		ExportImages(!System.IO.File.Exists("ATDFiles.pck") && !isInEditor);

		//Should only be needed when we are not in the editor, but adding it doesnt hurt us, even if the file isn't present
		bool s = ProjectSettings.LoadResourcePack("ATDFiles.pck");
		GetTree().ChangeScene("res://scenes/base.tscn");
	}

	private void ExportImages(bool packFiles) {
		PCKPacker p = new PCKPacker();


		if (packFiles) //Should we pack them up? Only needed when we are not inside the editor
			p.PckStart("ATDFiles.pck", 4);


		string basePath = ProjectSettings.GlobalizePath("res://Images"); //Get an absolute Path to our project/executable folder
		string baseGodotPath = "res://Images";

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

	private static void SaveFiles(bool packFiles, PCKPacker p, GFXLibrary lib, string libPath, string libGodotPath) {
		foreach (GFXLibrary.GFXFile file in lib.files) {
			string fileName = file.name.Trim('\0') + ".tres";
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

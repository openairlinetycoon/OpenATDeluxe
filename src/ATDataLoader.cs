using System;
using System.Collections.Generic;
using Godot;
using System.Linq;
using SDirectory = System.IO.Directory;
using SPath = System.IO.Path;
using System.Threading.Tasks;

public class ATDataLoader {

	static List<GFXLibrary> libraries = new List<GFXLibrary>();
	static List<Resource> resources = new List<Resource>();

	public static void LoadCSVData() {
		string dataFolder = ATFile.FindFolder("data") + "/";
		ClanCSVFile clanFile = new ClanCSVFile(dataFolder + "clan.csv");
		CSVFileDecoder decoder = new CSVFileDecoder(dataFolder + "brick.csv");
	}

	public static void LoadMusicData() {
		string soundFolder = ATFile.FindFolder("sound") + "/";
		string[] midFiles = SDirectory.GetFiles(soundFolder, "*.mid");
		string[] oggFiles = SDirectory.GetFiles(soundFolder, "*.ogg");

		if (oggFiles.Length != 0) { //OGG Files are preferred, as there currently is only a buggy Midi player available
			MusicController.musicFiles = Song.CreateFromFiles(oggFiles, Song.SongTypes.Ogg);
			MusicController.isOgg = true;
		} else {
			MusicController.musicFiles = Song.CreateFromFiles(midFiles, Song.SongTypes.Mid);
		}
	}

	public static void LoadImageData() {
		var libraryFolders = new List<string>(); //A list of graphic folders of the game
		libraryFolders.Add(ATFile.FindFolder("room"));
		libraryFolders.Add(ATFile.FindFolder("gli"));

		foreach (var folderKVP in libraryFolders) {
			string folderName = folderKVP;

			//currentFolder = folderName;

			//Load up all GFX files in the directory
			string[] files = SDirectory.GetFiles(folderName);
			Parallel.ForEach(files, (f) => {
				//foreach (string f in files) {
				if (!f.EndsWith(".gli", StringComparison.OrdinalIgnoreCase))
					return;

				var lib = new GFXLibrary(folderName + "/" + System.IO.Path.GetFileName(f));
				lib.GetFilesInLibrary();
				libraries.Add(lib);

				//}
			});
		}
	}

	public static void CreateImageResources() {
		foreach (var lib in libraries) {
			string godotPath = "res://Images/" + SPath.GetFileName(SPath.GetDirectoryName(lib.pathToGFXFile)) + "/" + SPath.GetFileNameWithoutExtension(lib.pathToGFXFile) + "/";

			foreach (var file in lib.files) {
				string path = godotPath + file.name.Trim('\0') + ".res";
				Texture resource = file.GetTexture();
				resource?.TakeOverPath(path);
				resources.Add(resource);
			}
		}
	}

	public static void SaveAllLoadedResources() {
		int all = resources.Count;
		int i = 0;
		foreach (Resource r in resources) {
			i++;
			if (r == null)
				continue;

			string path = r.ResourcePath;
			path = path.Remove(0, "res://".Length);

			if (i % 50 == 0) {
				GD.Print(i + "/" + all);
			}

			if (!SDirectory.Exists(SPath.GetDirectoryName(path))) {
				SDirectory.CreateDirectory(SPath.GetDirectoryName(path));
			}

			Error e = ResourceSaver.Save(path, r);
			if (e != 0) {
				GD.Print(e.ToString());
			}
		}
	}
}
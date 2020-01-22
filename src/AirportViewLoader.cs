using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Directory = System.IO.Directory;
using File = Godot.File;
using Path = System.IO.Path;
using System.Text;

public class AirportViewLoader : Node {
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	[Export]
	public string basePath = "";
	[Export]
	public NodePath baseScene;
	Node2D _baseScene;

	public int currentID;

	struct BrickInfo {
		public string path, name;
		public int xOffset, yOffset, zIndex;

	}
	Dictionary<long, BrickInfo> bricks = new Dictionary<long, BrickInfo>();

	public void PrepareBricks() {
		//TODO: Decrypt the CSV Files in here!
		//@"I:\Downloads\Kopie von Airline Tycoon Deluxe_Steam Files_2018-06-02\Airline Tycoon Deluxe\steamapps\common\Airline Tycoon Deluxe\data\
		using (var reader = new StreamReader(basePath + "/data/brick.csvs")) {

			reader.ReadLine();//Header
			reader.ReadLine();//Empty Line


			List<string> listA = new List<string>();
			List<string> listB = new List<string>();
			while (!reader.EndOfStream) {
				var line = reader.ReadLine();//Content
				var values = line.Split(';');

				if (values.Length == 0)
					continue; //Empty line!

				if (values[0] == "-")
					continue; //Other data belonging to the prev. clan - skipped for now

				long id = Convert.ToInt32(values[0]);
				string fileName = values[15];

				bool hasLuggage = Convert.ToInt32(values[0]) > 0;

				if (fileName.Contains(' ') || fileName.Contains(':')) {
					//We can't "really" process ranges yet! TODO
					//a space indicates another sprite
					//a : indicates a repetition of the preceeding sprite

					fileName = fileName.Split(' ', ':').First();
				}

				BrickInfo info = new BrickInfo();

				info.path = FindFile(fileName);
				info.name = Path.GetFileNameWithoutExtension(info.path);
				info.xOffset = Convert.ToInt32(values[8]);
				info.yOffset = Convert.ToInt32(values[9]);
				info.zIndex = Convert.ToInt32(values[5]);
				bricks.Add(id, info);
			}
		}


	}

	public string FindFile(string name) {
		return Directory.GetFiles(ProjectSettings.GlobalizePath("res://Images/gli") + "/", name + ".*", System.IO.SearchOption.AllDirectories).FirstOrDefault();
	}

	public void CreateSprite(long id, int x, int y, int par) {
		currentID++;

		Sprite s = new Sprite();
		_baseScene.AddChild(s);
		s.SetOwner(_baseScene);

		if (x > 176) {
			//x += (44 * 5 - 1056) / 176 * 176 + 176;
		}
		s.Centered = false;
		s.ZIndex = bricks[id].zIndex;
		s.SetPosition(new Vector2(x, y));
		s.Name = bricks[id].name + " " + currentID;
		if (par != 0)
			s.Visible = false;

		s.SetTexture(ResourceLoader.Load<Texture>(bricks[id].path));
	}

	public override void _Ready() {
		_baseScene = GetNode<Node2D>(baseScene);

		PrepareBricks();

		string[] files = Directory.GetFiles(basePath + "/misc/",
			"*.dat");

		foreach (string filePath in files) {
			File f = new File();
			f.Open(filePath, File.ModeFlags.Read);

			currentID = 0;
			LoadLevelFile(f);

			_baseScene.Name = Path.GetFileNameWithoutExtension(filePath);

			PackedScene save = new PackedScene();
			save.Pack(_baseScene);

			ResourceSaver.Save("res://scenes/airportPartsImport/" + _baseScene.Name + ".tscn", save);
			foreach (Node n in _baseScene.GetChildren()) {
				_baseScene.RemoveChild(n);
				n.QueueFree();
			}

			f.Close();
		}
	}

	private void LoadLevelFile(File f) {
		int fileAmount = (int)f.Get32();
		GD.Print("Files estimated: " + fileAmount);

		f.Get32();

		for (int i = 0; i < fileAmount; i++) {
			long id = f.Get32();
			id -= 0X10000000;


			int x = (int)f.Get32();
			int y = (int)f.Get32();
			int par = f.Get8();

			if (!bricks.ContainsKey(id))
				GD.Print("ID not found! ID: " + id);
			else {
				if (bricks[id].name.Contains("RUNE"))
					continue;

				CreateSprite(id, x, y, par);

				GD.Print("GFX - Name: " + bricks[id].name
				 + " at x: " + x + "+ " + bricks[id].xOffset
				 + ", y: " + y + "+ " + bricks[id].yOffset
				 + " PAR: " + par);
			}

		}
	}
}

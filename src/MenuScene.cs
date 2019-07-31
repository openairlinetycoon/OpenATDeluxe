using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MenuScene : Node2D {
	public static MenuScene instance;

	[Export]
	public NodePath _textGrid;
	public GridContainer textGrid;
	public Control[,] grid;

	[Export]
	public NodePath _klackerPlayer;
	public AudioStreamPlayer klackerPlayer;
	public static bool klackersEnabled = true;

	const int CharA = 'A';
	const int CharZ = 'Z';
	public const int MaxTextLength = 24;
	public const string TextImagesPath = "res://Images/room/klacker/";
	public readonly static string SoundPath = GFXLibrary.pathToAirlineTycoonD + "/SOUND/";
	public const string FilePrefixNormal = "KL_", FilePrefixDark = "KD_";

	public Dictionary<char, string> exceptions = new Dictionary<char, string>();

	List<Task> klackerTasks;
	public IBaseMenu currentScene;

	Random r = new Random(DateTime.Now.Millisecond);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		instance = this;

		MusicController.isInMainMenu = true;
		MusicController.instance.SetSong("at2");

		textGrid = GetNode<GridContainer>(_textGrid);
		klackerPlayer = GetNode<AudioStreamPlayer>(_klackerPlayer);

		GameController.instance?.SetTaskbar(false);

		PopulateGrid();
		PopulateExceptions();
		PopulateMenuScenes();


		PrepareMenuScene(new MainMenu());
	}

	private void PopulateExceptions() {
		exceptions.Add('Ä', "AE");
		exceptions.Add('&', "AMP");
		exceptions.Add('\'', "ANF1");
		exceptions.Add('"', "ANF2");
		exceptions.Add('!', "AUSR");
		exceptions.Add('~', "KREUZ");
		exceptions.Add(':', "DPKT");
		exceptions.Add('?', "FRAGE");
		exceptions.Add('=', "GLCH");
		exceptions.Add('(', "KLA");
		exceptions.Add(')', "KLZ");
		exceptions.Add(',', "KOMMA");
		exceptions.Add('#', "BULL");
		exceptions.Add('<', "LESS");
		exceptions.Add('*', "MAL");
		exceptions.Add('-', "MINUS");
		exceptions.Add('>', "MORE");
		exceptions.Add('Ö', "OE");
		exceptions.Add('.', "PKT");
		exceptions.Add('+', "PL");
		exceptions.Add(';', "SEMI");
		exceptions.Add('/', "SLASH");
		exceptions.Add(' ', "SP");
		exceptions.Add('Ü', "UE");
	}

	private void PopulateMenuScenes() {
		klackerTasks = new List<Task>();
	}

	private void PrepareMenuScene(IBaseMenu name, bool klackerOnlyChanges = false) {
		List<MenuItem> scene = name.GetMenuItems();
		int lineSkips = 0;
		for (int line = 0; line < scene.Count; line++) {
			if (scene[line].type == MenuItem.EntryType.Slider)
				lineSkips++;
			string text = scene[line].text;
			for (int x = 0; x < text.Length; x++) {
				if (x >= MaxTextLength)
					break;

				string output = CheckExceptions(text[x]);

				Sprite s = CreateTextSprite(scene[line], output);

				int xPos = x, yPos = line - lineSkips;
				switch (scene[line].type) {
					case MenuItem.EntryType.MoveLeft:
						yPos = grid.GetUpperBound(1);
						break;
					case MenuItem.EntryType.Slider:
						xPos = grid.GetUpperBound(0) - text.Length + x + 1;
						break;
					case MenuItem.EntryType.MoveRight:
						yPos = grid.GetUpperBound(1);
						xPos = grid.GetUpperBound(0) - text.Length + x + 1;
						break;
				}

				if (xPos >= MaxTextLength)
					break;

				grid[xPos, yPos].AddChild(s);
				CharacterItem characterItem = ((CharacterItem)grid[xPos, yPos]);
				bool dontKlacker = klackerOnlyChanges & characterItem.character != output;

				characterItem.AssignedMenuItem = scene[line];
				characterItem.character = output;
				characterItem.stringPosition = x;

				//grid[x, line].Connect("mouse_entered", grid[x, line], "OnMouseEnter");
				//((CharacterItem)grid[x, line]).OnMouseEnter();

				if (dontKlacker == false)
					klackerTasks.Add(AnimateText(scene[line], s, text[x]));
			}
		}

		Task.Run(PlayKlackers);
	}

	public void ChangeScene(IBaseMenu scene, bool klackerOnlyChanges = false) {

		foreach (Control g in grid) {
			CharacterItem characterItem = (g as CharacterItem);
			characterItem.AssignedMenuItem = null;
			characterItem.character = "";

			if (characterItem.IsConnected("mouse_entered", g, "MouseEntered")) {
				characterItem.Disconnect("mouse_entered", g, "MouseEntered");
				characterItem.Disconnect("mouse_exited", g, "MouseExited");
			}

			if (g.GetChildCount() == 1)
				g.GetChild(0)?.QueueFree();
		}

		MouseCursor.instance.ChangeMouseState(MouseCursor.MouseState.Normal);

		currentScene = scene;

		PrepareMenuScene(scene, klackerOnlyChanges);
	}

	private async Task AnimateText(MenuItem item, Sprite text, char current) {
		int turns = 0;
		int maxTurns = 5 + r.Next(-2, 5);

		while (turns < maxTurns) {
			turns++;

			int randomStart = r.Next(CharA, CharZ);

			string output = CheckExceptions((char)randomStart);

			if (!IsInstanceValid(text))
				return;
			text.Texture = (Texture)ResourceLoader.Load(GetFilePath(output, item.TypeFace));

			await Task.Delay(60);
		}


		if (IsInstanceValid(text))
			text.Texture = (Texture)ResourceLoader.Load(GetFilePath(CheckExceptions(current), item.TypeFace));
	}

	private async Task PlayKlackers() {
		if (!klackersEnabled)
			return;

		AudioStreamSample[] audioFiles = new AudioStreamSample[3];
		audioFiles[0] = new AudioStreamSample();
		audioFiles[1] = new AudioStreamSample();
		audioFiles[2] = new AudioStreamSample();

		List<AudioStreamPlayer> oneShotAudios = new List<AudioStreamPlayer>();

		byte[] data = System.IO.File.ReadAllBytes(SoundPath + "Klack0.raw");
		audioFiles[0].SetData(data);
		audioFiles[0].MixRate = 44100;

		data = System.IO.File.ReadAllBytes(SoundPath + "Klack1.raw");
		audioFiles[1].SetData(data);
		audioFiles[1].MixRate = 44100;

		data = System.IO.File.ReadAllBytes(SoundPath + "Klack2.raw");
		audioFiles[2].SetData(data);
		audioFiles[2].MixRate = 44100;

		klackerPlayer.SetStream(audioFiles[0]);

		Task t = Task.WhenAll(klackerTasks);



		while (!t.IsCompleted) {
			AudioStreamPlayer p = new AudioStreamPlayer();
			oneShotAudios.Add(p);
			AddChild(p);
			p.SetStream(audioFiles[r.Next(0, 2)]);
			p.Play();
			p.SetBus("soundFX");
			await Task.Delay(60);
		}

		foreach (var player in oneShotAudios) {
			player.QueueFree();
		}
	}

	private static bool IsLetterInAlphabet(char letter) {
		return letter > CharA && letter < CharZ;
	}

	private string CheckExceptions(char input) {
		string output = input.ToString();

		if (exceptions.ContainsKey(input)) {
			output = exceptions[input];
		}

		return output;
	}



	private void PopulateGrid() {
		// 24x16
		grid = new Control[24, 16];

		for (int y = 0; y < 16; y++) {
			for (int x = 0; x < 24; x++) {
				grid[x, y] = CreateControl();
				grid[x, y].Name = "X" + x + " Y" + y;
			}
		}
	}

	public Control CreateControl() {
		CharacterItem c = new CharacterItem();
		c.RectMinSize = new Vector2(16, 22);

		textGrid.AddChild(c);
		c.SetOwner(textGrid);
		return c;
	}
	public Sprite CreateTextSprite(MenuItem item, string character) {
		Sprite s = new Sprite();
		s.Name = character;
		s.Centered = false;
		s.Texture = (Texture)ResourceLoader.Load(GetFilePath(character, item.TypeFace));
		return s;
	}

	private static string GetFilePath(string character, string typeFace) {
		return TextImagesPath + typeFace + character.ToUpper() + ".res";
	}

	override public void _ExitTree() {
		GameController.instance.SetTaskbar(true);
	}
}

public class MenuItem {
	public string text;
	public string TypeFace {
		get {
			switch (type) {
				case (EntryType.LinkBlocked):
					return MenuScene.FilePrefixDark;
				default:
					return MenuScene.FilePrefixNormal;
			}
		}
	}


	public enum EntryType {
		Undefined,
		Space,
		Slider, //Eg. volume - INLINE
		Link,
		LinkBlocked,
		MoveLeft,
		MoveRight,
		TextField,
		HeaderBar,
		Header,
	}

	public EntryType type = EntryType.Undefined;

	public bool klackerOnlyChanges = false;

	public Action OnClick;
	public Action<int> OnClickSpecial;



	public MenuItem() {
		text = "";
		OnClick = null;
		type = EntryType.Space;
	}

	public MenuItem(string text, Action onClick) {
		this.text = text;
		OnClick = onClick;
	}
	public MenuItem(EntryType type) : this("", type, true) {

	}

	public MenuItem(string text, EntryType type, bool fillWithType = true) {
		this.text = text;
		this.type = type;

		if (type == EntryType.Slider)
			GD.PrintErr("Warning, using base MenuItem for Slider! Use SliderItem instead!");

		if (fillWithType) {
			switch (type) {
				case (EntryType.HeaderBar):
					this.text = "========================";
					break;
			}
		}


		OnClick = null;
	}
	public MenuItem(string text, EntryType type, Action onClick, bool fillWithType = true) : this(text, type, fillWithType) {
		OnClick = onClick;
	}
}


public class SliderItem : MenuItem {
	public static int ConvertRange(
		int originalStart, int originalEnd, // original range
		int newStart, int newEnd, // desired range
		int value) // value to convert
	{
		float scale = (float)(newEnd - newStart) / (originalEnd - originalStart);
		return (int)(newStart + ((value - originalStart) * scale));
	}

	public SliderItem(Func<int> value, Action<int> setValue, int rangeStart = -40, int rangeEnd = 3, int mute = -70) : base("", EntryType.Slider, true) {
		klackerOnlyChanges = true;
		OnClick = null;

		int position = ConvertRange(rangeStart, rangeEnd, 0, 6, value());
		position = Mathf.Clamp(position, 0, 6);

		text = "------- ";
		text = text.Remove(position, 1).Insert(position, "~");

		OnClickSpecial += (pos) =>
		{
			pos = Mathf.Clamp(pos, 0, 6);
			int newVolume = pos == 0 ? -80 : ConvertRange(0, 6, rangeStart, rangeEnd, pos);

			setValue(newVolume);

			text = "------- ";
			text = text.Remove(pos, 1).Insert(pos, "~");

			MenuScene.instance.ChangeScene(MenuScene.instance.currentScene, klackerOnlyChanges);
		};
	}
}

public class MenuChangeItem : MenuItem {
	public IBaseMenu sceneToChangeTo;

	public MenuChangeItem(string text, IBaseMenu newMenu) : base(text, EntryType.Link, false) {
		this.text = text;

		sceneToChangeTo = newMenu;

		OnClick = () => MenuScene.instance.ChangeScene(sceneToChangeTo, klackerOnlyChanges);
	}
}
public class SwitchItem : MenuItem {
	public IBaseMenu sceneToChangeTo;

	public SwitchItem(string onText, string offText, Func<bool> getValue, Action<bool> setValue) : base(onText, EntryType.Link, false) {

		text = getValue() ? onText : offText;
		OnClick = () =>
		{
			setValue(!getValue());

			text = getValue() ? onText : offText;

			MenuScene.instance.ChangeScene(MenuScene.instance.currentScene, true);
		};
	}
	public SwitchItem(string onText, string offText, SettingsValue<bool> setting) : this(onText, offText, setting.GetValue, setting.SetValue) { }
}

public interface IBaseMenu {
	List<MenuItem> GetMenuItems();
}
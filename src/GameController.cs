using Godot;
using System;
using System.Threading.Tasks;

public class GameController : Node2D {
	public static GameController instance;

	// static GameController() {
	// 	void HandleTaskException(object e) {
	// 		Task t = e as Task;
	//
	// 		GD.Print($"Unhandled exception in task!\n{t.Exception.InnerException.Message}\n{t.Exception.InnerException.StackTrace}");
	// 	}
	// 	TaskScheduler.UnobservedTaskException += (o, e) => HandleTaskException(o);
	// }

	public static int currentPlayerID = 2;
	public static string CurrentPlayerTag {
		get {
			return "P" + currentPlayerID;
		}
	}

	public void SetTaskbar(bool toggle) {
		taskbar.SetVisible(toggle);
	}

	[Export]
	public NodePath _taskbar;
	public Control taskbar;

	public static string[] playerCompanyNames = { "Sunshine Airways", "Falcon Lines", "Phönix Travel", "Honey Airlines" };
	public static string[] playerNames = { "Tina Cortez", "Siggi Sorglos", "Igor Tuppolevsky", "Mario Zucchero" };

	public static Action onUnhandledInput;
	public static bool canPlayerInteract = true;

	public static Random r = new Random();

	public static bool fastForward = false;

	public override void _Ready() {
		instance = this;
		taskbar = GetNode<Control>(_taskbar);

		RoomManager.ChangeRoom("RoomMainMenu", isAirport: false);
		GetTree().Connect("screen_resized", this, "OnScreenSizeChanged");
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (@event is InputEventKey k) {
			fastForward = false;
			if (k.Scancode == (int)KeyList.Space) {
				//onUnhandledInput?.Invoke();
				fastForward = k.Pressed;
			}
		}
		if (@event is InputEventMouseButton m) {
			OnMouseClick(m);
		}
	}

	public static void OnMouseClick(InputEventMouseButton m) {
		if (m.IsPressed() && m.ButtonIndex == (int)ButtonList.Left) {
			onUnhandledInput?.Invoke();
		}
	}

	override public void _Process(float delta) {
		TimeScale = fastForward ? 20:1;

	}

	public static int TimeScale { get; set; }

	public void OnScreenSizeChanged() {
		//GetViewport().SetSizeOverride(true, new Vector2(OS.GetWindowSize().x, GetViewportRect().Size.y));
		// Vector2 screenSize = OS.GetWindowSize();
		// Viewport viewport = GetViewport();

		// float scaleX = Mathf.Floor(screenSize.x / viewport.Size.x);
		// float scaleY = Mathf.Floor(screenSize.y / viewport.Size.y);

		// float scale = Mathf.Max(1, Mathf.Min(scaleX, scaleY));

		// Vector2 diffHalf = ((screenSize - (viewport.Size * scale)) / 2).Floor();

		// viewport.SetAttachToScreenRect(new Rect2(diffHalf, viewport.Size * scale));
	}
}

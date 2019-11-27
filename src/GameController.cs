using Godot;
using System;
using System.Threading.Tasks;

public class GameController : Node2D {
	public static GameController instance;

	static GameController() {
		void HandleTaskException(object e) {
			Task t = e as Task;

			GD.Print($"Unhandled exception in task!\n{t.Exception.InnerException.Message}\n{t.Exception.InnerException.StackTrace}");
		}
		TaskScheduler.UnobservedTaskException += (o, e) => HandleTaskException(o);
	}

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

	public override void _Ready() {
		instance = this;
		taskbar = GetNode<Control>(_taskbar);

		RoomManager.ChangeRoom("RoomMainMenu", isAirport: false);
	}
}

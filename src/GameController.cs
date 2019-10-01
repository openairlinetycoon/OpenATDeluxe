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

	public static int playerID = 2;

	public void SetTaskbar(bool toggle) {
		taskbar.SetVisible(toggle);
	}

	[Export]
	public NodePath _taskbar;
	public Control taskbar;

	public override void _Ready() {
		instance = this;
		taskbar = GetNode<Control>(_taskbar);

		RoomManager.ChangeRoom("RoomMainMenu", isAirport: false);
	}
}

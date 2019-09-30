using Godot;
using System;

public class GameController : Node2D {
	public static GameController instance;

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

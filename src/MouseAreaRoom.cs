using Godot;
using System;

public class MouseAreaRoom : MouseArea {
	[Export]
	public string roomSceneName;

	public override void OnClick() {
		//Change Room!
		RoomManager.ChangeRoom(roomSceneName, isExitToAirport);
	}
}
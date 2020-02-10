using Godot;
using System;

public class MouseAreaRoom : MouseArea {
	[Export]
	public string roomSceneName = "";

	[Export]
	public Vector2 entranceOffset;
	[Export]
	public bool isStandardDoor;

	override public int Layer => (int)BaseLayer.MouseAreaRoom;
	public override void _Ready() {
		base._Ready();

		entranceOffset = isStandardDoor ? new Vector2(0, 11.5f) : entranceOffset;
	}
	public override void OnClick() {
		//Change Room! -- No longer!
		//RoomManager.ChangeRoom(roomSceneName, isExitToAirport);
		if (GameController.canPlayerInteract == false)
			return;

		if (RoomManager.currentRoom == "RoomAirport") {

			PlayerCharacter.instance.SetPath(ToGlobal(entranceOffset));
			Action<BaseCharacter> changeRoom = null;
			changeRoom = (c) => {
				PlayerCharacter.instance.OnGoalReached -= changeRoom;

				RoomManager.roomPosition = ToGlobal(entranceOffset);
				RoomManager.ChangeRoom(roomSceneName, isExitToAirport);
			};

			PlayerCharacter.instance.OnGoalReached += changeRoom;
		} else {
			RoomManager.ChangeRoom(roomSceneName, isExitToAirport);
		}
	}
}
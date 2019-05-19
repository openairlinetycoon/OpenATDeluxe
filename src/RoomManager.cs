using Godot;
using System;
using System.Diagnostics;

public class RoomManager : Node {
	public static RoomManager instance;
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		instance = this;
	}

	public static void ChangeRoom(string newRoomName, bool isAirport) {
		if (isAirport)
			newRoomName = "RoomAirport";
		string fullPath = "res://scenes/rooms/" + newRoomName + ".tscn";
		File f = new File();
		Debug.Assert(f.FileExists(fullPath), "Room not found! Room: " + fullPath);

		MouseCursor.instance.Reset();

		//Remove the current room!
		foreach (Node child in instance.GetChildren()) {
			child.SetOwner(null);
			instance.RemoveChild(child);
		}

		Node n = ResourceLoader.Load<PackedScene>(fullPath).Instance();

		instance.AddChild(n);
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
}

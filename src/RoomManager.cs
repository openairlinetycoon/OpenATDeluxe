using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class RoomManager : Node2D {
	public static RoomManager instance;
	public static string currentRoom;
	public static Node2D currentRoomNode;

	private static Vector2 roomPosition;

	[Export] //--FIXME: It should be automated!
	public Godot.Collections.Array _rooms;
	private static Dictionary<string, PackedScene> rooms;

	public override void _Ready() {
		roomPosition = new Vector2(4448, 0);


		rooms = new Dictionary<string, PackedScene>();
		foreach (var entry in _rooms) {
			PackedScene scene = (PackedScene)entry;
			string file = System.IO.Path.GetFileNameWithoutExtension(scene.ResourcePath);
			rooms.Add(file, scene);
		}
		//LoadRooms(); --FIXME: godot can't find the "res://scenes/rooms/" folder reliably

		instance = this;
	}


	/// <summary>
	/// Load every Room in preparation
	/// </summary>
	private static void LoadRooms() {
		Directory d = new Directory();
		Error error = d.Open("res://scenes/rooms");

		Debug.Assert(error == Error.Ok, "Error opening the rooms folder! Error: " + error);

		d.ListDirBegin(true, true);
		string file;

		while ((file = d.GetNext()) != "") {
			rooms.Add(System.IO.Path.GetFileNameWithoutExtension(file), ResourceLoader.Load<PackedScene>("res://scenes/rooms/" + file));
		}
		d.ListDirEnd();
	}

	public static void ChangeRoom(string newRoomName, bool isAirport) {
		if (!isAirport) {
			CameraController cam = GetCameraControllerInCurrentRoom();
			if (cam != null)
				roomPosition = cam.GetPosition();
		}

		if (isAirport) {
			newRoomName = "RoomAirport";
		}


		//string fullPath = "res://scenes/rooms/" + newRoomName + ".tscn";
		//File f = new File();
		Debug.Assert(rooms.ContainsKey(newRoomName), "Room not found! Room: " + newRoomName);

		if (MouseCursor.instance != null)
			MouseCursor.instance.Reset();

		//Remove the current room!
		foreach (Node2D child in instance.GetChildren()) {
			child.QueueFree();
		}

		Node2D n = (Node2D)rooms[newRoomName].Instance();
		instance.AddChild(n);

		currentRoom = newRoomName;
		currentRoomNode = n;

		if (isAirport) {
			GetCameraControllerInCurrentRoom()?.SetPosition(roomPosition);
		}

		//instance.AddChild(n);
	}

	private static CameraController GetCameraControllerInCurrentRoom() {
		if (currentRoomNode == null)
			return null;

		CameraController cam;
		foreach (Node child in currentRoomNode.GetChildren()) {
			cam = child as CameraController;
			if (cam != null)
				return cam;
		}

		return null;
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
}

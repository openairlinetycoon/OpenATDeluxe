using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class RoomManager : Node2D {
	public static RoomManager instance;
	public static string currentRoom;
	public static Node2D currentRoomNode;

	public static Vector2 roomPosition;

	[Export] //--FIXME: It should be automated!
	public Godot.Collections.Array _rooms;
	private static Dictionary<string, PackedScene> rooms;

	private static List<string> visitedRooms = new List<string>();

	public static Action OnRoomExit;

	public override void _Ready() {
		roomPosition = new Vector2(4330, 880);


		rooms = new Dictionary<string, PackedScene>();
		foreach (var entry in _rooms) {
			PackedScene scene = (PackedScene)entry;
			string file = System.IO.Path.GetFileNameWithoutExtension(scene.ResourcePath);
			rooms.Add(file, scene);
		}
		//LoadRooms(); --FIXME: godot can't find the "res://scenes/rooms/" folder reliably

		instance = this;

		OnRoomExit += () => InteractionLayerManager.ResetLayers();
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
		// if (!isAirport) {
		// 	CameraController cam = GetCameraControllerInCurrentRoom();
		// 	if (cam != null)
		// 		roomPosition = cam.GetPosition();
		// }
		if (isAirport) {
			newRoomName = "RoomAirport";
		}

		OnRoomExit?.Invoke();

		if (MouseCursor.instance != null)
			MouseCursor.instance.Reset();

		//Remove the current room!
		foreach (Node2D child in instance.GetChildren()) {
			child.QueueFree();
		}

		currentRoom = newRoomName;
		
		Node2D n = GetRoomInstance(newRoomName);
		instance.AddChild(n);
		
		currentRoomNode = n;
		
		if (!visitedRooms.Contains(newRoomName)) {
			visitedRooms.Add(newRoomName);
		}

		if (isAirport) {
			GetCameraControllerInCurrentRoom()?.SetPosition(roomPosition);
			PlayerCharacter.instance.SetPosition(roomPosition);
		}
	}

	public static Node2D GetRoomInstance(string newRoomName) {
		//string fullPath = "res://scenes/rooms/" + newRoomName + ".tscn";
		//File f = new File();
		Debug.Assert(rooms.ContainsKey(newRoomName), "Room not found! Room: " + newRoomName);

		Node2D newRoom = (Node2D)rooms[newRoomName].Instance();
		return newRoom;
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

	public static bool WasRoomVisited(string room) {
		return visitedRooms.Contains(room);
	}
}

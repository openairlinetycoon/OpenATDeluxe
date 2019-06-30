using Godot;
using System.Linq;
using System.Collections.Generic;
using System;

public class MouseCursor : Node2D {
	public static MouseCursor instance;

	GFXLibrary lib;

	List<Node2D> states = new List<Node2D>();

	MouseState currentState;
	MouseArea currentHover; //The "object" we are currently hovering above

	public enum MouseState : int {
		Normal = 0,
		Hover,
		Load,
		Exit,
		MoveLeft,
		MoveRight,
	}

	public int movingCamera = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		instance = this;

		// lib = new GFXLibrary(GFXLibrary.pathToAirlineTycoonD + "/gli/glbasis.gli");
		// lib.GetFilesInLibrary();
		states.Add(GetNode<Node2D>("Normal"));
		states.Add(GetNode<Node2D>("Hover"));
		states.Add(GetNode<Node2D>("Load"));
		states.Add(GetNode<Node2D>("Exit"));
		states.Add(GetNode<Node2D>("MoveLeft"));
		states.Add(GetNode<Node2D>("MoveRight"));

		ChangeMouseState(MouseState.Normal);
		// if (lib.files.Count > 0) {
		//     SetTexture (lib.files[0].GetTexture());
		// }
		Input.SetMouseMode(Input.MouseMode.Hidden);
	}

	public void MouseEnter(Area2D other) {
		MouseArea area = other as MouseArea;
		if (area != null) {
			currentHover = area;
			ChangeMouseState(area.isExitToAirport ? MouseState.Exit : MouseState.Hover);
		} else {
			ChangeMouseState(MouseState.Hover);
		}
	}

	public void MouseLeave(Area2D other) {
		MouseArea area = other as MouseArea;
		if (area != null && area == currentHover) {
			currentHover = null;
		}

		ChangeMouseState(MouseState.Normal);
	}


	public void ChangeMouseState(MouseState toState) {
		currentState = toState;

		states.ForEach((s) => s.SetVisible(false));

		//Moving overrides other modes!
		if (movingCamera == 0) {
			states[(int)toState].SetVisible(true);
		} else {
			MouseState moveState = movingCamera == 1 ? MouseState.MoveRight : MouseState.MoveLeft;

			states[(int)moveState].SetVisible(true);
		}
	}

	int currentTexture = 0;

	public override void _UnhandledInput(InputEvent e) {
		if (e is InputEventMouseButton) {
			InputEventMouseButton mouse = e as InputEventMouseButton;

			if (mouse.IsPressed()) {
				if (currentHover != null && movingCamera == 0) {
					currentHover.OnClick();
				} else if (PlayerCharacter.instance != null && RoomManager.currentRoom == "RoomAirport") {
					//SET MOVING WAYPOINT
					PlayerCharacter.instance.SetPath(CameraController.airportCamera.GetGlobalMousePosition());
				}
				//         if (currentTexture < lib.filesInLibrary) {

				//             SetTexture (lib.files[currentTexture++].GetTexture());
				//         } else {
				//             currentTexture = 0;
				//         }
			}
		}
	}

	public void Reset() {
		movingCamera = 0;
		currentState = MouseState.Normal;
		currentHover = null;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta) {
		SetPosition(GetGlobalMousePosition());

		ChangeMouseState(currentState);
	}
}

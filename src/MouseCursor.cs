using Godot;
using System.Linq;
using System.Collections.Generic;
using System;

public class MouseCursor : Node2D {
	public static MouseCursor instance;

	GFXLibrary lib;

	List<Node2D> states = new List<Node2D>();

	MouseState currentState;
	Node currentHover; //The "object" we are currently hovering above
	List<HoverStackItem> hovers = new List<HoverStackItem>();

	public struct HoverStackItem {
		public MouseState state;
		public Node item;

		public HoverStackItem(MouseState state, Node item) {
			this.state = state;
			this.item = item;
		}
	}

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

	public void MouseEnter(Node other) {
		MouseState state = MouseState.Hover;
		if (other is MouseArea area) {
			ChangeMouseState(state = (area.isExitToAirport ? MouseState.Exit : MouseState.Hover));
		} else {
			ChangeMouseState(MouseState.Hover);
		}

		if (other != null) {
			currentHover = other;
			hovers.Add(new HoverStackItem(state, other));
		}
	}

	public void MouseLeave(Node other) {
		if (other is MouseArea area && area == currentHover) {
		}

		if (other != null)
			hovers.Remove(hovers.Find((o) => o.item == other));
		List<HoverStackItem> marked = new List<HoverStackItem>();
		foreach (HoverStackItem i in hovers) {
			if (IsInstanceValid(i.item) != true)
				marked.Add(i);
		}
		foreach (HoverStackItem i in marked) {
			hovers.Remove(i);
		}
		HoverStackItem next = hovers.LastOrDefault(); ;
		currentHover = next.item;

		ChangeMouseState(next.state);
	}

	public int GetIndexRecursive(Node n, bool withZIndexAdded = false) {
		int index = 0;
		Node current = n;
		Node root = GetTree().Root;
		while (current != root) {
			index += current.GetIndex();
			index += withZIndexAdded ? (int)(current.HasMethod("GetZIndex") ? current.Call("GetZIndex") : 0) : 0;

			current = current.GetParent();
		}

		return index;
	}


	public void ChangeMouseState(MouseState toState) {
		currentState = toState;

		if (GameController.canPlayerInteract == false) {
			switch (toState) {
				case MouseState.Exit:
				case MouseState.MoveLeft:
				case MouseState.MoveRight:
					toState = MouseState.Normal;
					break;
			}
		}

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

	public override void _Input(InputEvent e) {

		if (currentHover != null) {
			//GD.Print(currentHover.Name);
		}
		if (e is InputEventMouseButton mouse) {
			GameController.OnMouseClick(mouse);

			if (mouse.IsPressed()) {
				bool handled = false;
				if (currentHover != null && movingCamera == 0 && IsInstanceValid(currentHover)) {
					currentHover.Call("OnClick");
					handled = true;
				} else if (PlayerCharacter.instance != null && RoomManager.currentRoom == "RoomAirport") {
					//SET MOVING WAYPOINT
					PlayerCharacter.instance.SetPath(CameraController.airportCamera.GetGlobalMousePosition());
					handled = true;
				}
				//         if (currentTexture < lib.filesInLibrary) {

				//             SetTexture (lib.files[currentTexture++].GetTexture());
				//         } else {
				//             currentTexture = 0;
				//         }
				if (handled)
					GetTree().SetInputAsHandled();
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

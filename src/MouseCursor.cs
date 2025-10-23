using Godot;
using System.Linq;
using System.Collections.Generic;
using System;

public partial class MouseCursor : Node2D {
	public static MouseCursor instance;

	List<Node2D> states = new List<Node2D>();
	AnimatedSprite2D exitAnimatedSprite;

	MouseState currentState;
	HoverStackItem currentHover; //The "object" we are currently hovering above
	List<HoverStackItem> hovers = new List<HoverStackItem>();

	public struct HoverStackItem {
		public MouseState state;
		public Node item;

		public ulong additionTime;

		public HoverStackItem(MouseState state, Node item) {
			this.state = state;
			this.item = item;

			this.additionTime = Time.GetTicksMsec();
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

	// Double-tap detection variables
	private ulong lastClickTime = 0;
	private const ulong doubleTapThreshold = 300; // milliseconds

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

		// Get reference to Exit AnimatedSprite2D
		exitAnimatedSprite = GetNode<AnimatedSprite2D>("Exit");

		ChangeMouseState(MouseState.Normal);
		// if (lib.files.Count > 0) {
		//     SetTexture (lib.files[0].GetTexture());
		// }
		Input.MouseMode = Input.MouseModeEnum.Hidden;
	}

	public void MouseEnter(Node other) {
		MouseState state = MouseState.Hover;
		if (other is MouseArea area) {
			state = (area.isExitToAirport ? MouseState.Exit : MouseState.Hover);
		} else {
			state = MouseState.Hover;//ChangeMouseState();
		}

		if (other != null) {
			HoverStackItem item = new HoverStackItem(state, other);
			hovers.Add(item);
			hovers = hovers.OrderByDescending((stackItem) => {
				if (stackItem.item is IInteractionLayer l)
					return l.Layer;
				return 0;
			}).ThenByDescending((stackItem) => stackItem.additionTime).ToList();

			int index = hovers.IndexOf(item);
			GD.Print($"MouseEnter new index of: {index} and it has a layer of {(other is IInteractionLayer layer ? layer.Layer : 0)}");

			if (index != 0)
				return;

			currentHover = item;
		}


		ChangeMouseState(state);
	}

	public void MouseLeave(Node other) {

		if (other != null)
			hovers.Remove(hovers.Find((o) => o.item == other));
		CleanHovers();

		ChangeMouseState(currentHover.state);
	}

	private void CleanHovers() {
		List<HoverStackItem> marked = new List<HoverStackItem>();
		foreach (HoverStackItem i in hovers) {
			if (IsInstanceValid(i.item) != true)
				marked.Add(i);
		}
		foreach (HoverStackItem i in marked) {
			hovers.Remove(i);
		}

		HoverStackItem next = FindNextHoverableOrDefault();
		currentHover = next;
	}

	public HoverStackItem FindNextHoverableOrDefault() {
		foreach (HoverStackItem h in hovers) {
			if (!InteractionLayerManager.IsLayerDisabled((h.item is IInteractionLayer l ? l.Layer : 0))) {
				return h;
			}
		}

		return default(HoverStackItem);
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


	public void ChangeMouseState(MouseState toState, bool toCurrentHover = true) {
		if (toCurrentHover && currentHover.item is IInteractionLayer l) {
			if (InteractionLayerManager.IsLayerDisabled(l)) {
				return;
			}
		}

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

		states.ForEach((s) => s.Visible = false);

		//Moving overrides other modes!
		if (movingCamera == 0) {
			states[(int)toState].Visible = true;
			
			// Play animation for Exit state
			if (toState == MouseState.Exit && exitAnimatedSprite != null) {
				exitAnimatedSprite.Play("default");
			} else if (exitAnimatedSprite != null) {
				exitAnimatedSprite.Stop();
			}
		} else {
			MouseState moveState = movingCamera == 1 ? MouseState.MoveRight : MouseState.MoveLeft;

			states[(int)moveState].Visible = true;
			
			// Stop exit animation if we're moving camera
			if (exitAnimatedSprite != null) {
				exitAnimatedSprite.Stop();
			}
		}
	}

	int currentTexture = 0;

	public override void _Input(InputEvent e) {

		if (currentHover.item != null) {
			//GD.Print(currentHover.Name);
		}
		if (e is InputEventMouseButton mouse) {
			//GameController.OnMouseClick(mouse);

			if (mouse.IsPressed() && mouse.ButtonIndex == MouseButton.Left) {
				bool handled = false;
				
				// Check for double-tap
				ulong currentTime = Time.GetTicksMsec();
				bool isDoubleTap = (currentTime - lastClickTime) < doubleTapThreshold;
				lastClickTime = currentTime;

				if (currentHover.item != null && movingCamera == 0 && IsInstanceValid(currentHover.item)) {
					if (currentHover.item is IInteractionLayer l) {
						if (InteractionLayerManager.IsLayerDisabled(l))
							return;

						GD.Print($"Layer: {l.Layer}");
					}
					currentHover.item.Call("OnClick");
					handled = true;
				} else if (PlayerCharacter.instance != null && RoomManager.currentRoom == "RoomAirport" && movingCamera == 0) {
					//SET MOVING WAYPOINT
					Vector2 targetPosition = CameraController.airportCamera.GetGlobalMousePosition();
					PlayerCharacter.instance.SetPath(targetPosition, isDoubleTap);
					handled = true;
				}

				if (handled)
					GetViewport().SetInputAsHandled();
			}
		}
	}

	public void Reset() {
		movingCamera = 0;
		currentState = MouseState.Normal;
		currentHover = default(HoverStackItem);
		hovers.Clear();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		Position = GetGlobalMousePosition();

		CleanHovers();

		ChangeMouseState(currentHover.item == null ? MouseState.Normal : currentHover.state);
	}
}

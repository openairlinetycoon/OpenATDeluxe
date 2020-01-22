using Godot;
using System;

public class MouseArea : Area2D {
	[Export]
	public bool isExitToAirport = false;

	[Export]
	public bool ignoreInteractionLock = false;

	public Action onClick;
	public CollisionShape2D area;

	public override void _Ready() {
		if (GetChildCount() != 0)
			area = (CollisionShape2D)GetChild(0);

		if (area == null) {
			area = new CollisionShape2D();
			area.Shape = new RectangleShape2D();
			AddChild(area);
		}

		Connect("mouse_entered", this, nameof(MouseEntered));
		Connect("mouse_exited", this, nameof(MouseExited));
	}

	public void MouseEntered() {
		MouseCursor.instance?.MouseEnter(this);
	}
	public void MouseExited() {
		MouseCursor.instance?.MouseLeave(this);
	}


	public virtual void OnClick() {
		if (GameController.canPlayerInteract || ignoreInteractionLock)
			onClick?.Invoke();
	}
}
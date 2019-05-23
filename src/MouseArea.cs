using Godot;
using System;

public class MouseArea : Area2D {
	[Export]
	public bool isExitToAirport = false;

	public override void _Ready() {
		Connect("mouse_entered", this, nameof(MouseEntered));
		Connect("mouse_exited", this, nameof(MouseExited));
	}

	public void MouseEntered() {
		MouseCursor.instance.MouseEnter(this);
	}
	public void MouseExited() {
		MouseCursor.instance.MouseLeave(this);
	}


	public virtual void OnClick() {

	}
}
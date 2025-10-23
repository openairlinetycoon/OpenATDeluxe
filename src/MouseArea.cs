using Godot;
using System;

public partial class MouseArea : Area2D, IInteractionLayer {
	[Export]
	public bool isExitToAirport = false;

	[Export]
	public bool ignoreInteractionLock = false;

	public Action onClick;
	public CollisionShape2D area;

	public virtual int Layer => (int)BaseLayer.MouseArea;

	public override void _Ready() {
		// Enable input pickable so the Area2D can detect mouse events
		InputPickable = true;
		
		if (GetChildCount() != 0)
			area = (CollisionShape2D)GetChild(0);

		if (area == null) {
			area = new CollisionShape2D();
			area.Shape = new RectangleShape2D();
			AddChild(area);
		}
		
		Connect("mouse_entered", new Callable(this, nameof(MouseEntered)));
		Connect("mouse_exited", new Callable(this, nameof(MouseExited)));
	}

	public new void MouseEntered() {
		MouseCursor.instance?.MouseEnter(this);
	}
	public new void MouseExited() {
		MouseCursor.instance?.MouseLeave(this);
	}


	public virtual void OnClick() {
		if (GameController.canPlayerInteract || ignoreInteractionLock)
			onClick?.Invoke();
	}
}
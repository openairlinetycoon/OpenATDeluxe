using Godot;

public class CharacterItem : Control {
	private MenuItem assignedMenuItem;

	public MenuItem AssignedMenuItem {
		get => assignedMenuItem;
		set {
			assignedMenuItem = value;
			if (value?.OnClick != null) {
				Connect("mouse_entered", this, nameof(MouseEntered));
				Connect("mouse_exited", this, nameof(MouseExited));
			}
		}
	}

	public void MouseEntered() {
		MouseCursor.instance.MouseEnter(null);
	}
	public void MouseExited() {
		MouseCursor.instance.MouseLeave(null);
	}


	public override void _GuiInput(InputEvent e) {
		InputEventMouseButton mouse = e as InputEventMouseButton;

		if (mouse != null && AssignedMenuItem != null) {
			if (mouse.Pressed && mouse.ButtonIndex == (int)ButtonList.Left)
				AssignedMenuItem.OnClick?.Invoke();
		}
	}
}

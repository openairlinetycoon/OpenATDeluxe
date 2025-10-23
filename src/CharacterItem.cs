using Godot;

public partial class CharacterItem : Control {
	private MenuItem assignedMenuItem;
	public string character = "";
	public int stringPosition = 0;

	public MenuItem AssignedMenuItem {
		get => assignedMenuItem;
		set {
			assignedMenuItem = value;
			if (value?.OnClick != null || value?.OnClickSpecial != null) {
				Connect("mouse_entered", new Callable(this, nameof(MouseEntered)));
				Connect("mouse_exited", new Callable(this, nameof(MouseExited)));
			}
		}
	}

	public void MouseEntered() {
		MouseCursor.instance.MouseEnter(this);
	}
	public void MouseExited() {
		MouseCursor.instance.MouseLeave(this);
	}


	public void OnClick() {
		if (AssignedMenuItem != null) {
				AssignedMenuItem?.OnClick?.Invoke();
				AssignedMenuItem?.OnClickSpecial?.Invoke(stringPosition);
		}
	}
}

using System;
using Godot;

public partial class LineElement : Control, IInteractionLayer {
	public Action onClick;
	public Action onMouseEnter, onMouseLeave;

	public override void _Ready() {
		Connect("mouse_entered", new Callable(this, nameof(MouseEntered)));
		Connect("mouse_exited", new Callable(this, nameof(MouseExited)));
	}

	public void MouseEntered() {
		MouseCursor.instance?.MouseEnter(this);

		SoundPlayer p = SoundPlayer.CreatePlayer("/SOUND/change.raw", "effects", true, true);
		p.VolumeDb = 12;
		AddChild(p);
		p.Play();

		onMouseEnter?.Invoke();
	}
	public void MouseExited() {
		MouseCursor.instance?.MouseLeave(this);

		onMouseLeave?.Invoke();
	}
	public void OnClick() {
		MouseExited();
		onClick?.Invoke();
	}

	override public void _GuiInput(InputEvent e) {
		if (e is InputEventMouseButton mouse) {
			if (mouse.IsPressed()) {
				OnClick();
			}
		}
	}

	public int Layer => (int)BaseLayer.Elements;
}

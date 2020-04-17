using Godot;

public abstract class BaseElement : Node2D {
	public abstract void OnReady();
	public Texture texture;

	public sealed override void _Ready() {
		OnReady();
	}
}
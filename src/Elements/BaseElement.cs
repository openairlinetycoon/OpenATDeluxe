using Godot;

public abstract partial class BaseElement : Node2D {
	public abstract void OnReady();
	public Texture2D texture;

	public sealed override void _Ready() {
		OnReady();
	}
}
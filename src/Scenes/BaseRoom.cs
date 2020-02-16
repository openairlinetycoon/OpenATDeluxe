using Godot;
using System;
using System.Collections.Generic;

public abstract class BaseRoom : Node2D {
	public Control baseNode;

	public sealed override void _Ready() {
		AddToGroup("cancelable");

		baseNode = (Control)GetChild(0);

		OnReady();
	}

	override public void _ExitTree() {
		DialogueSystem.CleanActors();
	}

	/// <summary>
	/// Substitute for a forced call to base._Ready() 
	/// </summary>
	public abstract void OnReady();
	public abstract void Cancel();
}
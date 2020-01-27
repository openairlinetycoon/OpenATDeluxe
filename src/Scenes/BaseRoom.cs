using Godot;
using System;
using System.Collections.Generic;

public abstract class BaseRoom : Node2D {
	public static bool wasVisited = false;

	public Control baseNode;
	public sealed override void _Ready() {
		AddToGroup("cancelable");

		baseNode = (Control)GetChild(0);
		wasVisited = true;

		OnReady();
	}

	/// <summary>
	/// Substitute for a forced call to base._Ready() 
	/// </summary>
	public abstract void OnReady();
	public abstract void Cancel();
}
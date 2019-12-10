using Godot;
using System;
using System.Collections.Generic;

public class BaseRoom : Node2D {
	public Control baseNode;
	public override void _Ready() {
		AddToGroup("cancelable");

		baseNode = (Control)GetChild(0);
	}

	public virtual void Cancel() {

	}
}
using Godot;
using System;
using System.Collections;

[Tool]
public partial class AutoSize : NinePatchRect {

	override public void _Process(double _dt) {
		PivotOffset = Size / 2;
	}
	override public void _Draw() {
		// _Process(0);
		// base._Draw();
	}
}
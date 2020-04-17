using Godot;
using System;
using System.Collections;

[Tool]
public class AutoSize : NinePatchRect {

	override public void _Process(float _dt) {
		RectPivotOffset = RectSize / 2;
	}
	override public void _Draw() {
		// _Process(0);
		// base._Draw();
	}
}
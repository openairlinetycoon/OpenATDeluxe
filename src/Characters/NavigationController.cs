using Godot;
using System;

public class NavigationController : Navigation2D {
	public static NavigationController instance;

	public override void _Ready() {
		instance = this;
	}
}

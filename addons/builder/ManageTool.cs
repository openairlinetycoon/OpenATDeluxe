using Godot;
using System;
using System.Diagnostics;

[Tool]
public class ManageTool : EditorPlugin {
	static ManageTool instance;
	static Control dock;

	public ManageTool() {
		instance = this;
	}

	public override void _EnterTree() {
		dock = (Control)GD.Load<PackedScene>("addons/builder/dock.tscn").Instance();
		AddControlToDock(DockSlot.LeftUr, dock);
	}

	public static void Reload(Control oldDock) {
		instance.RemoveControlFromDocks(oldDock);
		oldDock.QueueFree();

		dock = (Control)GD.Load<PackedScene>("addons/builder/dock.tscn").Instance();
		instance.AddControlToDock(DockSlot.LeftUr, dock);
	}

	public override void _ExitTree() {
		RemoveControlFromDocks(dock);
		dock.QueueFree();
		// Initialization of the plugin goes here
	}
}


using Godot;
using System;
using System.Diagnostics;

[Tool]
public partial class ManageTool : EditorPlugin {
	static ManageTool instance;
	static DockInterface dock;

	public ManageTool() {
		instance = this;
	}

	public override void _EnterTree() {
		dock = (DockInterface)GD.Load<PackedScene>("addons/builder/dock.tscn").Instantiate();
		dock.isDocked = true;
		AddControlToDock(DockSlot.LeftUr, dock);
	}

	public static void Reload(Control oldDock) {
		instance.RemoveControlFromDocks(oldDock);
		oldDock.QueueFree();

		dock = (DockInterface)GD.Load<PackedScene>("addons/builder/dock.tscn").Instantiate();
		dock.isDocked = true;
		instance.AddControlToDock(DockSlot.LeftUr, dock);
	}

	public override void _ExitTree() {
		RemoveControlFromDocks(dock);
		dock.QueueFree();
		// Initialization of the plugin goes here
	}
}

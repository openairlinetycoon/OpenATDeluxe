using Godot;
using System;
using System.Collections.Generic;

public partial class NavigationController : Node2D {
	public static NavigationController instance;
	
	private NavigationRegion2D navigationRegion;

	public override void _Ready() {
		instance = this;
		
		// Try to find NavigationRegion2D in the scene tree
		navigationRegion = GetNodeOrNull<NavigationRegion2D>("NavigationRegion2D");
		if (navigationRegion == null) {
			// Search recursively in all descendants
			navigationRegion = FindNavigationRegion(this);
		}
		
		if (navigationRegion != null) {
			GD.Print("NavigationRegion2D found successfully");
		} else {
			GD.PushWarning("NavigationRegion2D not found in scene tree");
		}
	}
	
	private NavigationRegion2D FindNavigationRegion(Node node) {
		foreach (Node child in node.GetChildren()) {
			if (child is NavigationRegion2D nav) {
				return nav;
			}
			// Recursively search in children
			NavigationRegion2D found = FindNavigationRegion(child);
			if (found != null) {
				return found;
			}
		}
		return null;
	}
	
	public Vector2[] GetSimplePath(Vector2 start, Vector2 end, bool optimize = true) {
		if (navigationRegion == null) {
			GD.PushWarning("NavigationRegion2D not found, returning direct path");
			return new Vector2[] { start, end };
		}
		
		// Use NavigationServer2D to get the path
		var rid = navigationRegion.GetNavigationMap();
		var path = NavigationServer2D.MapGetPath(rid, start, end, optimize);
		
		return path;
	}
}

using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Container for airport relevant objects like escelators or room entrances
/// </summary>
public class AirportViewController : Node2D {
	public static AirportViewController instance;

	[Export]
	Godot.Collections.Array _escelators;
	public Node[] escelators;
	[Export]
	Godot.Collections.Array _roomEntrances;
	public Node[] roomEntrances;

	override public void _Ready() {
		instance = this;

		escelators = (from e in _escelators select GetNode((NodePath)e)).ToArray();
		roomEntrances = (from e in _escelators select GetNode((NodePath)e)).ToArray();
	}

}

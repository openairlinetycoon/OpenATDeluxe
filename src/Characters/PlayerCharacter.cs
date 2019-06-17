using Godot;
using System;

public class PlayerCharacter : BaseCharacter {
	public static PlayerCharacter instance;

	[Export]
	public int playerID;

	override public void _Ready() {
		type = 20 + playerID;
		base._Ready();

		instance = this;
	}

}
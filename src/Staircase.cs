using Godot;
using System;
using System.Collections.Generic;

public class Staircase : Node2D {
	[Export]
	public bool isUpstairs;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		//Connect("body_entered", this, "BodyEntered");
	}

	public void TriggerAnimation(BaseCharacter character) {
		//Character entered the zone

		Queue<Vector2> originalPath = new Queue<Vector2>(character.path);
		character.path.Clear();
		character.path.Enqueue(new Vector2(0, -30) + GetGlobalPosition());
		character.path.Enqueue(new Vector2(52, -54) + GetGlobalPosition());
		character.shiftsFloor = false;
		character.isInAnimation = true;
		int oldZ = character.ZIndex;
		character.ZIndex = isUpstairs ? 19 : 59;

		var buffer = character.OnGoalReached;

		Action<BaseCharacter> teleport = null;
		teleport = (c) => {
			c.OnPathFinished -= teleport;

			if (isUpstairs) {
				c.Position = new Vector2(12, -260) + GetGlobalPosition();
			} else {
				c.Position = new Vector2(12, 260) + GetGlobalPosition();
			}
			c.ZIndex = oldZ;
			c.isInAnimation = false;
			c.SetPath(c.mainGoal);
			c.OnGoalReached = buffer;
		};

		character.OnPathFinished += teleport;

	}
	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
}

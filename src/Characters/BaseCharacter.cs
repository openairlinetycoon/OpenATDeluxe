using Godot;
using System;
using System.Collections.Generic;

public partial class BaseCharacter : AnimatedSprite2D {
	[Export]
	public int type;

	public const int SpeedWalking = 200, SpeedRunning = 400;
	public bool isInAnimation;

	List<Staircase> staircases;

	CharacterBody2D collider;

	public int dir = 1;
	public Queue<Vector2> path;
	public Vector2 mainGoal;
	public Vector2 currentGoal;
	public bool shiftsFloor; //Does the player goe from one height level to the other
	const int ShiftHeight = 410; //Guesstimation for the heigth

	public bool isRunning = false; // Track if character is running

	public Action<BaseCharacter> OnPathFinished, OnGoalReached;

	[Export]
	private bool drawDebug = false;

	[Export]
	public string name = "";
	public AnimationData data;
	override public void _Ready() {
		data = ClanCSVFile.instance.GetAnimationData(type, name);

		SpeedScale = data.speed;

		staircases = new List<Staircase>();

		GD.Print(GetTree().HasGroup("Staircases"));
		//Lets assume we only have a character in an Airport
		foreach (Node n in GetTree().GetNodesInGroup("Staircases")) {
			if (n is Staircase)
				staircases.Add((Staircase)n);
		}

	collider = GetNode<CharacterBody2D>("Collider");

	SpriteFrames = new SpriteFrames();
	foreach (var keyPair in data.textures) {
		SpriteFrames.AddAnimation(keyPair.Key.ToString());
		Animation = keyPair.Key.ToString();

		foreach (AnimationData.TextureRef texture in keyPair.Value) {
			SpriteFrames.AddFrame(keyPair.Key.ToString(), texture.GetTexture());
		}
	}
}	public void SetPath(Vector2 _goal, bool shouldRun = false) {
		if (isInAnimation)
			return;

	mainGoal = _goal;
	currentGoal = mainGoal;
	isRunning = shouldRun;

	QueueRedraw();

	bool isShiftingUp = Position.Y > ShiftHeight;


	shiftsFloor = mainGoal.Y > ShiftHeight & Position.Y < ShiftHeight | mainGoal.Y < ShiftHeight & Position.Y > ShiftHeight;		OnPathFinished = null; //They probably don't want to talk to us anymore
		OnGoalReached = null;

		Staircase nearestStaircase = GetNearestStaircase(isShiftingUp);

		if (shiftsFloor) {
			Action<BaseCharacter> teleport = null;
			teleport = (c) => {
				OnPathFinished -= teleport;

				nearestStaircase.TriggerAnimation(this);
			};

			OnPathFinished += teleport;
			currentGoal = nearestStaircase.GlobalPosition;
		}

		path = new Queue<Vector2>(
			NavigationController.instance.GetSimplePath(Position, shiftsFloor ? nearestStaircase.GlobalPosition : mainGoal, true));
	}

	public Staircase GetNearestStaircase(bool isShiftingUp) {
		float minDistance = Mathf.Inf;
		Staircase closest = null;
		Vector2 globalPos = GetGlobalPosition();//mainGoal;//GetGlobalPosition();
		foreach (Staircase s in staircases) {
			if (s.isUpstairs && !isShiftingUp)
				continue;

			float dist = globalPos.DistanceTo(s.GlobalPosition); //(s.GlobalPosition.x - mainGoal.x);//globalPos.DistanceTo(s.GlobalPosition);
			dist *= Mathf.Sign(dist);
			if (dist < minDistance) {
				minDistance = dist;
				closest = s;
			}
		}

		return closest;
	}

	override public void _Process(double delta) {
	delta *= GameController.TimeScale;
	SpeedScale = data.speed *GameController.TimeScale;
	
	QueueRedraw();
	if (path != null && path?.Count != 0) {
		float speed = isRunning ? SpeedRunning : SpeedWalking;
		MoveOnPath((float)(speed * delta));
	} else {
		Animation = ((AnimationState)((int)AnimationState.NStanding + dir)).ToString();
		Play();
		isRunning = false; // Reset running state when path is finished
	}
}	public void MoveOnPath(float distance) {
		Vector2 start = Position;

		for (int i = 0; i < path.Count; i++) {
			Vector2 nextPosition = path.Peek();
			float distanceToPoint = start.DistanceTo(nextPosition);

			Vector2 direction = start - nextPosition;
			SetViewDir(direction);

			if (distance <= distanceToPoint && distance >= 0) {
				Position = start.Lerp(nextPosition, distance / distanceToPoint);
				break;
			} else if (distance < 0) {
				Position = nextPosition;
			}

			distance -= distanceToPoint;
			start = nextPosition;
			path.Dequeue();
		}

		if (path.Count == 0) {
			if (currentGoal == mainGoal) {
				OnGoalReached?.Invoke(this);
			}
			OnPathFinished?.Invoke(this);

		}


	}

	override public void _Draw() {
		if (drawDebug)
			DrawCircle(ToLocal(mainGoal), 2, new Color(1, 0, 0));
	}


	public void SetViewDir(Vector2 direction) {
		float angle = Mathf.Atan2(direction.Y, direction.X) - 1.5708f;
		dir = Mathf.RoundToInt(4 * angle / (2 * Mathf.Pi) + 4) % 4;

		// Use running animation if isRunning is true
		// AnimationState: N=0, E=1, S=2, W=3 for walking
		// AnimationState: NR=4, ER=5, SR=6, WR=7 for running
		int animationIndex = isRunning ? dir + (int)AnimationState.NR : dir;
		Animation = ((AnimationState)animationIndex).ToString();
		Play();
	}
}
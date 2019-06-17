using Godot;
using System;
using System.Collections.Generic;

public class BaseCharacter : AnimatedSprite {
	[Export]
	public int type;

	public const int SpeedWalking = 200, SpeedRunning = 400;
	public bool isInAnimation;

	List<Staircase> staircases;

	KinematicBody2D collider;

	public int dir = 1;
	public Queue<Vector2> path;
	public Vector2 goal;
	public bool shiftsFloor; //Does the player goe from one height level to the other
	const int ShiftHeight = 410; //Guesstimation for the heigth

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

		collider = GetNode<KinematicBody2D>("Collider");

		Frames = new SpriteFrames();
		foreach (var keyPair in data.textures) {
			Frames.AddAnimation(keyPair.Key.ToString());
			Animation = keyPair.Key.ToString();

			foreach (Texture texture in keyPair.Value) {
				Frames.AddFrame(keyPair.Key.ToString(), texture);
			}
		}
	}

	public void SetPath(Vector2 _goal) {
		if (isInAnimation)
			return;

		goal = _goal;

		bool isShiftingUp = Position.y > ShiftHeight;


		shiftsFloor = goal.y > ShiftHeight & Position.y < ShiftHeight | goal.y < ShiftHeight & Position.y > ShiftHeight;

		GD.Print(shiftsFloor);

		FinishedPath = null; //They probably don't want to talk to us anymore

		Staircase nearestStaircase = GetNearestStaircase(isShiftingUp);

		if (shiftsFloor) {
			Action<BaseCharacter> teleport = null;
			teleport = (c) => {
				FinishedPath -= teleport;

				nearestStaircase.TriggerAnimation(this);
			};

			FinishedPath += teleport;
		}

		path = new Queue<Vector2>(
			NavigationController.instance.GetSimplePath(Position, shiftsFloor ? nearestStaircase.GlobalPosition : goal, true));
	}

	public Staircase GetNearestStaircase(bool isShiftingUp) {
		float minDistance = Mathf.Inf;
		Staircase closest = null;
		Vector2 globalPos = GetGlobalPosition();
		foreach (Staircase s in staircases) {
			if (s.isUpstairs && !isShiftingUp)
				continue;

			float dist = globalPos.DistanceTo(s.GlobalPosition);
			if (dist < minDistance) {
				minDistance = dist;
				closest = s;
			}
		}

		return closest;
	}

	override public void _Process(float delta) {
		if (path != null && path?.Count != 0) {
			MoveOnPath(SpeedWalking * delta);
		} else {
			Animation = ((AnimationState)((int)AnimationState.NStanding + dir)).ToString();
		}
	}

	public void MoveOnPath(float distance) {
		Vector2 start = Position;

		for (int i = 0; i < path.Count; i++) {
			Vector2 nextPosition = path.Peek();
			float distanceToPoint = start.DistanceTo(nextPosition);

			Vector2 direction = start - nextPosition;
			SetViewDir(direction);

			if (distance <= distanceToPoint && distance >= 0) {
				Position = start.LinearInterpolate(nextPosition, distance / distanceToPoint);
				break;
			} else if (distance < 0) {
				Position = nextPosition;
			}

			distance -= distanceToPoint;
			start = nextPosition;
			path.Dequeue();
		}

		if (path.Count == 0)
			FinishedPath?.Invoke(this);
	}

	public Action<BaseCharacter> FinishedPath;

	public void SetViewDir(Vector2 direction) {
		float angle = Mathf.Atan2(direction.y, direction.x) - 1.5708f;
		dir = Mathf.RoundToInt(4 * angle / (2 * Mathf.Pi) + 4) % 4;

		Animation = ((AnimationState)dir).ToString();
	}
}
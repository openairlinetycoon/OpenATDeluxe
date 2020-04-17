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
	public Vector2 mainGoal;
	public Vector2 currentGoal;
	public bool shiftsFloor; //Does the player goe from one height level to the other
	const int ShiftHeight = 410; //Guesstimation for the heigth


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

		collider = GetNode<KinematicBody2D>("Collider");

		Frames = new SpriteFrames();
		foreach (var keyPair in data.textures) {
			Frames.AddAnimation(keyPair.Key.ToString());
			Animation = keyPair.Key.ToString();

			foreach (AnimationData.TextureRef texture in keyPair.Value) {
				Frames.AddFrame(keyPair.Key.ToString(), texture.GetTexture());
			}
		}
	}

	public void SetPath(Vector2 _goal) {
		if (isInAnimation)
			return;

		mainGoal = _goal;
		currentGoal = mainGoal;

		Update();

		bool isShiftingUp = Position.y > ShiftHeight;


		shiftsFloor = mainGoal.y > ShiftHeight & Position.y < ShiftHeight | mainGoal.y < ShiftHeight & Position.y > ShiftHeight;

		OnPathFinished = null; //They probably don't want to talk to us anymore
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

	override public void _Process(float delta) {
		delta *= GameController.TimeScale;
		SpeedScale = data.speed *GameController.TimeScale;
		
		Update();
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
		float angle = Mathf.Atan2(direction.y, direction.x) - 1.5708f;
		dir = Mathf.RoundToInt(4 * angle / (2 * Mathf.Pi) + 4) % 4;

		Animation = ((AnimationState)dir).ToString();
	}
}
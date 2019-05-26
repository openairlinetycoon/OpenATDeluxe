using Godot;
using System;

public class CameraController : Camera2D {
	[Export]
	public float speed;
	float width = 1920;
	public static Camera2D airportCamera;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		airportCamera = this;

	}


	public override void _Process(float delta) {
		//SetPosition(GetViewport().GetMousePosition());

		float offset = GetViewport().GetMousePosition().x - width / 2;
		float p = 100 / (width / 2) * offset;
		int side = Math.Sign(p);
		p *= side;

		if (p / 100 >= DragMarginLeft) {
			MouseCursor.instance.movingCamera = side;
		} else {
			MouseCursor.instance.movingCamera = 0;
			side = 0;
		}

		SetPosition (GetGlobalMousePosition () + new Vector2 (speed * (Input.IsMouseButtonPressed (1) ? 2 : 1), 0) * side);
	}

	public Vector2 _GetPosition () {
		return GetPosition ();
	}

	public void _SetPosition (Vector2 pos) {
		SetPosition (pos);
	}
}
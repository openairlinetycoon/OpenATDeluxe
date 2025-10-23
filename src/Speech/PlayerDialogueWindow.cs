using Godot;
using System;

public partial class PlayerDialogueWindow : DialogueWindow {
	public Texture2D player;
	public Texture2D[] talking = new Texture2D[3];
	public Texture2D telephone;
	public Texture2D[] telephoneTalking = new Texture2D[3];

	[Export]
	public NodePath iconPath;
	public Sprite2D icon;

	public bool isDialoguePartner;

	public bool isTalking;
	public bool isTelephoneCall;
	public float mouthIntervall = 0.1f * 1000, startTime;
	public Vector2 movePosition = new Vector2(0, 305);
	public Vector2 basePosition;

	Random ran = new Random();

	override public void _Ready() {
		base._Ready();

		icon = GetNode<Sprite2D>(iconPath);
		basePosition = icon.Position;

		player = (Texture2D)GD.Load($"res://Images/gli/glberatr/BER9{GameController.currentPlayerID}_01.res");
		talking[0] = (Texture2D)GD.Load($"res://Images/gli/glberatr/BER9{GameController.currentPlayerID}_02.res");
		talking[1] = (Texture2D)GD.Load($"res://Images/gli/glberatr/BER9{GameController.currentPlayerID}_03.res");
		talking[2] = (Texture2D)GD.Load($"res://Images/gli/glberatr/BER9{GameController.currentPlayerID}_04.res");

		telephone = (Texture2D)GD.Load($"res://Images/gli/glberatr/BER9{GameController.currentPlayerID}_05.res");
		telephoneTalking[0] = (Texture2D)GD.Load($"res://Images/gli/glberatr/BER9{GameController.currentPlayerID}_06.res");
		telephoneTalking[1] = (Texture2D)GD.Load($"res://Images/gli/glberatr/BER9{GameController.currentPlayerID}_07.res");
		telephoneTalking[2] = (Texture2D)GD.Load($"res://Images/gli/glberatr/BER9{GameController.currentPlayerID}_08.res");

		DialogueSystem.onDialogueStart += () => {
			//icon.Position = basePosition;
			//icon.Position += movePosition;
		};
	}

	override public void OnStartTalking() {
		if(RoomManager.currentRoom == "RoomOffice")
			return;
		
		if (DialogueSystem.currentlyTalking == GameController.CurrentPlayerTag) {
			isTalking = true;
			startTime = Time.GetTicksMsec() + mouthIntervall;
		}
	}

	override public void OnStopTalking() {
		isTalking = false;
		icon.Texture = isTelephoneCall ? telephone : player;
	}

	override public void _Process(double delta) {
		base._Process(delta);

		Vector2 goal = basePosition;
		if (DialogueSystem.currentDialogue == null || RoomManager.currentRoom == "RoomOffice") {
			goal = basePosition + movePosition;
			if (icon.Position.DistanceTo(goal) <= 20) {
				icon.Hide();
			}
		} else {
			icon.Show();
		}

		if (icon.Position.DistanceTo(goal) > 20) {
			icon.Position = icon.Position.Lerp(goal, (float)(delta * 4)).Round();
			icon.ZIndex = -1;
		} else {
			icon.ZIndex = 0;
		}

		if (isTalking && DialogueSystem.currentSound?.IsTalking() == true) {
			if (startTime < Time.GetTicksMsec()) {
				startTime = Time.GetTicksMsec() + mouthIntervall;
				int rID = ran.Next(0, 3);

				icon.Texture = isTelephoneCall ? telephoneTalking[rID] : talking[rID];
			}
		} else {
			icon.Texture = isTelephoneCall ? telephone : player;
		}
	}
}

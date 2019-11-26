using Godot;
using System;
using System.Collections.Generic;

public class Office : Node2D {
	public Control baseNode;

	AnimationList playerAnims = new AnimationList();
	public override void _Ready() {
		AddToGroup("cancelable");

		baseNode = (Control)GetChild(0);

		Vector2 playerPos = new Vector2(-280, -90);

		playerAnims.basePosition = playerPos;

		bool alreadyFired = false;
		bool TriggerIdleAnimation(AnimationGoal goal) {
			//If phonecall happens:
			bool ret = !alreadyFired;
			alreadyFired = true;

			goal.triggerID = 1;
			return ret;
		}

		Dialogue test = new Dialogue("Bank"); //Yes? - Start
		DialogueNode start = new DialogueNode(109);
		DialogueNodeReturning noNewLoan = new DialogueNodeReturning(120);
		DialogueNodeReturning noReturningOfLoan = new DialogueNodeReturning(140);

		test.AddNode(start)
			.AddNode(noNewLoan)
			.AddNode(noReturningOfLoan);

		start.AddOption(new DialogueOption(101, noNewLoan));
		start.AddOption(new DialogueOption(102, noReturningOfLoan));
		start.AddOption(new DialogueOptionReturning(103));

		DialogueSystem.StartDialogue(test, "P1");
		string player = "PL2";

		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "Wait.smk", goal: new AnimationGoal(onTrigger: TriggerIdleAnimation)));
		//Idle: 0
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "Blat.smk", audio: SoundPlayer.CreatePlayer("/video/plxblat.raw", "effects"), goal: new AnimationGoal(finish: 3, cancel: 0)));
		//Idle animation: 1
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "Scri1.smk", audio: SoundPlayer.CreatePlayer($"/video/{player}Scri1.raw", "effects"), goal: new AnimationGoal(finish: 0, cancel: 0)));
		//Idle animation: 2
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "Scri2.smk", audio: SoundPlayer.CreatePlayer($"/video/{player}Scri2.raw", "effects"), goal: new AnimationGoal(finish: 0, cancel: 0)));
		//Idle animation: 3
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "Brief.smk", goal: new AnimationGoal(finish: 5, cancel: 5)));
		//Letter grab: 4
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "BrifW.smk", goal: new AnimationGoal(trigger: 6, cancel: 6)));
		//Letter read: 5
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "BrifZ.smk", goal: new AnimationGoal(finish: 0)));
		//Letter return: 6 
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "Telef.smk", goal: new AnimationGoal(finish: 8, cancel: 8)));
		//Phone grab: 7
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "TeleW.smk", goal: new AnimationGoal(trigger: 9)));
		//Phone wait: 8
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "TeleR.smk", goal: new AnimationGoal(finish: 8)));
		//Phone talk: 9
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "TeleZ.smk", goal: new AnimationGoal(finish: 0)));
		//Phone return: 10

		playerAnims.Play(0);

		GetTree().CallGroup("cancelable", "Cancel");
	}

	override public void _Process(float delta) {
		playerAnims.ProcessTrigger();
	}

	public void Cancel() {
		playerAnims.Cancel();
	}
}

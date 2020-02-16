using Godot;
using System;
using System.Collections.Generic;

public class Office : BaseRoom {
	[Export]
	public NodePath telephoneListPath;
	public ListElement telephoneList;

	[Export]
	public NodePath telephonePath;

	AnimationList playerAnims = new AnimationList();
	public override void OnReady() {
		Vector2 playerPos = new Vector2(-280, -90);

		playerAnims.basePosition = playerPos;

		bool alreadyFired = false;
		bool TriggerIdleAnimation(AnimationGoal goal) {
			//If phonecall happens:
			bool ret = !alreadyFired;
			alreadyFired = true;
			
			if(ret)
				goal.triggerID = 1;
			
			return ret;
		}

		bool TriggerTelephoneAnimation(AnimationGoal goal) {
			bool ret = DialogueSystem.IsDialogueActive;
			if(ret)
				goal.triggerID = 7;
			return ret;
		}

		MouseArea telephone = (MouseArea)GetNode(telephonePath);
		telephone.onClick += () => telephoneList.ShowElement();
		telephoneList = (ListElement)GetNode(telephoneListPath);

		string player = "PL" + GameController.currentPlayerID;
		DialogueSystem.OverrideActor(new Actor(GameController.CurrentPlayerTag, (DialogueWindow)GetTree().Root.GetNode("Game").FindNode("PLOffice")));

		//Idle: 0
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "Wait.smk", goal: new AnimationGoal(onTrigger: (goal)=> {
				bool isTriggered = TriggerIdleAnimation(goal);
				isTriggered = isTriggered | TriggerTelephoneAnimation(goal); //Order matters! The later this call, the higher the priority!
				return isTriggered;
			})));
		
		//Idle animation: 1
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "Blat.smk", audio: SoundPlayer.CreatePlayer("/video/plxblat.raw", "effects"), goal: new AnimationGoal(finish: 3, cancel: 0)));
		
		//Idle animation: 2
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "Scri1.smk", audio: SoundPlayer.CreatePlayer($"/video/{player}Scri1.raw", "effects"), goal: new AnimationGoal(finish: 0, cancel: 0)));
		
		//Idle animation: 3
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "Scri2.smk", audio: SoundPlayer.CreatePlayer($"/video/{player}Scri2.raw", "effects"), goal: new AnimationGoal(finish: 0, cancel: 0)));
		
		//Letter grab: 4
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "Brief.smk", goal: new AnimationGoal(finish: 5, cancel: 5)));
		
		//Letter read: 5
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "BrifW.smk", goal: new AnimationGoal(trigger: 6, cancel: 6)));
		
		//Letter return: 6 
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "BrifZ.smk", goal: new AnimationGoal(finish: 0)));
		
		//Phone grab: 7
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "Telef.smk", goal: new AnimationGoalDialogueStart(true, finish: 8, cancel: 8)));
		
		//Phone wait: 8
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "TeleW.smk",
				goal: new AnimationGoalListening(null, "P2", 9, 10)));
		
		//Phone talk: 9
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "TeleR.smk",
				goal: new AnimationGoalTalking(null, "P2", 8, 10)));
		
		//Phone return: 10
		playerAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, player + "TeleZ.smk", goal: new AnimationGoal(finish: 0)));
		

		playerAnims.Play(0);
	}

	override public void _Process(float delta) {
		playerAnims.ProcessTrigger();
	}

	override public void Cancel() {
		playerAnims.Cancel();
	}
}

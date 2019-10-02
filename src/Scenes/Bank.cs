using Godot;
using System;
using System.Collections.Generic;

public class Bank : Node2D {
	public Control baseNode;

	AnimationList creditAnims = new AnimationList();
	AnimationList sharesAnims = new AnimationList();
	public override void _Ready() {
		AddToGroup("cancelable");

		baseNode = (Control)GetChild(0);

		Vector2 playerPos = new Vector2(-280, -90);

		creditAnims.basePosition = new Vector2(-142, -73);
		sharesAnims.basePosition = new Vector2(33, -63);
		creditAnims.CreateMouseArea(baseNode);
		sharesAnims.CreateMouseArea(baseNode);


		Dialogue creditDialogue = new Dialogue("Bank");
		{
			DialogueNode startCredit = new DialogueNode(109);
			DialogueNodeReturning noNewLoan = new DialogueNodeReturning(120);
			DialogueNodeReturning noReturningOfLoan = new DialogueNodeReturning(140);

			creditDialogue.AddNode(startCredit)
				.AddNode(noNewLoan)
				.AddNode(noReturningOfLoan);

			startCredit.AddOption(new DialogueOption(101, noNewLoan));
			startCredit.AddOption(new DialogueOption(102, noReturningOfLoan));
			startCredit.AddOption(new DialogueOptionReturning(103));
		}

		Dialogue shareDialogue = new Dialogue("Bank");
		{//keep them in scope, so that we don't use the wrong nodes from the prev. dialogue
			DialogueNode startShare = new DialogueNode(900);
			DialogueNodeReturning noShares = new DialogueNodeReturning(1200);
			DialogueNodeReturning noSelling = new DialogueNodeReturning(1100);

			shareDialogue.AddNode(startShare)
				.AddNode(noShares)
				.AddNode(noSelling);

			startShare.AddOption(new DialogueOption(1000, noShares));
			startShare.AddOption(new DialogueOption(1001, noSelling));
			startShare.AddOption(new DialogueOptionReturning(1005));
		}
		creditAnims.mouseArea.onClick += () => DialogueSystem.StartDialogue(creditDialogue, "P2", "B2");
		sharesAnims.mouseArea.onClick += () => DialogueSystem.StartDialogue(shareDialogue, "P2", "BA");
		//DialogueSystem.StartDialogue(shareDialogue, "P2", "BA");
		string player = "PL2";

		bool alreadyFired = false;
		bool TriggerIdleAnimationCredit(AnimationGoal goal) {
			//If dialogue is running:
			if (DialogueSystem.currentDialogue == creditDialogue) {
				goal.triggerID = 2;
				return true;
			}

			bool ret = !alreadyFired;
			alreadyFired = true;

			goal.triggerID = 1;
			return ret;
		}
		bool TriggerIdleAnimationShare(AnimationGoal goal) {
			//If dialogue is running:
			if (DialogueSystem.currentDialogue == shareDialogue) {
				goal.triggerID = 2;
				return true;
			}

			bool ret = !alreadyFired;
			alreadyFired = true;

			goal.triggerID = 1;
			return ret;
		}
		// bool WhileListening(AnimationGoal goal) {
		// 	if (DialogueSystem.currentlyTalking == "B2") {
		// 		goal.triggerID = 6;
		// 		return true;
		// 	}
		// 	if (DialogueSystem.currentDialogue != creditDialogue) {
		// 		goal.triggerID = 3;
		// 		return true;
		// 	}

		// 	return WhileTalkingAnimHelper(g, creditDialogue, "B2", 6, 3);
		// }
		// bool WhileTalking(AnimationGoal goal) {
		// 	const string actor = "B2";
		// 	const int noTalkingID = 4;
		// 	const int dialogueStoppedID = 3;
		// 	return WhileTalkingAnimHelper(goal, creditDialogue, actor, noTalkingID, dialogueStoppedID);
		// }


		creditAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_Wait.smk", goals: new AnimationGoal(onTrigger: TriggerIdleAnimationCredit)));
		creditAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_Work.smk", goals: new AnimationGoal(finish: 0, cancel: 0)));
		creditAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_Turn.smk", goals: new AnimationGoal(finish: 4, cancel: 4)));
		creditAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_TurnZ.smk", goals: new AnimationGoal(finish: 0)));
		creditAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_RedeW.smk",
			goals: new AnimationGoal(onTrigger: (g) => WhileListeningAnimHelper(g, creditDialogue, "B2", 6, 3))));
		creditAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_RedeK.smk",
			goals: new AnimationGoal(finish: 4, onTrigger: (g) => WhileListeningAnimHelper(g, creditDialogue, "B2", 6, 3))));
		creditAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_Rede.smk",
			goals: new AnimationGoal(onTrigger: (g) => WhileTalkingAnimHelper(g, creditDialogue, "B2", 4, 3))));

		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_Wait.smk", goals: new AnimationGoal(onTrigger: TriggerIdleAnimationShare)));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_Scrib.smk", goals: new AnimationGoal(finish: 0, cancel: 0)));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_Turn.smk", goals: new AnimationGoal(finish: 4, cancel: 4)));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_TurnZ.smk", goals: new AnimationGoal(finish: 0)));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_RedeW.smk",
			goals: new AnimationGoal(onTrigger: (g) => WhileListeningAnimHelper(g, shareDialogue, "BA", 6, 3))));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_RedeK.smk",
			goals: new AnimationGoal(finish: 4, onTrigger: (g) => WhileListeningAnimHelper(g, shareDialogue, "BA", 6, 3))));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_Rede.smk",
			goals: new AnimationGoal(onTrigger: (g) => WhileTalkingAnimHelper(g, shareDialogue, "BA", 4, 3))));


		creditAnims.Play(0);
		sharesAnims.Play(0);
		GetTree().CallGroup("cancelable", "Cancel");
	}

	private static bool WhileTalkingAnimHelper(AnimationGoal goal, Dialogue creditDialogue, string actor, int noTalkingID, int dialogueStoppedID) {
		if (DialogueSystem.currentlyTalking != actor) {
			goal.triggerID = noTalkingID;
			return true;
		}
		if (DialogueSystem.currentDialogue != creditDialogue) {
			goal.triggerID = dialogueStoppedID;
			return true;
		}

		return false;
	}
	private static bool WhileListeningAnimHelper(AnimationGoal goal, Dialogue creditDialogue, string actor, int startTalkingID, int dialogueStoppedID) {
		if (DialogueSystem.currentlyTalking == actor) {
			goal.triggerID = startTalkingID;
			return true;
		}
		if (DialogueSystem.currentDialogue != creditDialogue) {
			goal.triggerID = dialogueStoppedID;
			return true;
		}

		return false;
	}

	override public void _Process(float delta) {
		creditAnims.ProcessTrigger();
		sharesAnims.ProcessTrigger();
	}

	public void Cancel() {
		//creditAnims.Cancel();
	}
}

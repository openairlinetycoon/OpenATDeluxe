using Godot;
using System;
using System.Collections.Generic;

public class Bank : Node2D {
	public Control baseNode;

	AnimationList loanAnims = new AnimationList();
	AnimationList sharesAnims = new AnimationList();
	public override void _Ready() {
		AddToGroup("cancelable");

		baseNode = (Control)GetChild(0);

		Vector2 playerPos = new Vector2(-280, -90);

		loanAnims.basePosition = new Vector2(-142, -73);
		sharesAnims.basePosition = new Vector2(33, -63);
		loanAnims.CreateMouseArea(baseNode);
		sharesAnims.CreateMouseArea(baseNode);



		Dialogue loanDialogue = new Dialogue("Bank");
		{
			DialogueNode startCredit = new DialogueNode(109);
			DialogueNodeReturning noNewLoan = new DialogueNodeReturning(120);
			DialogueNodeReturning noReturningOfLoan = new DialogueNodeReturning(140);

			loanDialogue.AddNode(startCredit)
				.AddNode(noNewLoan)
				.AddNode(noReturningOfLoan);

			startCredit.AddOption(new DialogueOption(101, noNewLoan));
			startCredit.AddOption(new DialogueOption(102, noReturningOfLoan));
			startCredit.AddOption(new DialogueOptionReturning(103));
		}

		Dialogue sharesDialogue = new Dialogue("Bank");
		{//keep them in scope, so that we don't use the wrong nodes from the prev. dialogue
			DialogueNode startShare = new DialogueNode(900);

			DialogueNode buyShares = new DialogueNode(2000) { returnable = false };
			DialogueNodeReturning noShares = new DialogueNodeReturning(1200);
			DialogueNodeReturning noSelling = new DialogueNodeReturning(1100);

			startShare.AddOption(new DialogueOption(1000, buyShares));
			startShare.AddOption(new DialogueOption(1001, noSelling));
			startShare.AddOption(new DialogueOptionReturning(1005));

			//buyShares 2000
			DialogueNode okBuyAnother = new DialogueNode(2030) { returnable = false };

			buyShares.AddOption(new DialogueOption(2010, okBuyAnother, GameController.playerCompanyNames[0], GameController.playerNames[0]));
			buyShares.AddOption(new DialogueOption(2011, okBuyAnother, GameController.playerCompanyNames[1], GameController.playerNames[1]));
			buyShares.AddOption(new DialogueOption(2012, okBuyAnother, GameController.playerCompanyNames[2], GameController.playerNames[2]));
			buyShares.AddOption(new DialogueOption(2013, okBuyAnother, GameController.playerCompanyNames[3], GameController.playerNames[3]));
			buyShares.AddOption(new DialogueOptionReturning(2014));

			//okBuyAnother

			okBuyAnother.AddOption(new DialogueOption(2020, okBuyAnother, GameController.playerCompanyNames[0], GameController.playerNames[0]));
			okBuyAnother.AddOption(new DialogueOption(2021, okBuyAnother, GameController.playerCompanyNames[1], GameController.playerNames[1]));
			okBuyAnother.AddOption(new DialogueOption(2022, okBuyAnother, GameController.playerCompanyNames[2], GameController.playerNames[2]));
			okBuyAnother.AddOption(new DialogueOption(2023, okBuyAnother, GameController.playerCompanyNames[3], GameController.playerNames[3]));
			okBuyAnother.AddOption(new DialogueOptionReturning(2024));



			sharesDialogue.AddNode(startShare);
			// 	.AddNode(buyShares)
			// 	.AddNode(okBuyAnother)
			// 	.AddNode(noShares)
			// 	.AddNode(noSelling);

		}
		loanAnims.mouseArea.onClick += () => DialogueSystem.PrepareDialogue(loanDialogue, "P2", "B2");
		sharesAnims.mouseArea.onClick += () => DialogueSystem.PrepareDialogue(sharesDialogue, "P2", "BA");
		//DialogueSystem.StartDialogue(shareDialogue, "P2", "BA");
		string player = "PL2";

		bool alreadyFired = false;
		bool TriggerIdleAnimation(AnimationGoal goal, Dialogue thisDialogue) {
			//If dialogue is running:
			if (DialogueSystem.currentDialogue == thisDialogue) {
				goal.triggerID = 2;
				return true;
			}

			bool ret = !alreadyFired;
			alreadyFired = true;

			goal.triggerID = 1;
			return ret;
		}


		loanAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_Wait.smk", goal: new AnimationGoal(onTrigger: (g) => TriggerIdleAnimation(g, loanDialogue))));
		loanAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_Work.smk", goal: new AnimationGoal(finish: 0, cancel: 0)));
		loanAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_Turn.smk", goal: new AnimationGoalDialogueStart(finish: 4, cancel: 4)));
		loanAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_TurnZ.smk", goal: new AnimationGoal(finish: 0)));
		loanAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_RedeW.smk",
			goal: new AnimationGoalListening(loanDialogue, "B2", 6, 3)));
		loanAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_RedeK.smk",
			goal: new AnimationGoalListening(loanDialogue, "B2", 6, 3) { finishID = 4 }));
		loanAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_Rede.smk",
			goal: new AnimationGoalTalking(loanDialogue, "B2", 4, 3)));

		// loanAnims.Add(
		// 	SmkAnimation.CreateAnimation(baseNode, "BF_RedeW.smk",
		// 	goals: new AnimationGoal(onTrigger: (g) => WhileListeningAnimHelper(g, loanDialogue, "B2", 6, 3))));
		// loanAnims.Add(
		// 	SmkAnimation.CreateAnimation(baseNode, "BF_RedeK.smk",
		// 	goals: new AnimationGoal(finish: 4, onTrigger: (g) => WhileListeningAnimHelper(g, loanDialogue, "B2", 6, 3))));
		// loanAnims.Add(
		// 	SmkAnimation.CreateAnimation(baseNode, "BF_Rede.smk",
		// 	goals: new AnimationGoal(onTrigger: (g) => WhileTalkingAnimHelper(g, loanDialogue, "B2", 4, 3))));

		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_Wait.smk", goal: new AnimationGoal(onTrigger: (g) => TriggerIdleAnimation(g, sharesDialogue))));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_Scrib.smk", goal: new AnimationGoal(finish: 0, cancel: 0)));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_Turn.smk", goal: new AnimationGoalDialogueStart(finish: 4, cancel: 4)));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_TurnZ.smk", goal: new AnimationGoal(finish: 0)));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_RedeW.smk",
			goal: new AnimationGoalListening(sharesDialogue, "BA", 6, 3)));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_RedeK.smk",
			goal: new AnimationGoalListening(sharesDialogue, "BA", 6, 3) { finishID = 4 }));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_Rede.smk",
			goal: new AnimationGoalTalking(sharesDialogue, "BA", 4, 3)));

		// sharesAnims.Add(
		// 	SmkAnimation.CreateAnimation(baseNode, "BM_RedeW.smk",
		// 	goals: new AnimationGoal(onTrigger: (g) => WhileListeningAnimHelper(g, sharesDialogue, "BA", 6, 3))));
		// sharesAnims.Add(
		// 	SmkAnimation.CreateAnimation(baseNode, "BM_RedeK.smk",
		// 	goals: new AnimationGoal(finish: 4, onTrigger: (g) => WhileListeningAnimHelper(g, sharesDialogue, "BA", 6, 3))));
		// sharesAnims.Add(
		// 	SmkAnimation.CreateAnimation(baseNode, "BM_Rede.smk",
		// 	goals: new AnimationGoal(onTrigger: (g) => WhileTalkingAnimHelper(g, sharesDialogue, "BA", 4, 3))));


		loanAnims.Play(0);
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
		loanAnims.ProcessTrigger();
		sharesAnims.ProcessTrigger();
	}

	public void Cancel() {
		//creditAnims.Cancel();
	}
}

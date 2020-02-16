using Godot;
using System;
using System.Collections.Generic;

public class Bank : BaseRoom {

	AnimationList loanAnims = new AnimationList();
	AnimationList sharesAnims = new AnimationList();

	public override void OnReady() {
		Vector2 playerPos = new Vector2(-280, -90);

		loanAnims.basePosition = new Vector2(-142, -73);
		sharesAnims.basePosition = new Vector2(33, -63);
		loanAnims.CreateMouseArea(baseNode);
		sharesAnims.CreateMouseArea(baseNode);

		DialogueSystem.AddActor(new Actor("BA", (DialogueWindow)FindNode("BA")));
		DialogueSystem.AddActor(new Actor("B2", (DialogueWindow)FindNode("B2"), 520));

		Dialogue loanDialogue = new Dialogue("Bank", nameof(loanDialogue), "B2");
		{ // loan
			loanDialogue.AddOnTelephoneNode(new DialogueNodeReturning(150));
			
			DialogueNode startLoan = new DialogueNode(109);
			DialogueNodeReturning noNewLoan = new DialogueNodeReturning(120);
			DialogueNodeReturning noReturningOfLoan = new DialogueNodeReturning(140);

			loanDialogue.AddNode(startLoan)
				.AddNode(noNewLoan)
				.AddNode(noReturningOfLoan);

			startLoan.AddOptions(new DialogueOptionTelephone(101, noNewLoan),
								 new DialogueOptionTelephone(102, noReturningOfLoan),
								 new DialogueOptionReturning(103));
		}

		Dialogue stocksDialogue = new Dialogue("Bank", nameof(stocksDialogue), "BA");
		// shares
		{//keep them in scope, so that we don't use the wrong nodes from the prev. dialogue
			stocksDialogue.AddOnTelephoneNode(new DialogueNodeReturning(1200));
			DialogueNode startStock = new DialogueNode(900);

			DialogueNode buyStocks = new DialogueNode(2000) { returnable = false };
			DialogueNodeReturning noStocks = new DialogueNodeReturning(1200);
			DialogueNodeReturning noSelling = new DialogueNodeReturning(1100);

			startStock.AddOptions(new DialogueOptionTelephone(1000, buyStocks),
								  new DialogueOptionTelephone(1001, noSelling),
								  new DialogueOptionReturning(1005));

			//buyShares 2000
			DialogueNode okBuyAnother = new DialogueNode(2030) { returnable = false };

			buyStocks.AddOptions(
				new DialogueOption(2010, okBuyAnother, GameController.playerCompanyNames[0], GameController.playerNames[0]),
				new DialogueOption(2011, okBuyAnother, GameController.playerCompanyNames[1], GameController.playerNames[1]),
				new DialogueOption(2012, okBuyAnother, GameController.playerCompanyNames[2], GameController.playerNames[2]),
				new DialogueOption(2013, okBuyAnother, GameController.playerCompanyNames[3], GameController.playerNames[3]),
				new DialogueOptionReturning(2014)
			);

			//okBuyAnother

			okBuyAnother.AddOptions(
				new DialogueOption(2020, okBuyAnother, GameController.playerCompanyNames[0], GameController.playerNames[0]),
				new DialogueOption(2021, okBuyAnother, GameController.playerCompanyNames[1], GameController.playerNames[1]),
				new DialogueOption(2022, okBuyAnother, GameController.playerCompanyNames[2], GameController.playerNames[2]),
				new DialogueOption(2023, okBuyAnother, GameController.playerCompanyNames[3], GameController.playerNames[3]),
				new DialogueOptionReturning(2024)
			);



			stocksDialogue.AddNode(startStock);
			// 	.AddNode(buyShares)
			// 	.AddNode(okBuyAnother)
			// 	.AddNode(noShares)
			// 	.AddNode(noSelling);

		}

		loanAnims.mouseArea.onClick += () => { DialogueSystem.PrepareDialogue(loanDialogue); DialogueSystem.StartWithOptions(); };
		sharesAnims.mouseArea.onClick += () => DialogueSystem.PrepareDialogue(stocksDialogue);
		//DialogueSystem.StartDialogue(shareDialogue, "P2", "BA");

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

		//Loans
		loanAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_Wait.smk", goal: new AnimationGoal(onTrigger: (g) => TriggerIdleAnimation(g, loanDialogue))));
		loanAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_Work.smk", goal: new AnimationGoal(finish: 0, cancel: 0)));
		loanAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BF_Turn.smk", goal: new AnimationGoal(finish: 4, cancel: 4)));
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

		//Shares
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_Wait.smk", goal: new AnimationGoal(onTrigger: (g) => TriggerIdleAnimation(g, stocksDialogue))));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_Scrib.smk", goal: new AnimationGoal(finish: 0, cancel: 0)));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_Turn.smk", goal: new AnimationGoalDialogueStart(finish: 4, cancel: 4)));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_TurnZ.smk", goal: new AnimationGoal(finish: 0)));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_RedeW.smk",
			goal: new AnimationGoalListening(stocksDialogue, "BA", 6, 3)));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_RedeK.smk",
			goal: new AnimationGoalListening(stocksDialogue, "BA", 6, 3) { finishID = 4 }));
		sharesAnims.Add(
			SmkAnimation.CreateAnimation(baseNode, "BM_Rede.smk",
			goal: new AnimationGoalTalking(stocksDialogue, "BA", 4, 3)));

		loanAnims.Play(0);
		sharesAnims.Play(0);
	}

	override public void _Process(float delta) {
		loanAnims.ProcessTrigger();
		sharesAnims.ProcessTrigger();
	}

	override public void Cancel() {
		//creditAnims.Cancel();
	}
}

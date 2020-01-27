using Godot;
using System;
using System.Collections.Generic;

public class Manager : BaseRoom {

	AnimationList manager = new AnimationList();

	public override void OnReady() {
		DialogueSystem.AddActor(new Actor("BO", (DialogueWindow)FindNode("BO")));

		manager.basePosition = new Vector2(24, -136);
		manager.CreateMouseArea(baseNode);


		Dialogue managerDialogue = new Dialogue("Boss", nameof(managerDialogue), "BO");
		{
			DialogueNode start = new DialogueNode(4000);

			DialogueNodeReturning expansion = new DialogueNodeReturning(4114);
			DialogueNodeReturning objective = new DialogueNodeReturning(4060);
			DialogueNode objectiveStart = new DialogueNode(4050);

			objectiveStart.AddFollowup(objective);

			start.AddOptions(new DialogueOption(4010, objectiveStart),
							 new DialogueOption(4011, expansion),
							 new DialogueOptionReturning(4012));

			managerDialogue.AddNode(start);
		}

		manager.mouseArea.onClick += () => { DialogueSystem.StartDialogue(managerDialogue); manager.Play(2); };

		manager.Add(
			SmkAnimation.CreateAnimation(baseNode, "BB_Wait.smk"));
		manager.Add(
			SmkAnimation.CreateAnimation(baseNode, "BB_Wait.smk",
			goal: new AnimationGoalListening(managerDialogue, "BO", 2, 0)));
		manager.Add(
			SmkAnimation.CreateAnimation(baseNode, "BB_Base.smk",
			goal: new AnimationGoalTalking(managerDialogue, "BO", 1, 0)));


		manager.Play(0);
	}

	override public void _Process(float delta) {
		manager.ProcessTrigger();
	}

	override public void Cancel() {
	}
}

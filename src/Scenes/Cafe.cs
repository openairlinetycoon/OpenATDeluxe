using Godot;
using System;
using System.Collections.Generic;

public class Cafe : BaseRoom {
	AnimationList rickAnims = new AnimationList();

	//SAVED
	public static bool hasMetRick = false;
	
	//STILL WIP - will be finished when missions are implemented - rick depends on those!
	public override void OnReady() {
		Vector2 rickPos = new Vector2(-229, -91);
		rickAnims.basePosition = rickPos;
		rickAnims.CreateMouseArea(baseNode);
		
		DialogueSystem.AddActor(new Actor("RI",(DialogueWindow)FindNode("RI")));
		
		Dialogue rickDialogue = new Dialogue("Rick","rickDialogue","RI");
		{
			DialogueNode start = new DialogueNode(1000);
			rickDialogue.AddNode(start); //All nodes which can start a dialogue have to be added to the dialogue!
			
			DialogueNode greeting = new DialogueNode(1200);
			greeting.AddEvent(()=>hasMetRick=true);
			
			DialogueNode noNews = new DialogueNodeExit(2199);
			
			//if(firstMeet)
			start.AddOption(new DialogueOptionConditioned(()=>hasMetRick?2000:1100, ()=>hasMetRick?noNews:greeting));
			//else
			//start.AddOption(new DialogueOption(2000, randomTip));

			DialogueNode firstAdvice1 = new DialogueNode(1400);
			DialogueNode firstAdvice2 = new DialogueNodeExit(1401);
			firstAdvice1.AddFollowup(firstAdvice2);
			
			greeting.AddOption(new DialogueOption(1300, firstAdvice1));
		}
		
		
		rickAnims.mouseArea.onClick += () => DialogueSystem.StartDialogue(rickDialogue);
		
		//Idle 0:
		rickAnims.Add(SmkAnimation.CreateAnimation(baseNode,"Bar_WAIT.smk",
			goal:new AnimationGoalIdle("RI",2,1)));
		
		//Idle 1:
		rickAnims.Add(SmkAnimation.CreateAnimation(baseNode,"Bar_AUG.smk",
			goal:new AnimationGoal(finish:0)));
		
		//Talk 2:
		rickAnims.Add(SmkAnimation.CreateAnimation(baseNode,"Bar_rede.smk",
			goal:new AnimationGoalTalking(null,"RI",3,0)));
		
		//No Talk 3:
		rickAnims.Add(SmkAnimation.CreateAnimation(baseNode,"Bar_WAIT.smk",
			goal:new AnimationGoalListening(null,"RI",2,0)));

		
		rickAnims.Play(0);

	}
	override public void _Process(float delta) {
		rickAnims.ProcessTrigger();
	}
	public override void Cancel() {
		throw new NotImplementedException();
	}
}
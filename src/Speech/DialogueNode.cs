using System;
using System.Collections.Generic;
using Godot;

/*
    Needs:
    -Branching on condition

*/

public class DialogueNode {
	protected Dialogue master;

	public int textId;

	public string[] wildcards;

	public bool returnable = true;

	public List<DialogueOption> options = new List<DialogueOption>();

	public DialogueNode followup;

	protected Action onSpeechFinished, start;

	public DialogueNode(int textId, params string[] wildcards) {
		this.textId = textId;
		this.wildcards = wildcards;
	}

	public void AddFollowup(DialogueNode node) {
		followup = node;
		returnable = false;
	}

	public virtual void Start(Dialogue master) {
		this.master = master;

		start?.Invoke();
	}
	public virtual void OnSpeechFinished() {
		onSpeechFinished?.Invoke();
	}

	public void AddOption(DialogueOption option) {
		options.Add(option);
	}

	public void AddOptions(params DialogueOption[] option) {
		foreach (DialogueOption o in option) {
			options.Add(o);
		}
	}

	public void AddEvent(Action onSpeechFinished) {
		this.onSpeechFinished += onSpeechFinished;
	}
	
	public void PreventReturning() {
		returnable = false;
	}

	public bool HasFollowup() {
		return followup != null;
	}

	public void GoToFollowup() {
		master.StartNode(followup);
	}
}

public class DialogueNodeReturning : DialogueNode {
	public DialogueNodeReturning(int textId, params string[] wildcards) : base(textId, wildcards) {
		returnable = false;
	}

	new private void AddOption(DialogueOption option) { } //We don't allow options for returning nodes. That would confuse people

	public override void OnSpeechFinished() {
		base.OnSpeechFinished();

		master.ReturnToPrevNode(); //We are done here... maybe. TODO: Check if the game returns to prev. node when they finished talking
	}
}

public class DialogueNodeExit : DialogueNode {
	public DialogueNodeExit(int textId, params string[] wildcards) : base(textId, wildcards) {
		returnable = false;
	}

	new private void AddOption(DialogueOption option) { } //We don't allow options for returning nodes. That would confuse people

	public override void OnSpeechFinished() {
		base.OnSpeechFinished();

		DialogueSystem.StopDialogue();
	}
}
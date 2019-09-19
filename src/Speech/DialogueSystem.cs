using Godot;
using System;
using Environment = System.Environment;

public class DialogueSystem : Node2D {
	static DialogueSystem instance;

	override public void _Ready() {
		instance = this;
	}

	public static Dialogue currentDialogue;
	public static bool IsDialogueActive {
		get {
			return currentDialogue != null;
		}
	}

	public static DialogueWindow Speechbubble {
		get {
			_speechbubble = _speechbubble ?? (DialogueWindow)instance.FindNode("Prefab"); //TODO: FIX ME!!
			return _speechbubble;
		}
		set => _speechbubble = value;
	}

	private static DialogueWindow _speechbubble;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="dialogueGroup">Like "Bank", or "Makl".
	/// The first part of a localized string ("xxx>1000" - the xxx part).</param>
	/// <param name="id">The id number of the dialogue option ("xxx>1000" - the number part)</param>
	public static void StartDialogue(string dialogueGroup, int id) {

	}

	public static void StartDialogue(Dialogue dialogue, string actor1, string actor2 = "") {
		currentDialogue = dialogue;
		currentDialogue.Start();

		Speechbubble.ProcessText(currentDialogue);
	}


	public static void SelectOption(int option) {
		if (!IsDialogueActive)
			return;

		currentDialogue.SelectOption(option);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="dialogueGroup">Like "Bank", or "Makl".
	/// The first part of a localized string ("xxx>1000" - the xxx part).</param>
	/// <param name="id">The id number of the dialogue option ("xxx>1000" - the number part)</param>
	/// <param name="fromID">The possible answer dialogue options, in the same dialogue group starting from "fromID" and going to "toID".</param>
	/// <param name="toID">The possible answer dialogue options, in the same dialogue group starting from "fromID" and going to "toID".</param>
	public static void StartDialogue(string dialogueGroup, int id, int fromID, int toID) {

	}
}

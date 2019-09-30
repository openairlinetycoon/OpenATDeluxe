using Godot;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

	static DialogueStates state = DialogueStates.Other;

	public enum DialogueStates {
		ReadingHead,
		PickingOptions,
		ReadingOptions,
		Other,
	}

	private static DialogueWindow _speechbubble;

	/**
	** Dialogue:
	** Actor says something -> Speechbubble for actor with node's own text
	** Is actor done with talking? -> Speechbubble with options for answer in node's options
	** Actor answers -> Speechbubble just with the picked node option text
	** Performing action
	** Repeat
	**/

	/// <summary>
	//NOT IMPLEMENTED YET!
	/// </summary>
	/// <param name="dialogueGroup">Like "Bank", or "Makl".
	/// The first part of a localized string ("xxx>1000" - the xxx part).</param>
	/// <param name="id">The id number of the dialogue option ("xxx>1000" - the number part)</param>
	public static void StartDialogue(string dialogueGroup, int id) {

	}
	public static string GetFullTrText(int id, Dialogue dialogue) {
		return TranslationServer.Translate(dialogue.dialogueGroup + ">" + id);
	}

	public static void StartDialogue(Dialogue dialogue, string actor1, string actor2 = "") {
		currentDialogue = dialogue;
		currentDialogue.Start();

		Speechbubble.PrepareBubbleHeadText(0, dialogue);
		state = DialogueStates.ReadingHead;
		StartDialogueHeadSpeech();
	}
	public static string GetInstruction(string text) {
		string pattern = @"\[\[(.*)\]\]";

		string instruction = Regex.Match(text, pattern).Groups[1].Value;

		//Player 1 is used as placeholder in the localization. We have to change that to the current player
		instruction = instruction.Replace("P1", $"P{GameController.playerID}");

		return instruction;
	}
	private static void StartDialogueHeadSpeech() {
		void WaitForSpeechToFinish() {
			//Make Speech!

			System.Threading.Thread.Sleep(2000); //!DUMMY SLEEP
			Speechbubble.PrepareBubbleOptionsText(0, currentDialogue);
		}
		SoundPlayer speech
		 = SoundPlayer.CreatePlayer($"/VOICE/{GetInstruction(GetFullTrText(currentDialogue.CurrentNode.textId, currentDialogue))}.raw", "language", false);//P1\BA\3014


		instance.AddChild(speech);
		speech.Play();

		Task.Run(() => WaitForSpeechToFinish());
	}

	private static void StartDialogueAnswerSpeech(int optionIndex) {
		void WaitForSpeechToFinish() {
			//Make Speech!

			System.Threading.Thread.Sleep(2000); //!DUMMY SLEEP
			currentDialogue.SelectOption(optionIndex);
		}
		SoundPlayer speech
		 = SoundPlayer.CreatePlayer($"/VOICE/{GetInstruction(GetFullTrText(currentDialogue.CurrentNode.options[optionIndex].textId, currentDialogue))}.raw", "language", false);//P1\BA\3014


		instance.AddChild(speech);
		speech.Play();

		Task.Run(() => WaitForSpeechToFinish());
	}


	/// <summary>
	/// NOT IMPLEMENTED YET!
	/// </summary>
	/// <param name="dialogueGroup">Like "Bank", or "Makl".
	/// The first part of a localized string ("xxx>1000" - the xxx part).</param>
	/// <param name="id">The id number of the dialogue option ("xxx>1000" - the number part)</param>
	/// <param name="fromID">The possible answer dialogue options, in the same dialogue group starting from "fromID" and going to "toID".</param>
	/// <param name="toID">The possible answer dialogue options, in the same dialogue group starting from "fromID" and going to "toID".</param>
	public static void StartDialogue(string dialogueGroup, int id, int fromID, int toID) {

	}

	public static void SelectOption(int option) {
		if (!IsDialogueActive)
			return;
		GD.Print($"OPTION {option}");

		Speechbubble.PrepareBubbleAnswerText(option, 0, currentDialogue);


		StartDialogueAnswerSpeech(option);
		//currentDialogue.SelectOption(option);
	}


}

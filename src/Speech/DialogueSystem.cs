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

	public static string currentlyTalking;

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
		ReadingPickedOption,
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

		ReadNextNodeHead(dialogue);
	}

	private static void ReadNextNodeHead(Dialogue dialogue) {
		if (IsDialogueActive == false) {
			Speechbubble.Hide();

			return;//TODO: Delete/Hide Speechbubble
		}

		Speechbubble.Show();

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
	public static string GetInstructionActor(string text) {
		string pattern = @"(.*)\\"; //B2\109

		string actor = Regex.Match(text, pattern).Groups[1].Value;

		//Player 1 is used as placeholder in the localization. We have to change that to the current player
		actor = actor.Replace("P1", $"P{GameController.playerID}");

		return actor;
	}

	private static void CreateSoundAndWaitForFinish(string fileName) {
		SoundPlayer speech
		= SoundPlayer.CreatePlayer($"/VOICE/{fileName}.raw", "language", false);//P1\BA\3014


		instance.AddChild(speech);
		speech.Play();
		while (speech.Playing) { }
		//TODO: Find a CPU friendlier way of waiting
		//TODO: Add multiple sound instructions
		speech.QueueFree();
	}

	/// <summary>
	/// Starts the sound for the dialogue
	/// </summary>
	private static void StartDialogueHeadSpeech() {
		void WaitForSpeechToFinish() {
			string fileName = GetInstruction(GetFullTrText(currentDialogue.CurrentNode.textId, currentDialogue));
			currentlyTalking = GetInstructionActor(fileName);
			CreateSoundAndWaitForFinish(fileName);
			currentlyTalking = "";

			currentDialogue.CurrentNode.OnSpeechFinished();

			if (currentDialogue.CurrentNode.options.Count == 0) { //No options often means returning Node.
				ReadNextNodeHead(currentDialogue);
			} else {
				state = DialogueStates.PickingOptions;
				Speechbubble.PrepareBubbleOptionsText(0, currentDialogue);
			}
		}

		Task.Run(() => WaitForSpeechToFinish());
	}

	private static void StartDialogueAnswerSpeech(int optionIndex) {
		void WaitForSpeechToFinish() {
			string fileName = GetInstruction(GetFullTrText(currentDialogue.CurrentNode.options[optionIndex].textId, currentDialogue));
			currentlyTalking = GetInstructionActor(fileName);
			CreateSoundAndWaitForFinish(fileName);
			currentlyTalking = "";

			currentDialogue.CurrentNode.OnSpeechFinished();
			currentDialogue.SelectOption(optionIndex);

			ReadNextNodeHead(currentDialogue);
		}

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

	public static void StopDialogue() {
		currentDialogue = null;
	}


	public static void SelectOption(int option) {
		if (!IsDialogueActive)
			return;
		GD.Print($"OPTION {option}");

		state = DialogueStates.ReadingPickedOption;
		Speechbubble.PrepareBubbleAnswerText(option, 0, currentDialogue);
		StartDialogueAnswerSpeech(option);

		//currentDialogue.SelectOption(option);
	}


}

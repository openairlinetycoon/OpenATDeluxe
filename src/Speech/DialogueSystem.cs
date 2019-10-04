using Godot;
using System;
using System.Collections.Generic;
using System.Text;
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

	public static List<string> GetInstruction(string text) {

		string pattern = @"\[\[([^\]]*)\]\]";

		List<string> instructions = new List<string>();

		foreach (Match m in Regex.Matches(text, pattern)) {
			string actor = m.Groups[1].Value;
			actor = actor.Replace("P1", $"P{GameController.playerID}"); //Player 1 is used as placeholder in the localization. We have to change that to the current player

			instructions.Add(actor);
		}


		return instructions;
	}
	public static string GetInstructionActor(string text) {
		string pattern = @"([\w]*)\\"; //B2

		string actor = Regex.Match(text, pattern).Groups[1].Value;

		//Player 1 is used as placeholder in the localization. We have to change that to the current player
		actor = actor.Replace("P1", $"P{GameController.playerID}");

		return actor;
	}

	private static void CreateSoundAndWaitForFinish(List<string> instructions, string currentFullText) {

		int index = 0;
		foreach (string fileName in instructions) {
			if (fileName == "*") {
				if (currentlyTalking == "") {
					GD.PushError("Trying to create a speech wildcard failed! currentlyTalking was empty!");
					return;
				}
				int playerName = Regex.Matches(currentFullText, "]]")[index].Index;
			}
			SoundPlayer speech
			= SoundPlayer.CreatePlayer($"/VOICE/{fileName}.raw", "language", false);//P1\BA\3014
			instance.AddChild(speech);

			speech.Play();
			while (speech.Playing) { }
			//TODO: Find a CPU friendlier way of waiting
			//TODO: Add multiple sound instructions -- Coming!
			speech.QueueFree();

			index++;
		}
	}

	/// <summary>
	/// Needs currentlyTalking to be defined!
	/// </summary>
	/// <param name="fullText"></param>
	/// <param name="values"></param>
	/// <returns></returns>
	public static string FillWildcards(string fullText, params string[] values) {
		MatchCollection speechWildcards = Regex.Matches(fullText, @"\[\[([^\]]*)\]\] (%s)");
		var aStringBuilder = new StringBuilder(fullText);

		int index = 0;
		int offset = 0;
		foreach (Match m in Regex.Matches(fullText, @"%[a-z]*")) {

			if (values.Length < index) {
				GD.PushError("Not enough wildcards given!");
				break;
			}

			Match sWildcard = IsSpeechWildcard(m.Index); //These indices don't change.
			if (sWildcard != null) {
				int playerID = IsPlayer(values[index]);
				if (playerID != -1) {

					int speechPos = sWildcard.Groups[1].Index + offset;
					string replacementSpeech = $"{currentlyTalking}\\name{playerID}";

					aStringBuilder.Remove(speechPos, 1);
					aStringBuilder.Insert(speechPos, replacementSpeech);

					offset += replacementSpeech.Length - 1;
					int wildcardPos = m.Index + offset;

					aStringBuilder.Remove(wildcardPos, 2);
					aStringBuilder.Insert(wildcardPos, values[index]);
					offset += values[index].Length - 2;

				} else {
					GD.PushError("speech wildcard method not implemented yet! Following string: " + values[index]);
				}
			} else {
				int wildcardPos = m.Index + offset;

				aStringBuilder.Remove(wildcardPos, 2);
				aStringBuilder.Insert(wildcardPos, values[index]);
				offset += values[index].Length - 2;
			}

			index++;
		}

		fullText = aStringBuilder.ToString();
		return fullText;

		Match IsSpeechWildcard(int wildcardPos) {
			foreach (Match m in speechWildcards) {
				if (m.Groups[2].Index == wildcardPos)
					return m;
			}

			return null;
		}

		int IsPlayer(string playerName) {
			playerName = playerName.TrimStart(' ');
			for (int i = 0; i < GameController.playerCompanyNames.Length; i++) {
				string player = GameController.playerCompanyNames[i];

				if (playerName.BeginsWith(player))
					return i + 1;
			}

			return -1;
		}
	}

	/// <summary>
	/// Starts the sound for the dialogue
	/// </summary>
	private static void StartDialogueHeadSpeech() {
		void WaitForSpeechToFinish(string fullText, List<string> instr) {


			CreateSoundAndWaitForFinish(instr, fullText);
			currentlyTalking = "";

			currentDialogue.CurrentNode.OnSpeechFinished();

			if (currentDialogue.CurrentNode.options.Count == 0) { //No options often means returning Node.
				ReadNextNodeHead(currentDialogue);
			} else {
				state = DialogueStates.PickingOptions;
				Speechbubble.PrepareBubbleOptionsText(0, currentDialogue);
			}
		}

		string currentFullText = GetFullTrText(currentDialogue.CurrentNode.textId, currentDialogue);
		List<string> instructions = GetInstruction(currentFullText);
		currentlyTalking = GetInstructionActor(instructions[0]);

		if (currentDialogue.CurrentNode.wildcards != null) {
			currentFullText = FillWildcards(currentFullText, currentDialogue.CurrentNode.wildcards);
			instructions = GetInstruction(currentFullText); //Get the new set of instructions, in case there were wildcards replaced
		}

		Task.Run(() => WaitForSpeechToFinish(currentFullText, instructions));
	}

	private static void StartDialogueAnswerSpeech(int optionIndex) {
		void WaitForSpeechToFinish(string fullText, List<string> instr) {

			CreateSoundAndWaitForFinish(instr, fullText);
			currentlyTalking = "";

			currentDialogue.CurrentNode.OnSpeechFinished();
			currentDialogue.SelectOption(optionIndex);

			ReadNextNodeHead(currentDialogue);
		}

		string currentFullText = GetFullTrText(currentDialogue.CurrentNode.options[optionIndex].textId, currentDialogue);
		List<string> instructions = GetInstruction(currentFullText);
		currentlyTalking = GetInstructionActor(instructions[0]);

		if (currentDialogue.CurrentNode.options[optionIndex].wildcards != null) {
			currentFullText = FillWildcards(currentFullText, currentDialogue.CurrentNode.options[optionIndex].wildcards);
			instructions = GetInstruction(currentFullText); //Get the new set of instructions, in case there were wildcards replaced
		}

		Task.Run(() => WaitForSpeechToFinish(currentFullText, instructions));
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

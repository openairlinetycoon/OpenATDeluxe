using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
//using System.Threading.Tasks;
using Environment = System.Environment;

//TODO: Add skip function
public class DialogueSystem : Node2D {
	static DialogueSystem instance;
	public static Queue<Action> dialogueCommandQueue = new Queue<Action>();

	public static Dictionary<string, Actor> actors = new Dictionary<string, Actor>();

	public static Action onDialogueStart;

	override public void _Ready() {
		if (instance != null) {
			instance = this;
			return;
		}
		instance = this;

		RoomManager.OnRoomExit += CleanActors;

		AddPlayerActor();
	}


	override public void _Process(float _dt) {
		if (dialogueCommandQueue?.Count != 0) {
			dialogueCommandQueue.Dequeue().Invoke();
		}
	}

	public static Dialogue currentDialogue;
	public static bool IsDialogueActive {
		get {
			return currentDialogue != null;
		}
	}

	public static string currentlyTalking;

	public static bool skipHead;

	static DialogueWindow _speechbubble;
	public static DialogueWindow Speechbubble {
		get {
			if (_speechbubble != null)
				_speechbubble.Hide();

			_speechbubble = GetCurrentActorSpeechbubble();
			_speechbubble.Show();

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

	/**
	** Dialogue:
	** Actor says something -> Speechbubble for actor with node's own text
	** Is actor done with talking? -> Speechbubble with options for answer in node's options
	** Actor answers -> Speechbubble just with the picked node option text
	** Performing action
	** Repeat
	**/

	public static void AddActor(Actor actor) {
		if (actors.ContainsKey(actor.name)) {
			GD.Print($"{actor.name} is already added!");
			return;
		}
		actors.Add(actor.name, actor);
	}

	public static Actor GetCurrentActor() {
		string actor = currentlyTalking;
		if (!actors.ContainsKey(actor)) { //Fallback
			actor = GameController.CurrentPlayerTag;
			GD.Print($"No speechbubble found for actor: {currentlyTalking}! Using player actor instead");
		}

		return actors[actor];
	}

	private static void AddPlayerActor() {
		AddActor(new Actor(GameController.CurrentPlayerTag, (DialogueWindow)instance.FindNode("PL")));
	}
	public static void CleanActors() {
		if (instance == null)
			return;

		actors.Clear();
		AddPlayerActor();
	}

	public static DialogueWindow GetCurrentActorSpeechbubble() {
		return GetCurrentActor().speechbubble;
	}

	public static string GetFullTrText(int id, Dialogue dialogue) {
		return TranslationServer.Translate(dialogue.dialogueGroup + ">" + id);
	}

	/// <summary>
	/// NOT IMPLEMENTED YET!
	/// </summary>
	/// <param name="dialogueGroup">Like "Bank", or "Makl".
	/// The first part of a localized string ("xxx>1000" - the xxx part).</param>
	/// <param name="id">The id number of the dialogue option ("xxx>1000" - the number part)</param>
	public static void StartDialogue(string dialogueGroup, int id) {

	}

	public static void StartDialogue(Dialogue dialogue, string actor1, string actor2) {
		PrepareDialogue(dialogue, actor1, actor2);

		ReadNextNodeHead(dialogue);
	}

	public static void PrepareDialogue(Dialogue dialogue, string actor1, string actor2) {
		if (currentDialogue != null)
			return;

		currentDialogue = dialogue;
		currentDialogue.Start();
		onDialogueStart?.Invoke();

		currentlyTalking = actor2;
	}

	public static void StartCurrentDialogue() {
		ReadNextNodeHead(currentDialogue);
	}
	public static void StartWithOptions() {
		currentlyTalking = GameController.CurrentPlayerTag; //We just assume, that only the player can pick options
		Speechbubble.PrepareBubbleOptionsText(0, currentDialogue);
	}

	private static void ReadNextNodeHead(Dialogue dialogue) {
		if (IsDialogueActive == false) {
			Speechbubble.Hide();

			return;//TODO: Delete/Hide Speechbubble
		}

		StartDialogueHeadSpeech();
		Speechbubble.Show();

		Speechbubble.PrepareBubbleHeadText(0, dialogue);
		state = DialogueStates.ReadingHead;
	}

	public static List<string> GetInstruction(string text) {

		string pattern = @"\[\[([^\]]*)\]\]";

		List<string> instructions = new List<string>();

		foreach (Match m in Regex.Matches(text, pattern)) {
			string actor = m.Groups[1].Value;
			actor = actor.Replace("P1", GameController.CurrentPlayerTag); //Player 1 is used as placeholder in the localization. We have to change that to the current player

			instructions.Add(actor);
		}


		return instructions;
	}
	public static string GetInstructionActor(string text) {
		string pattern = @"([\w]*)\\"; //B2

		string actor = Regex.Match(text, pattern).Groups[1].Value;

		//Player 1 is used as placeholder in the localization. We have to change that to the current player
		actor = actor.Replace("P1", GameController.CurrentPlayerTag);

		return actor;
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

					aStringBuilder.Remove(wildcardPos, m.Groups[1].Length);
					aStringBuilder.Insert(wildcardPos, values[index]);
					offset += values[index].Length - m.Groups[1].Length;

				} else {
					GD.PushError("speech wildcard method not implemented yet! Following string: " + values[index]);
				}
			} else {
				int wildcardPos = m.Index + offset;

				aStringBuilder.Remove(wildcardPos, m.Groups[1].Length);
				aStringBuilder.Insert(wildcardPos, values[index]);
				offset += values[index].Length - m.Groups[1].Length;
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
	private static void CreateSoundsAndExecuteOnFinish(List<string> instructions, string currentFullText, Action onFinish) {
		List<SoundPlayer> player = new List<SoundPlayer>();

		onFinish += Speechbubble.OnStopTalking;

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

			player.Add(speech);
		}

		if (player.Count == 0) {
			onFinish.Invoke();
			return;
		}

		for (int i = 0; i < player.Count - 1; i++) {
			int lambda = i; //lambdas store the variable, not the value on creation. We store the value here to reference it later
			player[i].OnSoundFinished += () => { player[lambda].QueueFree(); player[lambda + 1].Play(); };
		}
		//We make an exception to the last entry to call our onFinish method, instead of trying to play the non existant next voice line 
		player[player.Count - 1].OnSoundFinished += () => { player[player.Count - 1].QueueFree(); onFinish.Invoke(); };

		SoundPlayer soundPlayer = player.FirstOrDefault();
		soundPlayer?.Play();

		Speechbubble.OnStartTalking();
	}

	private static void CompileTextAndInstructions(string[] wildcards, int textId, out string currentFullText, out List<string> instructions) {
		currentFullText = GetFullTrText(textId, currentDialogue);
		instructions = GetInstruction(currentFullText);
		currentlyTalking = GetInstructionActor(instructions[0]);

		if (wildcards != null) {
			currentFullText = FillWildcards(currentFullText, wildcards);
			instructions = GetInstruction(currentFullText); //Get the new set of instructions, in case there were wildcards replaced
		}
	}

	/// <summary>
	/// Starts the sound for the dialogue
	/// </summary>
	private static void StartDialogueHeadSpeech() {
		string[] wildcards = currentDialogue.CurrentNode.wildcards;
		int textId = currentDialogue.CurrentNode.textId;

		string currentFullText;
		List<string> instructions;
		CompileTextAndInstructions(wildcards, textId, out currentFullText, out instructions);

		dialogueCommandQueue.Enqueue(
			() => CreateSoundsAndExecuteOnFinish(instructions, currentFullText, () => {
				currentDialogue.CurrentNode.OnSpeechFinished();

				state = DialogueStates.PickingOptions;
				currentlyTalking = GameController.CurrentPlayerTag; //We just assume, that only the player can pick options
				Speechbubble.PrepareBubbleOptionsText(0, currentDialogue);
			}));
		//Task.Run(() => WaitForSpeechToFinish(currentFullText, instructions));
	}

	private static void StartDialogueAnswerSpeech(int optionIndex) {
		string[] wildcards = currentDialogue.CurrentNode.options[optionIndex].wildcards;
		int textId = currentDialogue.CurrentNode.options[optionIndex].textId;

		string currentFullText;
		List<string> instructions;
		CompileTextAndInstructions(wildcards, textId, out currentFullText, out instructions);

		dialogueCommandQueue.Enqueue(
			() => CreateSoundsAndExecuteOnFinish(instructions, currentFullText, () => {

				currentDialogue.CurrentNode.OnSpeechFinished();
				currentDialogue.SelectOption(optionIndex);

				ReadNextNodeHead(currentDialogue);
			}));

		//Task.Run(() => WaitForSpeechToFinish(currentFullText, instructions));
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
		dialogueCommandQueue.Clear();
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

public struct Actor {
	public string name;
	public DialogueWindow speechbubble;

	public Actor(string name, DialogueWindow speechbubble) {
		this.name = name;
		this.speechbubble = speechbubble;
	}
}


/* 
StartDialogue called
Create sounds
Play each sound
Create next Node
*/

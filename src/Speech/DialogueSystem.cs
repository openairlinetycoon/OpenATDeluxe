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

	public static SoundPlayer currentSound;
	public static Viewport otherRoomParent; //For telephone call
	public static Node2D otherRoomHolder;
	public static bool isTelephoneCall;

	override public void _Ready() {
		if (instance != null) {
			instance = this;
			return;
		}
		instance = this;

		otherRoomParent = (Viewport)FindNode("OtherTelephoneRoom");
		otherRoomHolder = ((Node2D)otherRoomParent.GetParent());
		otherRoomHolder.Hide();

		RoomManager.OnRoomExit += CleanActors;
		GameController.onUnhandledInput += Skip;

		AddPlayerActor();
	}


	override public void _Process(float _dt) {
		if (dialogueCommandQueue?.Count != 0) {
			dialogueCommandQueue.Dequeue().Invoke();
		}
	}

	public static Dictionary<string, Dialogue> registeredDialogues = new Dictionary<string, Dialogue>();
	public static Dialogue currentDialogue;
	public static bool IsDialogueActive => currentDialogue != null;

	public static string currentlyTalking;

	public static string actorA, actorB;

	public static bool skipHead;

	static DialogueWindow _speechbubble;
	public static DialogueWindow Speechbubble {
		get {
			if (IsInstanceValid(_speechbubble))
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
		Prepared,
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

	private static void SetCurrentDialogue(Dialogue dialogue) {
		currentDialogue = dialogue;
		InteractionLayerManager.DisableAllLayersButOne((int)BaseLayer.Elements);
		//GameController.canPlayerInteract = false;
	}

	public static void AddActor(Actor actor, bool replaceExisting = true) {
		if (actors.ContainsKey(actor.name)) {
			if (replaceExisting) {
				actors[actor.name] = actor;
				return;
			}else {
				GD.Print($"{actor.name} is already added! Use \"OverriderActor(...)\" to replace it instead!");
				return;
			}
		}
		actors.Add(actor.name, actor);
	}

	public static void OverrideActor(Actor newActor) {
		if (actors.ContainsKey(newActor.name)) {
			actors[newActor.name] = newActor;
			return;
		}
		
		AddActor(newActor);
	}

	public static Actor GetCurrentActor() {
		string actor = currentlyTalking;
		if (!actors.ContainsKey(actor)) {
			actor = GameController.CurrentPlayerTag; //Fallback
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

	private static DialogueWindow GetCurrentActorSpeechbubble() {
		return GetCurrentActor().speechbubble;
	}

	private static string GetFullTrText(int id, Dialogue dialogue) {
		return TranslationServer.Translate(dialogue.dialogueGroup + ">" + id);
	}

	/// <summary>
	/// Registers the Dialogue to the Dialogue System. This is needed to start the Dialogue with StartDialogue(string).
	/// This function is called in the constructor of the Dialogue class and shouldn't be called again.
	/// </summary>
	/// <param name="dialogue">The dialogue to register</param>
	/// <param name="id">A string identifying it.</param>
	public static void RegisterDialogue(Dialogue dialogue, string id) {
		if (registeredDialogues.ContainsKey(id)) {
			registeredDialogues.Remove(id);
		}
		registeredDialogues.Add(id, dialogue);
	}

	private static Node[] GetAllChildren(Node parent) {
		List<Node> nodes = new List<Node>();

		foreach (Node child in parent.GetChildren()) {
			if (child.GetChildCount() != 0) {
				GetAllChildrenRecursion(child, ref nodes);
			}

			nodes.Add(child);
		}

		return nodes.ToArray();
	}

	private static void GetAllChildrenRecursion(Node parent, ref List<Node> nodes) {
		foreach (Node child in parent.GetChildren()) {
			if (child.GetChildCount() != 0) {
				GetAllChildrenRecursion(child, ref nodes);
			}

			nodes.Add(child);
		}
	}

	/// <summary>
	/// Prepares and starts a phone call. It instantiates the called room and starts the dialogue 
	/// </summary>
	/// <param name="dialogueRoom">The ID of the room. eg. "RoomBank" or "RoomCafe" - see the scenes/rooms folder</param>
	/// <param name="dialogueName">The ID of the dialogue to start</param>
	public static void PrepareTelephoneCall(string dialogueRoom, string dialogueName) {
		//Load Room to the base canvas with the light
		foreach (Node2D child in otherRoomParent.GetChildren()) {
			child.QueueFree();
		}

		Node2D newRoom = RoomManager.GetRoomInstance(dialogueRoom);
		otherRoomParent.AddChild(newRoom);

		isTelephoneCall = true;
		PrepareDialogue(registeredDialogues[dialogueName]);

		Vector2 pushForCorrectPosition = Vector2.Right * GetCurrentActor().horizontalPushForCall;
		DialogueWindow dialogueWindow = GetCurrentActorSpeechbubble();

		dialogueWindow.HeadPosition += pushForCorrectPosition;
		if (dialogueWindow.GetParent() != instance) {
			dialogueWindow.GetParent().RemoveChild(dialogueWindow);
			instance.AddChild(dialogueWindow);
		}

		newRoom.Position = pushForCorrectPosition;
		otherRoomHolder.Hide();
	}

	public static void StartPreparedTelephoneCall() {
		StartCurrentDialogue();
		otherRoomHolder.Show();
	}

	/// <summary>
	/// Prepares everything for the Dialogue and begins to read its first Node head. It will not override an already running dialogue.
	/// </summary>
	/// <param name="dialogueId">The ID of the dialogue to start</param>
	public static void StartDialogue(string dialogueId) {
		StartDialogue(registeredDialogues[dialogueId]);
	}

	/// <summary>
	/// Prepares everything for the Dialogue and begins to read its first Node head. It will not override an already running dialogue.
	/// </summary>
	/// <param name="dialogue">What dialogue to start</param>
	public static void StartDialogue(Dialogue dialogue) {
		if (currentDialogue != null) {
			return;
		}

		PrepareDialogue(dialogue);

		ReadNextNodeHead(dialogue);
	}

	/// <summary>
	/// Defines the internal actors according to dialogue specification mars the dialogue as started. It also invokes the onDialogueStart of the dialogue. It will not prepare, when another dialogue is still running.
	/// </summary>
	/// <param name="dialogue">What dialogue to prepare</param>
	public static void PrepareDialogue(Dialogue dialogue) {
		if (currentDialogue != null)
			return;

		actorA = "P2";
		actorB = dialogue.partnerID;

		SetCurrentDialogue(dialogue);
		currentDialogue.Start();
		onDialogueStart?.Invoke();

		state = DialogueStates.Prepared;

		currentlyTalking = actorB;
	}

	public static void PrepareMonologue(Dialogue dialogue) {
		if (currentDialogue != null)
			return;

		actorA = "P2";
		actorB = dialogue.partnerID;

		SetCurrentDialogue(dialogue);
		currentDialogue.Start();
		//onDialogueStart?.Invoke(); //!Make check for player speechbubble

		state = DialogueStates.Prepared;

		currentlyTalking = actorB;
	}


	/// <summary>
	/// Starts the prepared dialogue.
	/// </summary>
	public static void StartCurrentDialogue() {
		if (state == DialogueStates.Prepared)
			ReadNextNodeHead(currentDialogue);
	}

	/// <summary>
	/// Skips the Node head of the dialogue and shows the options instead.
	/// </summary>
	public static void StartWithOptions() {
		currentlyTalking = GameController.CurrentPlayerTag; //We just assume, that only the player can pick options
		Speechbubble.PrepareBubbleOptionsText(0, currentDialogue);
	}

	/// <summary>
	/// Starts the audio playback of the current node head and prepares its rendering
	/// </summary>
	/// <param name="dialogue">What dialogue to read</param>
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

	/// <summary>
	/// Scans the text for any instructions inside of [[*]]. Replaces all occurrences of "P1" with the currently player tag
	/// </summary>
	/// <returns>A list of all instructions in the text, without braces</returns>
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

	/// <summary>
	/// Gets the first talking actor of the text. Replaces "P1" with the current player tag.
	/// </summary>
	/// <param name="text"></param>
	/// <returns>The actor ID</returns>
	public static string GetInstructionActor(string text) {
		string pattern = @"([\w]*)\\"; //B2

		string actor = Regex.Match(text, pattern).Groups[1].Value;

		//Player 1 is used as placeholder in the localization. We have to change that to the current player
		actor = actor.Replace("P1", GameController.CurrentPlayerTag);

		if (currentDialogue.enforceActors) {
			actor = currentlyTalking == actorA ? actorB : actorA;
		}

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

	/// <summary>
	/// Creates all necessary SoundPlayer for every given instruction and links the them together in a "playlist" for continuos playback
	/// </summary>
	/// <param name="instructions">A list of file path instructions. eg. "BA\\900" -> Bank stock person with voiceline 900</param>
	/// <param name="currentFullText"></param>
	/// <param name="onFinish"></param>
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
			player[i].OnSoundFinished += () => { player[lambda].QueueFree(); currentSound = player[lambda + 1]; currentSound.Play(); };
		}
		//We make an exception to the last entry to call our onFinish method, instead of trying to play the non existant next voice line 
		player[player.Count - 1].OnSoundFinished += () => { player[player.Count - 1]?.QueueFree(); currentSound = null; onFinish?.Invoke(); };

		currentSound = player.FirstOrDefault();
		currentSound?.Play();

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
				
				if(currentDialogue?.CurrentNode == null)
					return; //We could stop the current dialogue as a side effect from the OnSpeechFinish(), that's why we leave
				
				if (currentDialogue.CurrentNode.HasFollowup()) {
					state = DialogueStates.ReadingHead;
					currentDialogue.CurrentNode.GoToFollowup();

					ReadNextNodeHead(currentDialogue);
				} else {
					ShowOptions();
				}
			}));
		//Task.Run(() => WaitForSpeechToFinish(currentFullText, instructions));
	}

	private static void ShowOptions() {
		if (IsDialogueActive == false) {
			Speechbubble.Hide();

			return;//TODO: Delete/Hide Speechbubble
		}

		state = DialogueStates.PickingOptions;
		currentlyTalking = GameController.CurrentPlayerTag; //We just assume, that only the player can pick options
		Speechbubble.PrepareBubbleOptionsText(0, currentDialogue);
	}

	private static void StartDialogueAnswerSpeech(int optionIndex) {
		string[] wildcards = currentDialogue.CurrentNode.options[optionIndex].wildcards;
		int textId = currentDialogue.CurrentNode.options[optionIndex].TextId;

		string currentFullText;
		List<string> instructions;
		CompileTextAndInstructions(wildcards, textId, out currentFullText, out instructions);

		dialogueCommandQueue.Enqueue(
			() => CreateSoundsAndExecuteOnFinish(instructions, currentFullText, () => {

				bool noTalking = currentDialogue.CurrentNode.options[optionIndex] is DialogueOptionReturning;
				currentDialogue.CurrentNode.OnSpeechFinished();
				currentDialogue.SelectOption(optionIndex);

				if (noTalking)
					ShowOptions();
				else
					ReadNextNodeHead(currentDialogue);
			}));

		//Task.Run(() => WaitForSpeechToFinish(currentFullText, instructions));
	}

	public static void Skip() {
		currentSound?.Stop();
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
		Speechbubble?.Hide();
		
		currentDialogue = null;
		currentlyTalking = "";
		InteractionLayerManager.EnableAllLayers();
		//GameController.canPlayerInteract = true;

		dialogueCommandQueue.Clear();
		if (isTelephoneCall) {
			foreach (Node2D child in otherRoomParent.GetChildren()) {
				child.QueueFree();
			}
		}

		isTelephoneCall = false;
		((Node2D)otherRoomHolder).Hide();
	}


	public static void SelectOption(int option) {
		if (!IsDialogueActive && state != DialogueStates.PickingOptions)
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
	public float horizontalPushForCall;

	public Actor(string name, DialogueWindow speechbubble, float horizontalPushForCall = 0) {
		this.name = name;
		this.speechbubble = speechbubble;
		this.horizontalPushForCall = horizontalPushForCall;
	}
}


/* 
StartDialogue called
Create sounds
Play each sound
Create next Node
*/

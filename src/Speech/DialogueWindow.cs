using Godot;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

public class DialogueWindow : Control {
	//public Label textLabel;

	public VBoxContainer lineContainer;
	public MarginContainer container;

	public HBoxContainer speechbubble;
	public NinePatchRect rightTexture;

	public PackedScene linePrefab;
	public List<Control> lines;
	public List<Control> linesDebug;

	public TextureRect head;

	Vector2 baseSize, basePosition, headPosition;

	public string speechbubbleLinePrefab = "res://Prefabs/Speech/SpeechbubbleLinePrefab.tscn";

	public override void _Ready() {
		//textLabel = GetNode<Label>("Label");
		lineContainer = GetNode<VBoxContainer>("Content");

		// container = GetNode<MarginContainer>("Margin");
		lineContainer.Connect("resized", this, "OnContainerSizeChange");

		speechbubble = GetNode<HBoxContainer>("SpeechbubbleFlexible");
		rightTexture = GetNode<NinePatchRect>("SpeechbubbleFlexible/RightSide/Flip/Texture");
		head = GetNodeOrNull<TextureRect>("Head");

		linePrefab = (PackedScene)ResourceLoader.Load(speechbubbleLinePrefab);

		lines = new List<Control>();
		linesDebug = new List<Control>();

		baseSize = RectSize;
		basePosition = RectPosition;

		headPosition = head?.RectGlobalPosition ?? default(Vector2);

		this.Update();
		this.Hide();
	}

	public void OnContainerSizeChange() {
		Vector2 innerMargin = new Vector2(50, 10);
		//Add padding:
		speechbubble.RectPosition = lineContainer.RectPosition - innerMargin / 2;
		speechbubble.RectSize = lineContainer.RectSize + innerMargin;

		//Move to the right position, with the new margin in mind
		RectSize = baseSize - innerMargin;
		RectPosition = basePosition + innerMargin / 2;

		//Move the Speechbubble down a bit when the box is bigger than normal 
		//to mimmick the original behavior for longer player text. 
		if (DialogueSystem.currentlyTalking == GameController.CurrentPlayerTag) {
			float heightDifference = speechbubble.RectSize.y - 61;
			heightDifference = Mathf.Min(heightDifference, 140);

			RectPosition = RectPosition + new Vector2(0, heightDifference) / 2;
		}

		head?.SetGlobalPosition(headPosition);

		//Force redraw and repositioning
		speechbubble.Hide();
		speechbubble.Show();
	}

	public class OptionLine : Control {
		public int option;

		public override void _Ready() {
			Connect("mouse_entered", this, nameof(MouseEntered));
			Connect("mouse_exited", this, nameof(MouseExited));
		}

		public void MouseEntered() {
			MouseCursor.instance?.MouseEnter(this);

			SoundPlayer p = SoundPlayer.CreatePlayer("/SOUND/change.raw", "effects", true, true);
			p.VolumeDb = 12;
			AddChild(p);
			p.Play();
		}
		public void MouseExited() {
			MouseCursor.instance?.MouseLeave(this);
		}
		public void OnClick() {
			MouseCursor.instance?.ChangeMouseState(MouseCursor.MouseState.Normal);
			DialogueSystem.SelectOption(option);
		}

		override public void _GuiInput(InputEvent e) {
			if (e is InputEventMouseButton mouse) {

				if (mouse.IsPressed()) {
					OnClick();
				}
			}
		}
	}

	// public Control AddLines(int numberOfLines, int option) {
	// 	OptionLine newLine = new OptionLine();
	// 	newLine.Name = "Line";
	// 	newLine.MouseFilter = MouseFilterEnum.Pass;

	// 	newLine.option = option;

	// 	//int lineHeight = textLabel.GetLineHeight() + 3; //Godot's Label have line spacing of 3 apparently
	// 	newLine.RectMinSize = new Vector2(0, lineHeight * numberOfLines);

	// 	lineContainer.AddChild(newLine);
	// 	lines.Add(newLine);


	// 	return newLine;
	// }

	public static string FillWildcards(string fullText, params string[] values) {
		var aStringBuilder = new StringBuilder(fullText);

		int index = 0;
		int offset = 0;
		foreach (Match m in Regex.Matches(fullText, @"%[a-z]*")) {
			int wildcardPos = m.Index + offset;

			aStringBuilder.Remove(wildcardPos, 2);//!Account for the actual size, not just %s -> e.g. %li 
			aStringBuilder.Insert(wildcardPos, values[index]);
			offset += values[index].Length - 2;

			index++;
		}

		fullText = aStringBuilder.ToString();
		return fullText;
	}

	public static string GetFullCleanTrText(int id, Dialogue dialogue, string[] wildcards = null) {
		string text = CleanOffInstruction(TranslationServer.Translate(dialogue.dialogueGroup + ">" + id));
		if (wildcards != null)
			text = FillWildcards(text, wildcards);
		return text;
	}

	public void PrepareBubbleHeadText(int currentActor, Dialogue dialogue) {
		ClearLines();

		lines.Clear();

		HBoxContainer line = (HBoxContainer)linePrefab.Instance();
		line.GetNode<Control>("Control").Visible = false;

		Label textLabel = line.GetNode<Label>("Label");

		lineContainer.AddChild(line);
		lines.Add(line);
		//TODO: Add positioning to dialogue actor

		string text = GetFullCleanTrText(dialogue.CurrentNode.textId, dialogue, dialogue.CurrentNode.wildcards);
		textLabel.CallDeferred("set_text", text);

		lineContainer.Hide();
		lineContainer.Show();
	}

	private void ClearLines() {
		if (lines != null) {
			foreach (Control l in lines) {
				l.Hide();
				l.CallDeferred("queue_free");
			}
			// lineContainer.RectSize = new Vector2(lineContainer.RectSize.x, 0);
			// lineContainer.RectPosition = new Vector2(0, 0);
		}
	}

	//TODO: Add positioning to dialogue actor
	public void PrepareBubbleOptionsText(int currentActor, Dialogue dialogue) {
		int lineCount = 1; //Start at 1 to compensate the first '\n' in the loop


		ClearLines();

		lines.Clear();
		linesDebug.Clear();

		string text = "";
		int optionIndex = 0;
		foreach (DialogueOption option in dialogue.CurrentNode.options) {
			HBoxContainer line = (HBoxContainer)linePrefab.Instance();

			Label textLabel = line.GetNode<Label>("Label");

			lineContainer.AddChild(line);
			lines.Add(line);

			OptionLine newLine = new OptionLine();
			newLine.Name = "Line";
			newLine.option = optionIndex;
			newLine.MouseFilter = MouseFilterEnum.Pass;
			newLine.RectMinSize = line.RectSize;
			newLine.RectPosition = line.RectPosition;
			linesDebug.Add(newLine);
			textLabel.AddChild(newLine);

			//text += "* ";
			text = GetFullCleanTrText(option.textId, dialogue, option.wildcards);
			//text += '\n';
			textLabel.CallDeferred("set_text", text);

			//AddLines(textLabel.GetLineCount() - lineCount, optionIndex);
			//lineCount = textLabel.GetLineCount();

			optionIndex++;
		}
		lineContainer.Hide();
		lineContainer.Show();
	}
	public void PrepareBubbleAnswerText(int optionIndex, int currentActor, Dialogue dialogue) {
		ClearLines();

		lines.Clear();
		HBoxContainer line = (HBoxContainer)linePrefab.Instance();
		line.GetNode<Control>("Control").Visible = false;

		Label textLabel = line.GetNode<Label>("Label");

		lineContainer.AddChild(line);
		lines.Add(line);

		string text = GetFullCleanTrText(dialogue.CurrentNode.options[optionIndex].textId, dialogue, dialogue.CurrentNode.options[optionIndex].wildcards);

		textLabel.CallDeferred("set_text", text);

		lineContainer.Hide();
		lineContainer.Show();
	}

	public virtual void OnStartTalking() {

	}
	public virtual void OnStopTalking() {

	}

	public static string CleanOffInstruction(string text) {
		string pattern = @" ?\[\[([^\]]*)\]\]";
		return Regex.Replace(text, pattern, "");
	}

	override public void _Process(float delta) {
		Update();
	}

	override public void _Draw() {
		// float i = 0;
		// foreach (Control c in linesDebug) {
		// 	i += 0.25f;
		// 	Rect2 r = c.GetRect();
		// 	DrawRect(r, Color.FromHsv(i, 1, 1, 0.25f), true);

		// }
	}
}

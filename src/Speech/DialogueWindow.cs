using Godot;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class DialogueWindow : Control {
	public Label textLabel;

	public VBoxContainer lineContainer;
	public Control linePrefab;
	public List<Control> lines;

	public override void _Ready() {
		textLabel = GetNode<Label>("Label");
		lineContainer = GetNode<VBoxContainer>("Label/Lines");

		lines = new List<Control>();
	}

	public class OptionLine : Control {
		public int option;

		public override void _Ready() {
			Connect("mouse_entered", this, nameof(MouseEntered));
			Connect("mouse_exited", this, nameof(MouseExited));
		}

		public void MouseEntered() {
			MouseCursor.instance?.MouseEnter(this);
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

	public Control AddLines(int numberOfLines, int option) {
		OptionLine newLine = new OptionLine();
		newLine.Name = "Line";
		newLine.MouseFilter = MouseFilterEnum.Pass;

		newLine.option = option;

		int lineHeight = textLabel.GetLineHeight() + 3; //Godot's Label have line spacing of 3 apparently
		newLine.RectMinSize = new Vector2(0, lineHeight * numberOfLines);

		lineContainer.AddChild(newLine);
		lines.Add(newLine);

		return newLine;
	}

	public static string GetFullCleanTrText(int id, Dialogue dialogue) {
		return CleanOffInstruction(TranslationServer.Translate(dialogue.dialogueGroup + ">" + id));
	}

	public void PrepareBubbleHeadText(int currentActor, Dialogue dialogue) {
		if (lines != null) {
			foreach (Control line in lines) {
				line.QueueFree();
			}
		}

		lines.Clear();

		//TODO: Add positioning to dialogue actor

		string text = GetFullCleanTrText(dialogue.CurrentNode.textId, dialogue);
		textLabel.Text = text;
	}

	//TODO: Add positioning to dialogue actor
	public void PrepareBubbleOptionsText(int currentActor, Dialogue dialogue) {
		int lineCount = 1; //Start at 1 to compensate the first '\n' in the loop

		if (lines != null) {
			foreach (Control line in lines) {
				line.QueueFree();
			}
		}

		lines.Clear();

		string text = "";
		int optionIndex = 0;
		foreach (DialogueOption option in dialogue.CurrentNode.options) {
			text += "* ";
			text += GetFullCleanTrText(option.textId, dialogue);
			text += '\n';
			textLabel.Text = text;

			AddLines(textLabel.GetLineCount() - lineCount, optionIndex);
			lineCount = textLabel.GetLineCount();

			optionIndex++;
		}

		text = text.TrimEnd('\n');
		textLabel.Text = text;
	}
	public void PrepareBubbleAnswerText(int optionIndex, int currentActor, Dialogue dialogue) {
		if (lines != null) {
			foreach (Control line in lines) {
				line.QueueFree();
			}
		}

		lines.Clear();

		string text = GetFullCleanTrText(dialogue.CurrentNode.options[optionIndex].textId, dialogue); ;
		textLabel.Text = text;

	}

	public static string CleanOffInstruction(string text) {
		string pattern = @"\[\[(.*)\]\]";
		return Regex.Replace(text, pattern, "");
	}

	override public void _Process(float delta) {
		Update();
	}

	override public void _Draw() {
		// float i = 0;
		// foreach (Control c in lines) {
		// 	i += 0.25f;
		// 	Rect2 r = c.GetRect();
		// 	DrawRect(r, Color.FromHsv(i, 1, 1, 0.25f), true);

		// }
	}
}

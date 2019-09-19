using Godot;
using System;

public class DialogueWindow : Node {
	public Label textLabel;

	public override void _Ready() {
		textLabel = GetNode<Label>("Label");
	}

	public void ProcessText(Dialogue dialogue) {
		string text = GetFullTrText(dialogue.CurrentNode.textId, dialogue);
		text += "\n";
		foreach (DialogueOption option in dialogue.CurrentNode.options) {
			text += "* ";
			text += GetFullTrText(option.textId, dialogue);
			text += "\n";

			//TODO: Add mouse "area" for option selection
		}

		textLabel.Text = text;
	}
	public string GetFullTrText(int id, Dialogue dialogue) {
		return TranslationServer.Translate(dialogue.dialogueGroup + ">" + id);
	}
	public string CleanOfInstruction(string text) {
		throw new NotImplementedException();
	}

}

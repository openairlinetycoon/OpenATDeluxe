using System;

public class DialogueOption {
	public int textId;

	public string[] wildcards;

	DialogueNode destination;

	public DialogueOption(int textId, DialogueNode destination, params string[] wildcards) {
		this.textId = textId;
		this.destination = destination;
		this.wildcards = wildcards;
	}

	public virtual DialogueNode GetDestinationNode() {
		return destination;
	}
}
public class DialogueOptionReturning : DialogueOption {
	public DialogueOptionReturning(int textId, params string[] wildcards) : base(textId, null, wildcards) {
	}

	override public DialogueNode GetDestinationNode() {
		return null;
	}
}

public class DialogueOptionConditioned : DialogueOption {
	public Func<DialogueNode> condition;

	public DialogueOptionConditioned(int textId, Func<DialogueNode> destinations, params string[] wildcards) : base(textId, null, wildcards) {
		this.condition = destinations;
	}

	public override DialogueNode GetDestinationNode() {
		throw new NotImplementedException();
	}
}
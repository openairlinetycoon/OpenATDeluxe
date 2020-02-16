using System;

public class DialogueOption {
	public int textId;

	public string[] wildcards;

	protected DialogueNode destination;

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

	override public DialogueNode GetDestinationNode() {
		return condition.Invoke();
	}
}

public class DialogueOptionTelephoneConditioned : DialogueOption {
	public Func<DialogueNode> condition;
	DialogueNodeReturning onTelephone;

	public DialogueOptionTelephoneConditioned(int textId, Func<DialogueNode> destinations, DialogueNodeReturning onTelephone = null, params string[] wildcards) : base(textId, null, wildcards) {
		this.condition = destinations;
		this.onTelephone = onTelephone;
	}

	override public DialogueNode GetDestinationNode() {
		return DialogueSystem.isTelephoneCall ? onTelephone : condition.Invoke();
	}
}

public class DialogueOptionTelephone : DialogueOption {
	DialogueNodeReturning onTelephone;
	
	public DialogueOptionTelephone(int textId, DialogueNode destination, DialogueNodeReturning onTelephone = null, params string[] wildcards) : base(textId, destination, wildcards) {
		this.onTelephone = onTelephone;
	}

	public override DialogueNode GetDestinationNode() {
		return DialogueSystem.isTelephoneCall ? onTelephone : destination;
	}
}
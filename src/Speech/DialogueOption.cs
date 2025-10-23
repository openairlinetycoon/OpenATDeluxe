using System;

public partial class DialogueOption {
	protected int textId;
	public virtual int TextId => textId;

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
public partial class DialogueOptionReturning : DialogueOption {
	public DialogueOptionReturning(int textId, params string[] wildcards) : base(textId, null, wildcards) {

	}

	override public DialogueNode GetDestinationNode() {
		return null;
	}
}

public partial class DialogueOptionConditioned : DialogueOption {
	public Func<DialogueNode> conditionDestination;
	public Func<int> conditionText;

	override public int TextId {
		get { return conditionText?.Invoke() ?? textId; }
	}

	public DialogueOptionConditioned(int textId, Func<DialogueNode> destinations, params string[] wildcards) : base(textId, null, wildcards) {
		this.conditionDestination = destinations;
	}
	public DialogueOptionConditioned(Func<int> text, Func<DialogueNode> destinations, params string[] wildcards) : base(0, null, wildcards) {
		this.conditionDestination = destinations;
		this.conditionText = text;
	}
	public DialogueOptionConditioned(Func<int> text, DialogueNode destination, params string[] wildcards) : base(0, destination, wildcards) {
		this.conditionText = text;
	}

	override public DialogueNode GetDestinationNode() {
		return conditionDestination?.Invoke() ?? destination;
	}
}

public partial class DialogueOptionTelephoneConditioned : DialogueOption {
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

public partial class DialogueOptionTelephone : DialogueOption {
	DialogueNodeReturning onTelephone;
	
	public DialogueOptionTelephone(int textId, DialogueNode destination, DialogueNodeReturning onTelephone = null, params string[] wildcards) : base(textId, destination, wildcards) {
		this.onTelephone = onTelephone;
	}

	public override DialogueNode GetDestinationNode() {
		return DialogueSystem.isTelephoneCall ? onTelephone : destination;
	}
}
using Godot;
using System;
using System.Collections.Generic;



public class Dialogue {
	public string dialogueGroup;
	public string id;
	public string partnerID;
	public bool enforceActors;
	DialogueNode _currentNode;
	public DialogueNode CurrentNode { get => _currentNode; private set => _currentNode = value; }
	public int CurrentNodeIndex { get => nodes.IndexOf(_currentNode); }

	public DialogueNode onTelephoneNode;

	// Used to return to previous dialogue nodes
	Stack<DialogueNode> dialogueStack = new Stack<DialogueNode>();
	List<DialogueNode> nodes = new List<DialogueNode>();

	public Dialogue(string dialogueGroup, string id, string partnerID = "", bool enforceActors = false) {
		this.dialogueGroup = dialogueGroup;
		this.id = id;
		this.partnerID = partnerID;
		this.enforceActors = enforceActors;

		DialogueSystem.RegisterDialogue(this, id);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="node"></param>
	/// <returns>Itself, good for stacking</returns>
	public Dialogue AddNode(DialogueNode node) {
		nodes.Add(node);

		return this;
	}

	public void AddOnTelephoneNode(DialogueNodeReturning onTelephoneNode) {
		this.onTelephoneNode = onTelephoneNode;
	}

	/// <summary>
	/// Starts the node which got added first.
	/// </summary>
	public void Start() {
		dialogueStack.Clear();
		CurrentNode = null;
		StartNode(nodes[0]);
	}

	public void StartNode(DialogueNode node, bool addCurrentNodeToStack = true) {
		if (CurrentNode != null)
			addCurrentNodeToStack = CurrentNode.returnable & addCurrentNodeToStack;

		//if (!nodes.Contains(node))
		//	throw new ArgumentOutOfRangeException("Node not inside Dialogue!");

		if (CurrentNode != null && addCurrentNodeToStack) {
			//Add old node to stack
			dialogueStack.Push(CurrentNode);
		}

		CurrentNode = node;

		CurrentNode.Start(this);
	}

	public void ReturnToPrevNode() {
		if (dialogueStack.Count == 0) {
			DialogueSystem.StopDialogue();
			return;
		}
		DialogueSystem.skipHead = true;

		DialogueNode prev = dialogueStack.Pop();

		StartNode(prev); //Opt out of Stack to prevent looping forth and back
	}

	public void SelectOption(int id) {
		//TODO: Add OutOfRange check!
		DialogueNode nextNode = CurrentNode.options[id].GetDestinationNode();
		if (CurrentNode.options[id] is DialogueOptionReturning) {
			ReturnToPrevNode();
		} else if (CurrentNode.options[id] is DialogueOptionTelephone && nextNode == null) {
			StartNode(onTelephoneNode); //We skip to the default when we are calling and the option didn't specify a custom node
		}else{
			StartNode(nextNode);
		}
	}
}
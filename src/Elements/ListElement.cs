using System;
using System.Collections.Generic;
using Godot;

public class ListElement : BaseElement {
	public List<ListItem> lines;

	[Export]
	public NodePath baseListItemPath;
	public Control baseListItem;

	[Export]
	public NodePath linePrefabPath;
	public Control linePrefab;


	[Export]
	public int maxLinesPerPage;

	[Export]
	public bool showNextPage;
	int page = 0;

	public bool itemsCloseList;


	public override void OnReady() {
		linePrefab = (Control)GetNode(linePrefabPath);
		baseListItem = (Control)GetNode(baseListItemPath);

		linePrefab.Visible = false;
		Visible = false;
		GameController.onUnhandledInput += OnUnhandledInput;
	}

	private void OnUnhandledInput()
	{
		if (Visible) HideElement();
	}

	public void ShowElement() {
		foreach (Node n in baseListItem.GetChildren()) {
			if (n == linePrefab)
				continue;

			n.QueueFree();
		}

		for (int i = 0 + page * maxLinesPerPage; i < lines.Count; i++) {
			Control line = (Control)linePrefab.Duplicate();
			line.Visible = true;

			line.Name = $"Item {i}";

			Label lineText;
			if (line is Label l) {
				lineText = l;
			} else {
				lineText = (Label)line.FindNode("Text");
			}

			lineText.Text = lines[i].text;
			line.Visible = lines[i].visible;

			LineElement element = new LineElement();
			element.Name = "Line";
			element.onClick += lines[i].onClick;
			element.MouseFilter = Control.MouseFilterEnum.Pass;
			element.RectMinSize = line.RectSize;
			element.RectPosition = line.RectPosition;

			if (itemsCloseList) {
				element.onClick += Hide;
			}

			line.AddChild(element);
			baseListItem.AddChild(line);
		}

		//GameController.canPlayerInteract = false;
		InteractionLayerManager.DisableAllLayersButOne(BaseLayer.Elements);
		Show();
	}

	private new void Show() {
		base.Show();
	}

	public void HideElement() {
		Visible = false;

		InteractionLayerManager.EnableAllLayers();
		//GameController.canPlayerInteract = true;
	}

	public override void _ExitTree() {
		GameController.onUnhandledInput -= OnUnhandledInput;
	}
}

public class ListItem {
	public string text;
	public Action onClick;

	public bool visible;

	public ListItem(string text, Action onClick) {
		this.text = text;
		this.onClick = onClick;
	}
}
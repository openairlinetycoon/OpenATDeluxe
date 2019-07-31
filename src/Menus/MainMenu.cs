using Godot;
using System;
using System.Collections.Generic;

public struct MainMenu : IBaseMenu {
	private static List<MenuItem> items = new List<MenuItem>() {
				new MenuItem(
					Tr("NewG>500"), //Main Menu:
					MenuItem.EntryType.Header),
				new MenuItem(
					"",
					MenuItem.EntryType.HeaderBar),

				new MenuItem(
					"# "+Tr("NewG>501"), // # Free Game
					MenuItem.EntryType.Link) {OnClick = ()=>{
						RoomManager.ChangeRoom("", true);
						GameController.instance.SetTaskbar(true);}},
				new MenuItem(
					Tr("NewG>515"),//Campaigns
					MenuItem.EntryType.LinkBlocked),
				new MenuItem(
					Tr("NewG>514"),//Network Game
					MenuItem.EntryType.LinkBlocked),
				new MenuItem(
					Tr("NewG>506"), //Load Game
					MenuItem.EntryType.LinkBlocked),

				new MenuItem(),

				new MenuItem(
					Tr("NewG>504"), //Home Airport
					MenuItem.EntryType.LinkBlocked),
				new MenuChangeItem(
					Tr("NewG>507"), //Options
					new SettingsMenu()),

				new MenuItem(),

				new MenuItem(
					Tr("NewG>505"), //Intro
					MenuItem.EntryType.LinkBlocked),
				new MenuItem(
					Tr("NewG>508"), //Credits
					MenuItem.EntryType.LinkBlocked),
				new MenuItem(
					Tr("NewG>509"), //Highscores
					MenuItem.EntryType.LinkBlocked),

				new MenuItem(),

				new MenuItem(
					Tr("NewG>510"), //Quit Game
					MenuItem.EntryType.Link,
					()=>MenuScene.instance.GetTree().Quit())
			};

	public List<MenuItem> GetMenuItems() {
		return items;
	}

	private static string Tr(string v) {
		return TranslationServer.Translate(v);
	}
}

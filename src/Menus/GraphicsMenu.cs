using Godot;
using System;
using System.Collections.Generic;

public class GraphicsMenu : IBaseMenu {
	private static List<MenuItem> items = new List<MenuItem>() {
				new MenuItem(
					Tr("Misc>4010"),
					MenuItem.EntryType.Header
				),
				new MenuItem(
					"",
					MenuItem.EntryType.HeaderBar
				),
				new SwitchItem(
					Tr("Misc>4012"), //Planes
					Tr("Misc>4011"),
					SettingsManager.planeGraphics
				),
				new SwitchItem(
					Tr("Misc>4014"), //PASSENGERS
					Tr("Misc>4013"),
					SettingsManager.passengerGraphics
				),
				new SwitchItem(
					Tr("Misc>4016"), //BLINDS
					Tr("Misc>4015"),
					SettingsManager.blindGraphics
				),
				new SwitchItem(
					Tr("Misc>4018"), //ROOM DETAILS
					Tr("Misc>4017"),
					SettingsManager.roomDetailsGraphics
				),
				new SwitchItem(
					Tr("Misc>4021"), //THOUGHTS
					Tr("Misc>4020"),
					SettingsManager.thoughtsGraphics
				),
				new SwitchItem(
					Tr("Misc>4022"), //SCROLLING
					Tr("Misc>4023"),
					SettingsManager.scrollingGraphics
				),
				new SwitchItem(
					Tr("Misc>4025"), //TRANSPARENCY
					Tr("Misc>4024"),
					SettingsManager.transparencyGraphics
				),
				new SwitchItem(
					Tr("Misc>4027"), //SHADOWS
					Tr("Misc>4026"),
					SettingsManager.shadowGraphics
				),
				new MenuItem(),
				new MenuChangeItem(
					Tr("Misc>4007"), //Ok
					new SettingsMenu()) {type = MenuItem.EntryType.MoveLeft}
			};
	public List<MenuItem> GetMenuItems() {
		return items;
	}

	private static string Tr(string v) {
		return TranslationServer.Translate(v);
	}
}

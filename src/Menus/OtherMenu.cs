using Godot;
using System;
using System.Collections.Generic;

public class OtherMenu : IBaseMenu {
	private static List<MenuItem> items = new List<MenuItem>() {
				new MenuItem(
					Tr("Misc>4030"),
					MenuItem.EntryType.Header
				),
				new MenuItem(
					"",
					MenuItem.EntryType.HeaderBar
				),
				new SwitchItem(
					Tr("Misc>4032"), //SECRETARY
					Tr("Misc>4031"),
					SettingsManager.secretary
				),
				new SwitchItem(
					Tr("Misc>4034"), //ADVISOR
					Tr("Misc>4033"),
					SettingsManager.advisor
				),
				new SwitchItem(
					Tr("Misc>4039"), //AUTOSAVE
					Tr("Misc>4038"),
					SettingsManager.autosave
				),
				new SwitchItem(
					Tr("Misc>4041"), //FAX
					Tr("Misc>4040"),
					SettingsManager.fax
				),
				new SwitchItem(
					Tr("Misc>4043"), //REAL NAMES
					Tr("Misc>4042"),
					SettingsManager.realNames
				),
				new SwitchItem(
					Tr("Misc>4045"), //TEXT BUBBLES
					Tr("Misc>4044"),
					SettingsManager.textBubbles
				),
				new SwitchItem(
					Tr("Misc>4047"), //SUMMARY
					Tr("Misc>4046"),
					SettingsManager.summary
				),
				new SwitchItem(
					Tr("Misc>4049"), //ROUND NUMBERS
					Tr("Misc>4048"),
					SettingsManager.roundNumbers
				),
				new SwitchItem(
					Tr("Misc>4050"), //GAME START TODAY
					Tr("Misc>4051"),
					SettingsManager.gameStartToday
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

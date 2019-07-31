using Godot;
using System;
using System.Collections.Generic;

public class SettingsMenu : IBaseMenu {
	private static List<MenuItem> items = new List<MenuItem>() {
				new MenuItem(
					Tr("Misc>4000"),
					MenuItem.EntryType.Header
				),
				new MenuItem(
					"",
					MenuItem.EntryType.HeaderBar
				),
				new MenuChangeItem(
					Tr("Misc>4001"), //Graphics
					new GraphicsMenu()),
				new MenuChangeItem(
					Tr("Misc>4002"), //Sound
					new SoundMenu()),
				new MenuChangeItem(
					Tr("Misc>4003"), //Other
					new OtherMenu()),
				new MenuItem(),
				new MenuChangeItem(
					Tr("Misc>4007"), //Ok
					new MainMenu()),
			};
	public List<MenuItem> GetMenuItems() {
		return items;
	}

	private static string Tr(string v) {
		return TranslationServer.Translate(v);
	}
}

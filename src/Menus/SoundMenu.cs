using Godot;
using System;
using System.Collections.Generic;

public class SoundMenu : IBaseMenu {
	private static List<MenuItem> items = new List<MenuItem>() {
				new MenuItem(
					Tr("Misc>4120"),
					MenuItem.EntryType.Header
				),
				new MenuItem(
					"",
					MenuItem.EntryType.HeaderBar
				),
				new MenuItem(
					Tr("Next Song"), //Midi Music On/Off
					MenuItem.EntryType.Link,()=>MusicController.NextSong()
				),
				new MenuItem(
					MenuItem.EntryType.Space
				),
				new MenuItem(
					Tr("Misc>4150"), //RANDOM SONG
					MenuItem.EntryType.Link
				),
				new MenuItem(),
				new MenuItem(),
				new MenuItem(
					Tr("Misc>4132"), //DIGI SOUND On/Off
					MenuItem.EntryType.Link
				),
				new MenuItem(),
				new MenuItem(
					Tr("Misc>4122"), //Ambience
					MenuItem.EntryType.Link
				),
				new SliderItem(
					SettingsManager.ambienceVolume.GetValue,
					SettingsManager.ambienceVolume.SetValue),
				new MenuItem(
					Tr("Misc>4123"), //Announcement
					MenuItem.EntryType.Link
				),
				new SliderItem(
					SettingsManager.announcementVolume.GetValue,
					SettingsManager.announcementVolume.SetValue),
				new MenuItem(
					Tr("Misc>4124"), //Language
					MenuItem.EntryType.Link
				),
				new SliderItem(
					SettingsManager.languageVolume.GetValue,
					SettingsManager.languageVolume.SetValue),
				new MenuItem(
					Tr("Misc>4125"), //Effects
					MenuItem.EntryType.Link
				),
				new SliderItem(
					SettingsManager.effectsVolume.GetValue,
					SettingsManager.effectsVolume.SetValue),
				new MenuItem(
					Tr("Misc>4126"), //Planes
					MenuItem.EntryType.Link
				),
				new SliderItem(
					SettingsManager.planesVolume.GetValue,
					SettingsManager.planesVolume.SetValue),
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

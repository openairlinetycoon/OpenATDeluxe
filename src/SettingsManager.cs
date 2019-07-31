using Godot;
using System;

public static class SettingsManager {
	private static string settingsPath;
	private static ConfigFile file = new ConfigFile();

	static SettingsManager() {
		settingsPath = (string)ProjectSettings.GetSetting("application/config/project_settings_override");

		file.Load(settingsPath);
	}

	private static string GetSection(string from) {
		return from.Split('/')[0];
	}

	private static string GetKey(string from) {
		return from.Substring(from.Find("/") + 1);
	}

	public static void SetSetting(string name, object value) {
		string section = GetSection(name);
		string key = GetKey(name);

		file.SetValue(section, key, value);
		file.Save(settingsPath);
	}

	public static object GetSetting(string name, object @default = null) {
		string section = GetSection(name);
		string key = GetKey(name);

		if (!file.HasSectionKey(section, key)) {
			SetSetting(name, @default);
		}
		object value = file.GetValue(section, key, @default);
		return value;
	}
}

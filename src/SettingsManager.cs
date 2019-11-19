using Godot;
using System;
using System.Linq;
using System.Reflection;

public static class SettingsManager {
	#region static values
	public static SettingsValue<int> planesVolume = new SettingsValue<int>(
					() => (int)AudioServer.GetBusVolumeDb(2),
					(vol) => { AudioServer.SetBusVolumeDb(2, vol); },
					"audio/bus2vol"
				);

	public static SettingsValue<int> effectsVolume = new SettingsValue<int>(
					() => (int)AudioServer.GetBusVolumeDb(3),
					(vol) => { AudioServer.SetBusVolumeDb(3, vol); },
					"audio/bus3vol"
				);
	public static SettingsValue<int> languageVolume = new SettingsValue<int>(
					() => (int)AudioServer.GetBusVolumeDb(4),
					(vol) => { AudioServer.SetBusVolumeDb(4, vol); },
					"audio/bus4vol"
				);
	public static SettingsValue<int> announcementVolume = new SettingsValue<int>(
					() => (int)AudioServer.GetBusVolumeDb(5),
					(vol) => { AudioServer.SetBusVolumeDb(5, vol); },
					"audio/bus5vol"
				);
	public static SettingsValue<int> ambienceVolume = new SettingsValue<int>(
					() => (int)AudioServer.GetBusVolumeDb(6),
					(vol) => { AudioServer.SetBusVolumeDb(6, vol); },
					"audio/bus6vol"
				);

	public static SettingsValue<bool> planeGraphics = new SettingsValue<bool>("graphics/planes");
	public static SettingsValue<bool> passengerGraphics = new SettingsValue<bool>("graphics/passenger");
	public static SettingsValue<bool> blindGraphics = new SettingsValue<bool>("graphics/blind");
	public static SettingsValue<bool> roomDetailsGraphics = new SettingsValue<bool>("graphics/roomDetails");
	public static SettingsValue<bool> thoughtsGraphics = new SettingsValue<bool>("graphics/thoughts");
	public static SettingsValue<bool> scrollingGraphics = new SettingsValue<bool>("graphics/scrolling");
	public static SettingsValue<bool> transparencyGraphics = new SettingsValue<bool>("graphics/transparency");
	public static SettingsValue<bool> shadowGraphics = new SettingsValue<bool>("graphics/shadow");


	public static SettingsValue<bool> secretary = new SettingsValue<bool>("other/secretary");
	public static SettingsValue<bool> advisor = new SettingsValue<bool>("other/advisor");
	public static SettingsValue<bool> autosave = new SettingsValue<bool>("other/autosave");
	public static SettingsValue<bool> fax = new SettingsValue<bool>("other/fax");
	public static SettingsValue<bool> realNames = new SettingsValue<bool>("other/realNames");
	public static SettingsValue<bool> textBubbles = new SettingsValue<bool>("other/textBubbles");
	public static SettingsValue<bool> summary = new SettingsValue<bool>("other/summary");

	public static SettingsValue<bool> roundNumbers = new SettingsValue<bool>("other/roundNumbers");
	public static SettingsValue<bool> gameStartToday = new SettingsValue<bool>("other/gameStartToday");

	#endregion
	private static string settingsPath;
	private static ConfigFile file = new ConfigFile();

	static SettingsManager() {
		settingsPath = (string)ProjectSettings.GetSetting("application/config/project_settings_override");

		file.Load(settingsPath);
	}

	public static void LoadSavedData() {
		var fields = typeof(SettingsManager).GetFields(BindingFlags.Public | BindingFlags.Static)
											.Where(f => f.FieldType.Name.BeginsWith("SettingsValue"))
											.ToArray();

		foreach (var field in fields) {
			((ISettingsValue)field.GetValue(null))?.Load();
		}
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
			return @default;
		}
		object value = file.GetValue(section, key, @default);
		return value;
	}
}

public interface ISettingsValue {
	void Load();
}

public class SettingsValue<T> : ISettingsValue {
	public Action<T> SetValue;
	public Func<T> GetValue;
	public T value;
	public string settingsName;

	public SettingsValue(string settingsName) {
		this.settingsName = settingsName;

		SetValue = (input) => { value = input; SettingsManager.SetSetting(this.settingsName, value); };
		GetValue = () => value;
	}

	public SettingsValue(Func<T> getValue, Action<T> setValue, string settingsName) {
		SetValue = setValue;
		GetValue = getValue;
		this.settingsName = settingsName;

		SetValue += (value) => { SettingsManager.SetSetting(this.settingsName, value); };
	}

	public void Load() {
		SetValue((T)SettingsManager.GetSetting(settingsName, GetDefault(typeof(T))));
	}

	public static object GetDefault(Type type) {
		if (type.IsValueType) {
			return Activator.CreateInstance(type);
		}
		return null;
	}
}

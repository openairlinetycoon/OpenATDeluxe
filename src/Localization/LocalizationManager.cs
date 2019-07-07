using Godot;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using File = System.IO.File;
using System.Text;

public class LocalizationManager {
	public static LocalizationManager instance;

	public List<LocalizationStack> stacks;

	public static void LoadLocalizationData() {
		new LocalizationManager();
	}

	public LocalizationManager() {
		instance = this;
		stacks = new List<LocalizationStack>();

		string[] locFiles = System.IO.Directory.GetFiles(GFXLibrary.pathToAirlineTycoonD + "/misc/", "*.res");

		List<LocalizationFile> files = new List<LocalizationFile>();

		foreach (string file in locFiles) {
			if (System.IO.Path.GetFileName(file) == "register.res")
				continue;
			stacks.AddRange(new LocalizationFile(file).locData);
			//File.WriteAllText(file + "s", files.Last().GetData());
		}

		AddStacksToTranslationServer();
	}

	public void AddStacksToTranslationServer() {
		List<Translation> translations = new List<Translation>();
		foreach (LocalizedString.LanguageCode lang in (LocalizedString.LanguageCode[])Enum.GetValues(typeof(LocalizedString.LanguageCode))) {
			translations.Add(new Translation() { Locale = lang.ToString() });
		}


		List<Export> exports = new List<Export>();
		foreach (LocalizationStack stack in stacks) {
			string translationName;

			foreach (var strPair in stack.strings) {
				translationName = $"{stack.name}>{strPair.Key}";

				Export e = new Export();
				exports.Add(e);
				for (int lang = 0; lang <= (int)LocalizedString.LanguageCode.de; lang++) {
					if (strPair.Value.strings.ContainsKey((LocalizedString.Language)lang)) {
						translations[lang].AddMessage(translationName, strPair.Value.strings[(LocalizedString.Language)lang]);
						e.SetData(translationName, strPair.Value.strings[(LocalizedString.Language)lang], lang);

					} else {
						GD.Print($"Translation {(LocalizedString.Language)lang} not found for {translationName}!");
					}
				}
			}

		}

		// Use something like CsvHelper or write your own library.
		// using (var writer = new StreamWriter(ProjectSettings.GlobalizePath("res://localization.csv")))
		// using (var csv = new CsvWriter(writer)) {

		// 	csv.WriteRecords(exports);
		// }

		foreach (Translation t in translations) {
			TranslationServer.AddTranslation(t);
		}
	}

	public class Export {
		private string _id;
		private string _de; //German
		private string _pt_BR;
		private string _en;
		private string _fr;
		private string _it;
		private string _nl;
		private string _pt;
		private string _es;

		public string id { get => _id; set => _id = value; }
		public string pt_BR { get => _pt_BR; set => _pt_BR = value; }
		public string en { get => _en; set => _en = value; }
		public string fr { get => _fr; set => _fr = value; }
		public string it { get => _it; set => _it = value; }
		public string nl { get => _nl; set => _nl = value; }
		public string pt { get => _pt; set => _pt = value; }
		public string es { get => _es; set => _es = value; }
		public string de { get => _de; set => _de = value; }

		public void SetData(string id, string data, int language) {
			this._id = id;
			switch (language) {
				case (0):
					_pt_BR = data;
					break;
				case (1):
					_en = data;
					break;
				case (2):
					_fr = data;
					break;
				case (3):
					_it = data;
					break;
				case (4):
					_nl = data;
					break;
				case (5):
					_pt = data;
					break;
				case (6):
					_es = data;
					break;
				case (7):
					_de = data;
					break;
			}
		}
	}

	class LocalizationFile : BaseFileDecoder {
		public LocalizationFile(string _filePath) : base(_filePath) {
			locData = new List<LocalizationStack>();

			LocalizationStack currentStack = null;
			LocalizedString currentString = null;
			foreach (string lineMessy in fileData.Split('\n')) {
				//Order of "if"s is important! A ">" string can be in a ">>" string but not vice verca!
				string line = lineMessy.TrimEnd('\r');

				if (line.BeginsWith("//")) //Skip comments
					continue;
				if (line.Length < 2) //Skip empty lines
					continue;
				if (line.BeginsWith("  x")) { //Missing translation
					currentStack.RemoveString(currentString.id);
					continue;
				}

				if (line.BeginsWith(">>")) { //New string!
					string idS = line.Substring(2);
					idS = idS.Split(' ', '/')[0];
					int.TryParse(idS, out int id);

					bool overrideString = true;
					if (currentStack.strings.ContainsKey(id)) {
						if (!overrideString) {
							GD.Print($"Localized String already in collection. NOT overriding! id: {id} stack: {currentStack.name}");
							continue;
						} else {
							GD.Print($"Localized String already in collection. Overriding now! id: {id} stack: {currentStack.name}");
							currentStack.strings.Remove(id);
						}
					}

					currentString = new LocalizedString(id);
					currentStack.AddString(id, currentString);
					continue;
				}

				if (line.BeginsWith(">")) { //New stack!
					currentStack = new LocalizationStack(line.Substring(1));
					locData.Add(currentStack);
					continue;
				}

				if (line.BeginsWith("  ")) { //New string entry!
					currentString.AddString(line);
				}
			}
		}

		public List<LocalizationStack> locData;
	}


}

public class LocalizationStack {
	public string name;

	public Dictionary<int, LocalizedString> strings;

	public LocalizationStack(string name) {
		this.name = name;
		strings = new Dictionary<int, LocalizedString>();
	}

	public void AddString(int id, LocalizedString str) {
		strings.Add(id, str);
	}
	public void RemoveString(int id) {
		strings.Remove(id);
	}

	public LocalizationStack(string name, Dictionary<int, LocalizedString> strings) {
		this.name = name;
		this.strings = strings;
	}
}
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class LocalizedString {
	public int id;
	public Dictionary<Language, string> strings;

	public LocalizedString(int id) {
		this.id = id;
		strings = new Dictionary<Language, string>();
	}

	public void AddString(string data) {
		//  B::**DATA**
		data = data.Trim(' ');
		//B::**DATA**

		data = data.Split("//").FirstOrDefault();

		if (data == null || data.Length == 0 || data == " ") {
			GD.Print("Error! Data empty");
			return;
		}

		if (data.Substring(1).BeginsWith("::")) { //Data can be for every language!
			Enum.TryParse(data.Substr(0, 1), out Language l);
			//B

			strings.Add(l, data.Substring(3));
		} else {
			for (int l = 0; l <= (int)Language.D; l++) {
				strings.Add((Language)l, data);
			}
		}
		//**DATA**
	}

	public enum Language {
		B, //Portugese
		E, //English
		F, //French
		I, //Italian
		N, //Dutch
		O, //Portugese (Again?)
		S, //Spanish
		D, //German
	}
	public enum LanguageCode { //Tied to Language int - Watchout when reordering!
		pt_BR = Language.B, //Portugese
		en = Language.E, //English
		fr = Language.F, //French
		it = Language.I, //Italian
		nl = Language.N, //Dutch
		pt = Language.O, //Portugese (Again?)
		es = Language.S, //Spanish
		de = Language.D, //German
	}
}

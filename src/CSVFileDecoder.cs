using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using File = Godot.File;

public class CSVFileDecoder : BaseFileDecoder {
	public CSVFileDecoder(string _filePath) : base(_filePath) {

	}

	public Dictionary<string, List<string>> GetCSVDictionary() {
		Dictionary<string, List<string>> csv = new Dictionary<string, List<string>>();

		using (var reader = new StringReader(fileData)) {
			string header = reader.ReadLine();
			var headerValues = header.Split(';');

			foreach (string v in headerValues) {
				csv.Add(v, new List<string>());
			}

			reader.ReadLine();//Empty Line


			List<string> listA = new List<string>();
			List<string> listB = new List<string>();
			while (!(reader.Peek() > -1)) { //Go until there is no more
				var values = reader.ReadLine().Split(';');//Content

				if (values.Length == 0)
					continue; //Empty line!

				// Not needed here, but it will be needed later - saved for now
				// if (fileName.Contains(' ') || fileName.Contains(':')) {
				// 	//We can't "really" process ranges yet! TODO
				// 	//a space indicates another sprite
				// 	//a : indicates a repetition of the preceeding sprite

				// 	fileName = fileName.Split(' ', ':').First();
				// }

				for (int i = 0; i < values.Length; i++) {
					csv[csv.Keys.ElementAt(i)].Add(values[i]); //Add the value to the corresponding csv part.
				}
			}
		}

		return csv;
	}

}
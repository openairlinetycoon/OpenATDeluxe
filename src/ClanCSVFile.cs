using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ClanCSVFile : CSVFileDecoder {
	public static ClanCSVFile instance;
	Dictionary<int /* type */, Dictionary<string, AnimationData>> animations; //TODO: PLEASE HELP ME! THIS IS TORTUE TO LOOK AT!

	public AnimationData GetAnimationData(int type, string name) {
		if (name == "")
			return animations[type].First().Value;

		return animations[type][name];
	}

	public ClanCSVFile(string _filePath) : base(_filePath) {
		if (instance != null)
			throw (new NotSupportedException("ClanCSVFile is a singelton type class!"));

		instance = this;

		LoadCSVData();
	}

	private void LoadCSVData() {
		animations = new Dictionary<int /* type */, Dictionary<string, AnimationData>>();

		using (var reader = new System.IO.StringReader(fileData)) {
			string header = reader.ReadLine();
			var headerValues = header.Split(';');

			while ((reader.Peek() > -1)) { //Go until there is no more
				var values = reader.ReadLine().Split(';');//Content

				if (values.Length == 0)
					continue; //Empty line!
				if (values[0] == "-")
					continue;

				AnimationData anim = new AnimationData();

				int id = Convert.ToInt32(values[0]);
				string name = values[2];

				anim.speed = Convert.ToInt32(values[5]);

				bool hasLuggage = Convert.ToInt32(values[7]) > 0;
				if (hasLuggage)
					name = name + "L";
				//Texture names for walking and running begin at index 13 - "Direction;...;Amount" repeat until 34
				int startingPoint = 13;

				for (int i = 0; i <= 7 * 3; i += 3) { //Loops through all cardinal directions of animation both for running and walking
					anim.AddSprites(
						firstTextureName: values[startingPoint + i + 1],
						amount: Convert.ToInt32(values[startingPoint + i + 2]),
						state: (AnimationState)(i / 3)
						);
				}

				string[] standingPoses = values[45].Split(' ');

				if (standingPoses.Length >= 4) {
					anim.AddSprite(
					   textureName: standingPoses[0],
					   state: AnimationState.NStanding
					   );
					anim.AddSprite(
						textureName: standingPoses[1],
						state: AnimationState.EStanding
						);
					anim.AddSprite(
						textureName: standingPoses[2],
						state: AnimationState.SStanding
						);
					anim.AddSprite(
						textureName: standingPoses[3],
						state: AnimationState.WStanding
						);
				}

				if (!animations.ContainsKey(id))
					animations.Add(id, new Dictionary<string, AnimationData>());

				if (animations[id].ContainsKey(name))
					name = name + Guid.NewGuid();//It's probably a piece of luggage, we only use the IDs for those!
				animations[id].Add(name, anim);
			}
		}
	}
}
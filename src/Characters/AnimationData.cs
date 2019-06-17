using System;
using System.Collections.Generic;
using Godot;

public class AnimationData {
	public string name;
	public Dictionary<AnimationState, List<Texture>> textures;

	public int speed;

	private const string PathToClanFolder = "res://Images/gli/glclan/";

	public AnimationData() {
		textures = new Dictionary<AnimationState, List<Texture>>();
	}

	public void AddSprites(string firstTextureName, int amount, AnimationState state) {
		string textureName = firstTextureName;

		if (firstTextureName.Contains("0"))
			textureName = firstTextureName.Substr(0, firstTextureName.Length - 2); //Remove the base numbers, we create our own

		File f = new File();
		if (!f.FileExists(PathToClanFolder + firstTextureName + ".res"))
			return;

		List<Texture> textureData = new List<Texture>();

		if (amount == -1) {
			textureData.Add((Texture)ResourceLoader.Load(PathToClanFolder + firstTextureName + ".res"));
		}

		for (int i = 0; i < amount; i++) {
			string p = PathToClanFolder + textureName + ((i + 1) < 10 ? "0" : "") + (i + 1) + ".res";

			if (!f.FileExists(p))
				continue;
			textureData.Add((Texture)ResourceLoader.Load(p));
		}

		textures.Add(state, textureData);
	}

	public void AddSprite(string textureName, AnimationState state) {
		AddSprites(textureName, -1, state);
	}

	public AnimationData(string name, Dictionary<AnimationState, List<Texture>> sprites) {
		this.name = name;
		this.textures = sprites;
	}
}

public enum AnimationState {
	//Walking
	N,
	E,
	S,
	W,
	//Running
	NR,
	ER,
	SR,
	WR,
	//Others
	Gimmick,
	Winken,
	//Sitting
	NSi,
	SSi,
	//Standing
	NStanding,
	EStanding,
	SStanding,
	WStanding,
}
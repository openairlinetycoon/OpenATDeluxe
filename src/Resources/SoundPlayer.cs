using Godot;
using System;
using File = System.IO.File;

//TODO: Add pooling system to minimize GC

public class SoundPlayer : AudioStreamPlayer {
	public readonly static string SoundPath = GFXLibrary.pathToAirlineTycoonD;

	[Export]
	bool loop;

	[Export]
	public string filePath;
	AudioStreamSample audioFile = new AudioStreamSample();

	public static SoundPlayer CreatePlayer(string file, string bus) {
		SoundPlayer player = new SoundPlayer();
		if (!File.Exists(SoundPath + file)) {
			throw (new System.IO.FileNotFoundException($"File: {SoundPath + file} not found!"));
		}

		player.SetAudioStream(file);
		player.Bus = bus;

		player.filePath = file;

		return player;
	}

	private void SetAudioStream(string file) {
		byte[] data = File.ReadAllBytes(SoundPath + file);
		audioFile.SetData(data);
		audioFile.MixRate = 44100;

		Stream = audioFile;
	}

	override public void _Ready() {
		if (Stream == null) {
			SetAudioStream(filePath);
		}
	}
}

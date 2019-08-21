using Godot;
using System;
using File = System.IO.File;

//TODO: Add pooling system to minimize GC

public class SoundPlayer : AudioStreamPlayer {
	public readonly static string SoundPath = GFXLibrary.pathToAirlineTycoonD;

	[Export]
	public bool loop, use8BitEncoding;

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
		if (use8BitEncoding) {
			audioFile.MixRate = 44100;
		} else {
			audioFile.MixRate = 22050;
			audioFile.Format = AudioStreamSample.FormatEnum.Format16Bits;
		}

		Stream = audioFile;
	}

	override public void _Ready() {
		if (Stream == null) { //Created inside the editor -> We need to initialize with editor values
			if (!File.Exists(SoundPath + filePath)) {
				GD.PrintErr($"File: {SoundPath + filePath} not found!");
				return;
			}

			SetAudioStream(filePath);
			if (Autoplay) //Autoplay does not work natively, because there is was Audio Stream to play before we added it here
				Play();
		}
	}
}

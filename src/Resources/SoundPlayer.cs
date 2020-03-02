using Godot;
using System;
using File = System.IO.File;

//TODO: Add pooling system to minimize GC

public class SoundPlayer : AudioStreamPlayer {
	public readonly static string SoundPath = GFXLibrary.pathToAirlineTycoonD;

	public Action OnSoundFinished;

	bool started;

	[Export]
	public bool loop, use8BitEncoding;

	[Export]
	public string filePath;
	AudioStreamSample audioFile = new AudioStreamSample();
	ushort[] _shortData;
	ushort[] shortData {
		get {
			if (_shortData == null) {
				byte[] data = audioFile.GetData();

				var size = data.Length / sizeof(UInt16);

				_shortData = new UInt16[size];
				for (var index = 0; index < size; index++) {
					shortData[index] = BitConverter.ToUInt16(data, index * sizeof(UInt16));
				}
			}

			return _shortData;
		}
	}

	public static SoundPlayer CreatePlayer(string file, string bus, bool use8BitEncoding = false, bool oneShot = false) {
		SoundPlayer player = new SoundPlayer();
		if (!File.Exists(SoundPath + file)) {
			throw (new System.IO.FileNotFoundException($"File: {SoundPath + file} not found!"));
		}

		player.use8BitEncoding = use8BitEncoding;

		player.SetAudioStream(file);
		player.Bus = bus;

		player.filePath = file;

		if (oneShot) {
			player.OnSoundFinished += () => player.QueueFree();
		}

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

	new public void Play(int fromPosition = 0) {
		base.Play(fromPosition);

		started = true;
	}

	override public void _Ready() {
		if (Stream == null) { //Created inside the editor -> We need to initialize with editor values
			if (!File.Exists(SoundPath + filePath)) {
				GD.PrintErr($"File: {SoundPath + filePath} not found!");
				return;
			}

			SetAudioStream(filePath);
			if (Autoplay) //Autoplay does not work natively, because there was no Audio Stream to play before we added it here
				Play();
		}
	}

	override public void _Process(float delta) {
		if (!IsPlaying() && started) {
			started = false;
			OnSoundFinished?.Invoke();

			if (loop) {
				started = true;
				Play();
			}
		}
	}

	public bool IsTalking() {
		int pos = (22050 * (int)(GetPlaybackPosition() * 1000)) / 1000 + 400;
		if (pos + 1000 > shortData.Length)
			return false;

		const int audioThreshold = 512;
		return shortData[pos] > audioThreshold
		|| shortData[pos + 100] > audioThreshold
		|| shortData[pos + 200] > audioThreshold
		|| shortData[pos + 400] > audioThreshold
		|| shortData[pos + 560] > audioThreshold
		|| shortData[pos + 620] > audioThreshold
		|| shortData[pos + 700] > audioThreshold
		|| shortData[pos + 800] > audioThreshold
		|| shortData[pos + 900] > audioThreshold
		|| shortData[pos + 999] > audioThreshold;
	}
}

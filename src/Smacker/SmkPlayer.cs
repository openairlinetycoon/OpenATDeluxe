using Godot;
using System;

public class SmkPlayer : Sprite {
	[Export]
	public string fileName;

	ImageTexture[] buffer;

	float fps;
	float timeDelta;
	float currentTimeDelta;
	int currentFrame = 0;

	SmackerFile file;
	SmackerDecoder decoder;

	/// <summary>
	/// The decoder needs to go through every frame to fully populate a palette.
	/// the sequence would be heavily fragmented in the first run, if we don't do this.
	/// </summary>
	private void PrewarmDecoder() {
		int frames = (int)file.Header.NbFrames;

		for (int i = 0; i < frames; i++) {
			decoder.ReadNextFrame();
		}
		decoder.Reset();

	}

	/// <summary>
	/// Save every frame to a image texture and buffer it.
	/// </summary>
	private void BufferImages() {
		int frames = (int)file.Header.NbFrames;
		buffer = new ImageTexture[frames];

		for (int i = 0; i < frames; i++) {
			decoder.ReadNextFrame();

			buffer[i] = PrepareImageTexture();
		}
	}

	private ImageTexture PrepareImageTexture() {

		Image image = new Image();


		image.CreateFromData((int)file.Header.Width, (int)file.Header.Height, false, Image.Format.Rgba8, decoder.RGBAData);
		image.PremultiplyAlpha();
		ImageTexture t = new ImageTexture();
		t.CreateFromImage(image);
		return t;
	}

	private void LoadSmacker() {
		file = SmackerFile.OpenFromStream(new System.IO.FileStream(GFXLibrary.pathToAirlineTycoonD + "/video/" + fileName, System.IO.FileMode.Open));
		decoder = file.Decoder;
		fps = (float)file.Header.Fps;
		timeDelta = 1 / fps;
	}

	public override void _Ready() {
		LoadSmacker();
		PrewarmDecoder();
		BufferImages();
	}

	public override void _Process(float delta) {
		currentTimeDelta += delta;
		if (currentTimeDelta > timeDelta) {
			currentTimeDelta = 0;

			currentFrame++;
			currentFrame = Mathf.Wrap(currentFrame, 0, buffer.Length);

			Texture = buffer[currentFrame];
		}
	}
}

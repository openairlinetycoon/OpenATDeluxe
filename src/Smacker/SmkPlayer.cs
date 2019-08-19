using Godot;
using System;

public class SmkPlayer : Sprite {
	[Export]
	public string fileName;

	public Action OnPlay, OnPause, OnAnimationFinish;

	public bool isPlaying = true;

	ImageTexture[] buffer;

	float fps;
	float timeDelta;
	float currentTimeDelta;
	int currentFrame = 0;

	public static SmkPlayer CreateSmacker(Node parent, string name, string folder = "video/") {
		SmkPlayer p = new SmkPlayer();
		p.Centered = false;
		p.UseParentMaterial = true;
		p.fileName = folder + name;
		parent.AddChild(p, true);
		return p;
	}


	public static SmkPlayer CreateSmacker(Node parent, string name, Vector2 position, string folder = "/video/") {
		SmkPlayer p = CreateSmacker(parent, name, folder);
		p.Position = position;
		return p;
	}

	public static SmkPlayer CreateSmacker(Node parent, string name, Vector2 position, bool isPlaying, string folder = "/video/") {
		SmkPlayer p = CreateSmacker(parent, name, position, folder);
		p.isPlaying = isPlaying;
		return p;
	}
	public static SmkPlayer CreateSmacker(Node parent, string name, Vector2 position, bool isPlaying, bool isVisible, string folder = "/video/") {
		SmkPlayer p = CreateSmacker(parent, name, position, isPlaying, folder);
		p.Visible = isVisible;
		return p;
	}


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
	/// Saves us GC
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
		file = SmackerFile.OpenFromStream(new System.IO.FileStream(GFXLibrary.pathToAirlineTycoonD + fileName, System.IO.FileMode.Open));
		decoder = file.Decoder;
		fps = (float)file.Header.Fps;
		timeDelta = 1 / fps;
	}

	public override void _Ready() {
		LoadSmacker();
		PrewarmDecoder();
		BufferImages();
	}

	public void Play() {
		OnPlay?.Invoke();
		isPlaying = true;
		Visible = true;

		currentFrame = 0;
		Texture = buffer[currentFrame];
	}
	public void Pause() {
		OnPause?.Invoke();
		isPlaying = false;
	}
	new public void Hide() {
		isPlaying = false;
		Visible = false;
	}

	public override void _Process(float delta) {
		if (!isPlaying || !Visible)
			return;

		currentTimeDelta += delta;
		if (currentTimeDelta > timeDelta) {
			currentTimeDelta = 0;

			currentFrame++;

			if (currentFrame == buffer.Length)
				OnAnimationFinish?.Invoke();

			currentFrame = Mathf.Wrap(currentFrame, 0, buffer.Length);
			Texture = buffer[currentFrame];
		}
	}


}

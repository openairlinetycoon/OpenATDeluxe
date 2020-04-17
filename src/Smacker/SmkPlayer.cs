using Godot;
using System;
using System.IO;

public class SmkPlayer : Sprite {
	[Export]
	public string fileName;

	public Action OnPlay, OnPause, OnAnimationFinish;

	public bool isPlaying = true;

	protected ImageTexture[] buffer;

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
	protected void PrewarmDecoder() {
		int frames = (int)file.Header.NbFrames;
		buffer = new ImageTexture[frames];

		for (int i = 0; i < frames; i++) {
			decoder.ReadNextFrame(true);
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

	protected void PrepareFrameImage() {
		decoder.ReadNextFrame();
		buffer[currentFrame] = PrepareImageTexture();
	}

	private ImageTexture PrepareImageTexture() {

		Image image = new Image();


		image.CreateFromData((int)file.Header.Width, (int)file.Header.Height, false, Image.Format.Rgba8, decoder.RGBAData);
		image.PremultiplyAlpha();
		ImageTexture t = new ImageTexture();
		t.CreateFromImage(image);
		return t;
	}

	protected Stream fileStream;
	protected void LoadSmacker(bool loadFileToMemory = true) {
		string filePath = GFXLibrary.pathToAirlineTycoonD + fileName;
		if (!System.IO.File.Exists(filePath)) {
			filePath = ATFile.FindFile(System.IO.Path.GetFileName(fileName));
		}

		if (loadFileToMemory) {
			//These files are not that big, so we can just load them into memory, to free the file
			FileStream temp = new FileStream(filePath, System.IO.FileMode.Open);
			using (temp) {
				//create new MemoryStream object
				fileStream = new MemoryStream();
				fileStream.SetLength(temp.Length);
				//read file to MemoryStream
				temp.Read(((MemoryStream)fileStream).GetBuffer(), 0, (int)temp.Length);
			}
		} else {
			fileStream = new FileStream(filePath, System.IO.FileMode.Open);
		}
		file = SmackerFile.OpenFromStream(fileStream);
		decoder = file.Decoder;
		fps = (float)file.Header.Fps;
		timeDelta = 1 / fps;
	}

	public override void _Ready() {
		base._Ready();
		LoadSmacker();
		PrewarmDecoder();
		//BufferImages();
	}

	override public void _ExitTree() {
		fileStream.Close();
	}

	public void Play() {
		OnPlay?.Invoke();
		isPlaying = true;
		Visible = true;

		currentFrame = 0;
		if (buffer[currentFrame] == null)
			PrepareFrameImage();
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

	public int GetWidth() {
		if (buffer[currentFrame] == null)
			PrepareFrameImage();
		return buffer[currentFrame].GetWidth();
	}
	public int GetHeight() {
		if (buffer[currentFrame] == null)
			PrepareFrameImage();
		return buffer[currentFrame].GetHeight();
	}

	public override void _Process(float delta) {
		base._Process(delta);

		if (!isPlaying || !Visible)
			return;

		currentTimeDelta += delta;
		if (currentTimeDelta > timeDelta) {
			currentTimeDelta = 0;
			currentFrame++;

			if (currentFrame == buffer.Length)
				OnAnimationFinish?.Invoke();

			currentFrame = Mathf.Wrap(currentFrame, 0, buffer.Length);


			if (buffer[currentFrame] == null)
				PrepareFrameImage();

			Texture = buffer[currentFrame];

		}
	}
}




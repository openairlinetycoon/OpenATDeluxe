using Godot;
using System;
using System.Collections.Generic;

public class MusicController : Node {
	public static MusicController instance;
	public static Song[] musicFiles;
	public static bool isOgg;
	int currentSong = 7;

	public static bool isInMainMenu;

	[Export]
	public NodePath _midiPlayer;
	Node midiPlayer;

	[Export]
	public NodePath _oggPlayer;
	AudioStreamPlayer oggPlayer;

	public bool IsMidiPlaying {
		get {
			return (bool)midiPlayer.Get("playing");
		}
	}
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		instance = this;

		if (musicFiles == null || musicFiles?.Length == 0) {
			SetProcess(false);
			return;
		}
		midiPlayer = GetNode(_midiPlayer);
		oggPlayer = (AudioStreamPlayer)GetNode(_oggPlayer);
	}

	//  Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta) {
		if (HasSongFinished()) {
			NextSong();
			PlaySong();
		}
	}

	private void PlaySong() {
		string song = musicFiles[currentSong].name;
		if ((song == "title" || song == "at2") && isInMainMenu == false) //is main menu song?
			NextSong();

		song = musicFiles[currentSong].name;
		Song.SongTypes type = musicFiles[currentSong].type;

		if (type == Song.SongTypes.Ogg) {
			oggPlayer.SetStream(musicFiles[currentSong].oggData);
			oggPlayer.Play();
		} else if (type == Song.SongTypes.Mid) {
			midiPlayer.Call("set_file", musicFiles[currentSong]);
			midiPlayer.Call("play", 0);
		}
	}

	public bool HasSongFinished() {
		if (isOgg) {
			return !oggPlayer.Playing;
		} else {
			return !IsMidiPlaying;
		}
	}

	public void SetSong(string name) {
		for (int i = 0; i < musicFiles.Length; i++) {
			if (musicFiles[i].name == name) {
				currentSong = i;
				PlaySong();
			}

		}
	}

	public void NextSong() {
		currentSong++;

		if (currentSong >= musicFiles.Length) {
			currentSong = 0;
		}
	}
}

public class Song {
	public string path, name;

	public enum SongTypes {
		Ogg,
		Mid
	}

	public SongTypes type;

	public AudioStreamOGGVorbis oggData;

	public Song(string path, SongTypes songType) {
		this.path = path;
		type = songType;


		name = System.IO.Path.GetFileNameWithoutExtension(path);
		LoadSong();
	}

	void LoadSong() {
		switch (type) {
			case (SongTypes.Ogg):
				CreateOggResource();
				break;
			case (SongTypes.Mid):
				//Do nothing
				break;
		}
	}

	void CreateOggResource() {
		oggData = new AudioStreamOGGVorbis();
		oggData.SetData(System.IO.File.ReadAllBytes(path));
	}

	public static Song[] CreateFromFiles(string[] files, SongTypes songType) {
		List<Song> songs = new List<Song>();
		foreach (string f in files) {
			songs.Add(new Song(f, songType));
		}

		return songs.ToArray();
	}
}
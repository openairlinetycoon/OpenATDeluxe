using System;
using System.Runtime;
using System.Linq;
using Godot;

public class MidiPlayer : Node {
	public bool IsPlaying {
		get {
			if (!isLoaded)
				return false;

			return !(FluidSynthWrapper.PlayerGetStatus(player) != 1);
		}
	}

	static IntPtr settings;
	static IntPtr synth;
	static IntPtr player;
	static IntPtr adriver;
	static IntPtr soundfont;

	static bool isLoaded = false;

	override public void _Ready() {
		// settings = FluidSynthWrapper.NewSettings();
		// synth = FluidSynthWrapper.NewSynth(settings);
		// player = FluidSynthWrapper.NewPlayer(synth);
		// adriver = FluidSynthWrapper.NewAudioDriver(settings, synth);

		// GD.Print(result);
		//result = SetFile("P:/Projekte/Major Games/OpenATD/src/Midi/funky2.mid");
		//GD.Print(result);
	}

	private void Unload() {
		FluidSynthWrapper.PlayerStop(player);
		FluidSynthWrapper.PlayerJoin(player);

		FluidSynthWrapper.SynthRemoveSFFromStack(synth, soundfont);

		FluidSynthWrapper.DeleteAudioDriver(adriver);
		FluidSynthWrapper.DeletePlayer(player);
		FluidSynthWrapper.DeleteSynth(synth);
	}

	static public void Load() {
		if (!isLoaded)
			settings = FluidSynthWrapper.NewSettings();
		synth = FluidSynthWrapper.NewSynth(settings);
		player = FluidSynthWrapper.NewPlayer(synth);

		if (!isLoaded) {
			int result = FluidSynthWrapper.SynthSFLoad(synth, "P:/Projekte/Major Games/OpenATD/src/Midi/Saphyr.sf2", 1);
			soundfont = FluidSynthWrapper.SynthGetSF(synth, 0);
		} else {
			FluidSynthWrapper.SynthAddSF(synth, soundfont);
			FluidSynthWrapper.SynthReset(synth);
		}

		isLoaded = true;
	}

	private void PreparePlayback() {
		adriver = FluidSynthWrapper.NewAudioDriver(settings, synth);
	}

	public void Play(string file) {
		if (isLoaded)
			Unload();
		Load();
		FluidSynthWrapper.PlayerAddFile(player, file);
		PreparePlayback();
		FluidSynthWrapper.PlayerPlay(player);
	}

	override public void _ExitTree() {
		FluidSynthWrapper.PlayerStop(player);
		FluidSynthWrapper.PlayerJoin(player);

		FluidSynthWrapper.DeleteAudioDriver(adriver);
		FluidSynthWrapper.DeletePlayer(player);
		FluidSynthWrapper.DeleteSynth(synth);
		FluidSynthWrapper.DeleteSettings(settings);
	}
}
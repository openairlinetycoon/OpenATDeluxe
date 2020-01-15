using System;
using System.Runtime.InteropServices;

internal class FluidSynthWrapper {
	#region constructor
	[DllImport("libfluidsynth-2.dll", EntryPoint = "new_fluid_settings")]
	public static extern IntPtr NewSettings();

	[DllImport("libfluidsynth-2.dll", EntryPoint = "new_fluid_synth")]
	public static extern IntPtr NewSynth(IntPtr settings);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "new_fluid_player")]
	public static extern IntPtr NewPlayer(IntPtr synth);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "new_fluid_audio_driver")]
	public static extern IntPtr NewAudioDriver(IntPtr settings, IntPtr synth);
	#endregion
	#region destructor
	[DllImport("libfluidsynth-2.dll", EntryPoint = "delete_fluid_settings")]
	public static extern IntPtr DeleteSettings(IntPtr settings);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "delete_fluid_synth")]
	public static extern IntPtr DeleteSynth(IntPtr synth);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "delete_fluid_player")]
	public static extern IntPtr DeletePlayer(IntPtr player);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "delete_fluid_audio_driver")]
	public static extern IntPtr DeleteAudioDriver(IntPtr audioDriver);
	#endregion

	[DllImport("libfluidsynth-2.dll", EntryPoint = "fluid_synth_program_reset")]
	public static extern int SynthReset(IntPtr synth);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "fluid_synth_sfload")]
	public static extern int SynthSFLoad(IntPtr synth, [MarshalAs(UnmanagedType.LPStr)] string filename, int resetPreset);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "fluid_synth_get_sfont")]
	public static extern IntPtr SynthGetSF(IntPtr synth, uint index);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "fluid_synth_remove_sfont")]
	public static extern IntPtr SynthRemoveSFFromStack(IntPtr synth, IntPtr sf);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "fluid_synth_add_sfont")]
	public static extern int SynthAddSF(IntPtr synth, IntPtr sf);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "fluid_player_add")]
	public static extern int PlayerAddFile(IntPtr player, [MarshalAs(UnmanagedType.LPStr)] string midifile);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "fluid_player_play")]
	public static extern int PlayerPlay(IntPtr player);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "fluid_player_stop")]
	public static extern int PlayerStop(IntPtr player);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "fluid_player_join")]
	public static extern int PlayerJoin(IntPtr player);

	[DllImport("libfluidsynth-2.dll", EntryPoint = "fluid_player_get_status")]
	public static extern int PlayerGetStatus(IntPtr player);
}
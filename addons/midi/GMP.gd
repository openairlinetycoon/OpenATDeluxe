@tool
"""
	Godot MIDI Player Plugin by arlez80 (Yui Kinomoto)
"""

extends EditorPlugin

func _enter_tree( ):
	self.add_custom_type( "GodotMIDIPlayer", "AudioStreamPlayer", preload("MidiPlayer.gd"), preload("icon.png") )

func _exit_tree( ):
	self.remove_custom_type( "GodotMIDIPlayer" )

func _has_main_screen():
	return true

func _make_visible( visible:bool ):
	pass

func _get_plugin_name( ):
	return "Godot MIDI Player"
